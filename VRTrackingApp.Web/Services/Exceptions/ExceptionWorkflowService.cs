using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.MSRC;
using VRTrackingApp.Web.Services.MSRC.Models;
using VRTrackingApp.Web.Services.Notifications;

namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>Outcome of a workflow decision applied via <see cref="ExceptionWorkflowService"/>.</summary>
public enum WorkflowResult
{
    NoOp,
    Advanced,
    Approved,
    Rejected,
    NeedMoreInfo
}

/// <summary>
/// Owns the exception approval state machine (P2). Stateless: it mutates the
/// supplied <see cref="ExceptionRecord"/> (which the caller keeps tracked by its
/// <c>DbContext</c>), so callers are responsible for <c>SaveChangesAsync()</c>.
/// </summary>
public class ExceptionWorkflowService
{
    private readonly INotificationService? _notify;
    private readonly IVulnerabilityEnrichmentService? _enrichment;

    /// <summary>
    /// Initialise the approval chain for a freshly requested exception: records the
    /// resolved stage-1 role, marks it <see cref="ExceptionStatus.PendingTechnicalApproval"/>
    /// and creates the three ordered approval steps (Technical → Manager → Security).
    /// </summary>
    public ExceptionWorkflowService(INotificationService? notify = null, IVulnerabilityEnrichmentService? enrichment = null)
    {
        _notify = notify;
        _enrichment = enrichment;
    }

    public void StartApproval(ExceptionRecord ex, string stage1Role)
    {
        ex.Stage1Role = stage1Role;
        ex.CurrentApprovalStage = ApprovalStage.Technical;
        ex.Status = ExceptionStatus.PendingTechnicalApproval;
        if (!ex.SubmittedAt.HasValue) ex.SubmittedAt = DateTime.UtcNow;

        ex.ApprovalSteps ??= new List<ExceptionApprovalStep>();
        if (ex.ApprovalSteps.Count == 0)
        {
            ex.ApprovalSteps.Add(new ExceptionApprovalStep { StepOrder = 1, Stage = ApprovalStage.Technical, RequiredRole = stage1Role, Decision = ApprovalDecision.Pending });
            ex.ApprovalSteps.Add(new ExceptionApprovalStep { StepOrder = 2, Stage = ApprovalStage.Manager, RequiredRole = AppRoles.RiskCommittee, Decision = ApprovalDecision.Pending });
            ex.ApprovalSteps.Add(new ExceptionApprovalStep { StepOrder = 3, Stage = ApprovalStage.Security, RequiredRole = AppRoles.Ciso, Decision = ApprovalDecision.Pending });
        }
    }

    /// <summary>
    /// Populate vendor response from MSRC data if the finding has a CVE.
    /// This can be called when creating a vendor response or during exception review.
    /// </summary>
    public async Task<VendorResponse?> PopulateVendorResponseFromMSRCAsync(ExceptionRecord ex, CancellationToken ct = default)
    {
        if (_enrichment == null || ex.VulnerabilityInstance?.VulnerabilityFinding?.Cve == null)
            return null;

        var cve = ex.VulnerabilityInstance.VulnerabilityFinding.Cve;
        var enrichment = await _enrichment.EnrichByCVEAsync(cve, ct);

        if (enrichment == null)
            return null;

        var vendorResponse = new VendorResponse
        {
            ExceptionRecordId = ex.Id,
            Vendor = "Microsoft",
            ResponseText = BuildVendorResponseText(enrichment),
            PatchEtaDate = enrichment.MicrosoftReleaseDate?.DateTime ?? enrichment.MicrosoftReleaseDate?.DateTime,
            ReceivedAt = DateTime.UtcNow
        };

        ex.VendorResponses ??= new List<VendorResponse>();
        ex.VendorResponses.Add(vendorResponse);

        return vendorResponse;
    }

