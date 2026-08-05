using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRTrackingApp.Web.Services.NVD.Models;

namespace VRTrackingApp.Web.Services.NVD;

public interface INvdService
{
    Task<NvdCveResponse?> GetCveByKeywordAsync(string keyword, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default);
    Task<NvdCve?> GetCveByIdAsync(string cveId, CancellationToken ct = default);
    Task<NvdCveResponse?> GetCvesByDateRangeAsync(string pubStartDate, string pubEndDate, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default);
    Task<NvdCveResponse?> GetCvesByCpeAsync(string cpeUri, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default);
}