using System.Text.RegularExpressions;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Inspects a vulnerability finding and works out WHAT the remediation is:
/// a Windows KB to install, a registry configuration change, a Linux package,
/// or something that needs a human. This is the piece that lets the same
/// automation work across different vulnerabilities.
/// </summary>
public partial class PatchIdentifierParser
{
    private readonly RegistryPlaybookStore _registry;

    public PatchIdentifierParser(RegistryPlaybookStore registry) => _registry = registry;

    [GeneratedRegex(@"KB\d{6,7}", RegexOptions.IgnoreCase)]
    private static partial Regex KbRegex();

    public RemediationPlan Resolve(VulnerabilityInstance instance)
    {
        var finding = instance.VulnerabilityFinding;
        var os = instance.AssetHost?.OperatingSystem
                 ?? instance.AssetHost?.Asset?.OperatingSystem
                 ?? "";

        // Use MSRC-enriched KB numbers if available (highest priority)
        var msrcKbs = finding?.KBNumbersArray ?? [];
        var msrcUrls = finding?.PatchDownloadUrlsArray ?? [];

        // All the free text where a KB / registry hint might appear.
        var haystack = string.Join("\n", new[]
        {
            finding?.PluginName,
            finding?.Solution,
            finding?.Description,
            finding?.Synopsis,
            finding?.References,
            instance.PluginOutput
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var isWindows = os.Contains("windows", StringComparison.OrdinalIgnoreCase);
        var isLinux = new[] { "linux", "red hat", "redhat", "rhel", "ubuntu", "centos", "debian", "oracle linux", "suse" }
            .Any(k => os.Contains(k, StringComparison.OrdinalIgnoreCase));

        // 0) MSRC-enriched KB numbers (highest priority)
        if (msrcKbs.Length > 0)
        {
            var primaryKb = msrcKbs[0].ToUpperInvariant();
            var primaryUrl = msrcUrls.Length > 0 ? msrcUrls[0] : null;
            return new RemediationPlan(RemediationKind.WindowsKb, primaryKb,
                $"MSRC-enriched: {string.Join(", ", msrcKbs)}") 
            { 
                PatchDownloadUrl = primaryUrl,
                AllKbNumbers = msrcKbs 
            };
        }

        // 1) Most specific: a curated registry playbook matched by plugin id / keyword.
        var curated = _registry.ResolveCurated(finding?.PluginId ?? 0, haystack);
        if (curated is not null)
            return new RemediationPlan(RemediationKind.RegistryConfig, null,
                $"Registry configuration playbook: {curated.Name}.") { Registry = curated };

        // 2) A concrete KB in the text -> patch install.
        var kb = KbRegex().Match(haystack);
        if (kb.Success)
            return new RemediationPlan(RemediationKind.WindowsKb, kb.Value.ToUpperInvariant(),
                $"Found {kb.Value.ToUpperInvariant()} referenced in the finding text.");

        // 3) Registry expectations parsed generically from the plugin output.
        var parsed = _registry.ParseFromOutput(instance.PluginOutput);
        if (parsed is not null)
            return new RemediationPlan(RemediationKind.RegistryConfig, null,
                $"Parsed {parsed.Settings.Count} expected registry value(s) from plugin output.") { Registry = parsed };

        // 4) Windows patch wording but no KB number.
        if (isWindows && MentionsPatch(haystack))
            return new RemediationPlan(RemediationKind.WindowsKb, null,
                "Windows patch remediation, but no KB number was found in the finding text.");

        // 5) Linux package/erratum.
        if (isLinux)
            return new RemediationPlan(RemediationKind.LinuxPackage, finding?.Cve,
                "Linux host — remediation via package/erratum update.");

        return new RemediationPlan(RemediationKind.Manual, null,
            "Could not classify an automatable fix for this finding — manual review required.");
    }

    private static bool MentionsPatch(string text) =>
        new[] { "patch", "update", "cumulative", "security update", "rollup", "hotfix" }
            .Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
}
