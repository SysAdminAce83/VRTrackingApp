using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Compliance;

namespace VRTrackingApp.Web.Services.Compliance;

public class ComplianceControlService : IComplianceControlService
{
    private readonly VRTrackingAppContext _db;

    public ComplianceControlService(VRTrackingAppContext db) => _db = db;

    public async Task<IReadOnlyList<ComplianceControl>> GetAllAsync(string? framework = null, string? family = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.ComplianceControls.AsQueryable();

        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(c => c.Framework == framework);
        if (!string.IsNullOrWhiteSpace(family))
            query = query.Where(c => c.ControlFamilyNavigation != null && c.ControlFamilyNavigation.Name == family);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.ControlId.ToLower().Contains(term)
                || c.Name.ToLower().Contains(term)
                || c.Description.ToLower().Contains(term));
        }

        return await query
            .Include(c => c.ControlFamilyNavigation)
            .OrderBy(c => c.Framework)
            .ThenBy(c => c.ControlId)
            .ToListAsync(ct);
    }

    public async Task<ComplianceControl?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.ComplianceControls
            .Include(c => c.ControlFamilyNavigation)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<ComplianceControl?> GetByControlIdAsync(string controlId, CancellationToken ct = default)
    {
        return await _db.ComplianceControls
            .FirstOrDefaultAsync(c => c.ControlId == controlId, ct);
    }

    public async Task<ComplianceControl> CreateAsync(ComplianceControl control, CancellationToken ct = default)
    {
        control.CreatedAt = DateTime.UtcNow;
        _db.ComplianceControls.Add(control);
        await _db.SaveChangesAsync(ct);
        return control;
    }

    public async Task UpdateAsync(ComplianceControl control, CancellationToken ct = default)
    {
        control.UpdatedAt = DateTime.UtcNow;
        _db.ComplianceControls.Update(control);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var control = await _db.ComplianceControls.FindAsync(id);
        if (control != null)
        {
            _db.ComplianceControls.Remove(control);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Framework>> GetFrameworksAsync(CancellationToken ct = default)
    {
        return await _db.Frameworks
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ControlFamily>> GetControlFamiliesAsync(int frameworkId, CancellationToken ct = default)
    {
        return await _db.ControlFamilies
            .Where(f => f.FrameworkId == frameworkId && f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);
    }
}