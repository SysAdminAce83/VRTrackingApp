using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRTrackingApp.Web.Services.MSRC.Models;

namespace VRTrackingApp.Web.Services.MSRC;

public interface IMsrcService
{
    Task<CVRFDocument?> GetCVRFAsync(string id, CancellationToken ct = default);
    Task<CSAFDocument?> GetCSAFAsync(string id, CancellationToken ct = default);
    Task<UpdateSummary[]> GetUpdatesAsync(string? odataFilter = null, CancellationToken ct = default);
    Task<UpdateSummary[]> GetUpdatesByCVEAsync(string cve, CancellationToken ct = default);
    Task<UpdateSummary[]> GetUpdatesByYearAsync(int year, CancellationToken ct = default);
}