    private static string BuildVendorResponseText(MSRCEnrichmentData enrichment)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(enrichment.MicrosoftAdvisoryId))
            parts.Add($"Microsoft Advisory: {enrichment.MicrosoftAdvisoryId}");

        if (!string.IsNullOrEmpty(enrichment.MicrosoftBulletinId))
            parts.Add($"Bulletin: {enrichment.MicrosoftBulletinId}");

        if (enrichment.KBNumbers?.Length > 0)
            parts.Add($"KB Articles: {string.Join(", ", enrichment.KBNumbers)}");

        if (!string.IsNullOrEmpty(enrichment.ExploitabilityAssessment))
            parts.Add($"Exploitability: {enrichment.ExploitabilityAssessment}");

        if (enrichment.MicrosoftReleaseDate.HasValue)
            parts.Add($"Patch Release Date: {enrichment.MicrosoftReleaseDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrEmpty(enrichment.Workaround))
            parts.Add($"Workaround: {enrichment.Workaround}");

        if (!string.IsNullOrEmpty(enrichment.FAQUrl))
            parts.Add($"FAQ: {enrichment.FAQUrl}");

        if (!string.IsNullOrEmpty(enrichment.CVRFId))
            parts.Add($"CVRF Document: {enrichment.CVRFId}");

        if (!string.IsNullOrEmpty(enrichment.CSAFId))
            parts.Add($"CSAF Document: {enrichment.CSAFId}");

        return string.Join("\n", parts);
    }

    /// <summary>True when any of the caller's roles may act on the current approval step.</summary>
    public bool CanActOnCurrent(ExceptionRecord ex, ICollection<string>? roles)
    {
        if (ex.CurrentApprovalStage == null) return false;
        if (roles != null && roles.Contains(AppRoles.Admin)) return true;

        var step = CurrentStep(ex);
        return step != null && roles != null && roles.Contains(step.RequiredRole);
    }

    /// <summary>Apply an approver decision to the current step and advance / reject / bounce the chain.</summary>
    public WorkflowResult RecordDecision(ExceptionRecord ex, ApprovalDecision decision, int userId, string comment)
    {
        var step = CurrentStep(ex);
        if (step == null) return WorkflowResult.NoOp;

        step.DecisionByUserId = userId;
        step.DecisionAt = DateTime.UtcNow;
        step.Comment = comment;
        step.Decision = decision;

        switch (decision)
        {
            case ApprovalDecision.Rejected:
                ex.Status = ExceptionStatus.Rejected;
                ex.RejectionReason = comment;
                ex.CurrentApprovalStage = null;
                if (ex.VulnerabilityInstance != null) ex.VulnerabilityInstance.Status = "Open";
                return WorkflowResult.Rejected;

            case ApprovalDecision.NeedMoreInfo:
                ex.Status = ExceptionStatus.NeedMoreInfo;
                return WorkflowResult.NeedMoreInfo;

            case ApprovalDecision.Approved:
                return Advance(ex);

            default:
                return WorkflowResult.NoOp;
        }
    }

    /// <summary>
    /// Re-enter the chain after an approver requested more information: returns the
    /// exception to the pending state for the current stage (and re-opens that step)
    /// so the approver can act on it again. Caller persists changes.
    /// </summary>
    public void Resubmit(ExceptionRecord ex)
    {
        if (ex.CurrentApprovalStage == null) return;

        var step = ex.ApprovalSteps?
            .Where(s => s.Stage == ex.CurrentApprovalStage && s.Decision == ApprovalDecision.NeedMoreInfo)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();
        if (step != null)
        {
            step.Decision = ApprovalDecision.Pending;
            step.DecisionAt = null;
        }

        ex.Status = ex.CurrentApprovalStage switch
        {
            ApprovalStage.Technical => ExceptionStatus.PendingTechnicalApproval,
            ApprovalStage.Manager => ExceptionStatus.PendingManagerApproval,
            ApprovalStage.Security => ExceptionStatus.PendingSecurityApproval,
            _ => ex.Status
        };
    }

    private ExceptionApprovalStep? CurrentStep(ExceptionRecord ex) =>
        ex.ApprovalSteps?
            .Where(s => s.Stage == ex.CurrentApprovalStage && s.Decision == ApprovalDecision.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

    private WorkflowResult Advance(ExceptionRecord ex)
    {
        switch (ex.CurrentApprovalStage)
        {
            case ApprovalStage.Technical:
                ex.CurrentApprovalStage = ApprovalStage.Manager;
                ex.Status = ExceptionStatus.PendingManagerApproval;
                return WorkflowResult.Advanced;

            case ApprovalStage.Manager:
                ex.CurrentApprovalStage = ApprovalStage.Security;
                ex.Status = ExceptionStatus.PendingSecurityApproval;
                return WorkflowResult.Advanced;

            case ApprovalStage.Security:
                ex.CurrentApprovalStage = null;
                ex.Status = ExceptionStatus.ActiveException;
                ex.ApprovedAt = DateTime.UtcNow;
                if (!ex.StartDate.HasValue) ex.StartDate = DateTime.UtcNow;
                if (!ex.ExpiryDate.HasValue && ex.ReviewFrequencyDays.HasValue)
                    ex.ExpiryDate = (ex.StartDate ?? DateTime.UtcNow).AddDays(ex.ReviewFrequencyDays.Value);
                if (!ex.NextReviewDate.HasValue)
                    ex.NextReviewDate = ex.ExpiryDate
                        ?? (ex.StartDate ?? DateTime.UtcNow).AddDays(ex.ReviewFrequencyDays ?? 90);
                if (ex.VulnerabilityInstance != null) ex.VulnerabilityInstance.Status = "Exception";
                return WorkflowResult.Approved;

            default:
                return WorkflowResult.NoOp;
        }
    }
}
