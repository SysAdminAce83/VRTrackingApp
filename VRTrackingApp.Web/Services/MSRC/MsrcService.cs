using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Web.Services.MSRC.Models;

namespace VRTrackingApp.Web.Services.MSRC;

public class MsrcService : IMsrcService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MsrcService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string BaseUrl = "https://api.msrc.microsoft.com/";

    public MsrcService(HttpClient httpClient, ILogger<MsrcService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<CVRFDocument?> GetCVRFAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching CVRF document: {Id}", id);
            var response = await _httpClient.GetAsync($"cvrf/{id}", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch CVRF {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<CVRFDocument>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching CVRF {Id}", id);
            return null;
        }
    }

    public async Task<CSAFDocument?> GetCSAFAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching CSAF document: {Id}", id);
            var response = await _httpClient.GetAsync($"csaf/{id}", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch CSAF {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<CSAFDocument>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching CSAF {Id}", id);
            return null;
        }
    }

    public async Task<UpdateSummary[]> GetUpdatesAsync(string? odataFilter = null, CancellationToken ct = default)
    {
        try
        {
            var url = "updates";
            if (!string.IsNullOrWhiteSpace(odataFilter))
            {
                url += $"?$filter={Uri.EscapeDataString(odataFilter)}";
            }

            _logger.LogDebug("Fetching updates with filter: {Filter}", odataFilter);
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch updates: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<UpdateResponse>(content, _jsonOptions);
            return result?.Value ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching updates");
            return [];
        }
    }

    public async Task<UpdateSummary[]> GetUpdatesByCVEAsync(string cve, CancellationToken ct = default)
    {
        var filter = $"cves/any(c: c eq '{cve}')";
        return await GetUpdatesAsync(filter, ct);
    }

    public async Task<UpdateSummary[]> GetUpdatesByYearAsync(int year, CancellationToken ct = default)
    {
        var filter = $"year(InitialReleaseDate) eq {year}";
        return await GetUpdatesAsync(filter, ct);
    }
}