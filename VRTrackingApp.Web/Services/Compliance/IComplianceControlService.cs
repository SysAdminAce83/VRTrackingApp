using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Compliance;

public interface IComplianceControlService
{
    Task<IReadOnlyList<ComplianceControl>> GetAllAsync(string? framework = null, string? family = null, string? search = null, CancellationToken ct = default);
    Task<ComplianceControl?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ComplianceControl?> GetByControlIdAsync(string controlId, CancellationToken ct = default);
    Task<ComplianceControl> CreateAsync(ComplianceControl control, CancellationToken ct = default);
    Task UpdateAsync(ComplianceControl control, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Framework>> GetFrameworksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ControlFamily>> GetControlFamiliesAsync(int frameworkId, CancellationToken ct = default);
}