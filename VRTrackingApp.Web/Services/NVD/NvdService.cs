using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Web.Services.NVD.Models;

namespace VRTrackingApp.Web.Services.NVD;

public class NvdService : INvdService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NvdService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string BaseUrl = "https://services.nvd.nist.gov/rest/json/cves/2.0";

    public NvdService(HttpClient httpClient, ILogger<NvdService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<NvdCveResponse?> GetCveByKeywordAsync(string keyword, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching NVD CVEs for keyword: {Keyword}", keyword);
            var url = $"?keywordSearch={Uri.EscapeDataString(keyword)}&startIndex={startIndex}&resultsPerPage={resultsPerPage}";
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch NVD CVEs for keyword {Keyword}: {StatusCode}", keyword, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<NvdCveResponse>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching NVD CVEs for keyword {Keyword}", keyword);
            return null;
        }
    }

    public async Task<NvdCve?> GetCveByIdAsync(string cveId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching NVD CVE: {CveId}", cveId);
            var url = $"?cveId={Uri.EscapeDataString(cveId)}";
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch NVD CVE {CveId}: {StatusCode}", cveId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<NvdCve>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching NVD CVE {CveId}", cveId);
            return null;
        }
    }

    public async Task<NvdCveResponse?> GetCvesByDateRangeAsync(string pubStartDate, string pubEndDate, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching NVD CVEs from {Start} to {End}", pubStartDate, pubEndDate);
            var url = $"?pubStartDate={Uri.EscapeDataString(pubStartDate)}&pubEndDate={Uri.EscapeDataString(pubEndDate)}&startIndex={startIndex}&resultsPerPage={resultsPerPage}";
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch NVD CVEs by date range: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<NvdCveResponse>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching NVD CVEs by date range");
            return null;
        }
    }

    public async Task<NvdCveResponse?> GetCvesByCpeAsync(string cpeUri, int startIndex = 0, int resultsPerPage = 200, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Fetching NVD CVEs for CPE: {Cpe}", cpeUri);
            var url = $"?cpeUri={Uri.EscapeDataString(cpeUri)}&startIndex={startIndex}&resultsPerPage={resultsPerPage}";
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch NVD CVEs for CPE {Cpe}: {StatusCode}", cpeUri, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<NvdCveResponse>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching NVD CVEs for CPE {Cpe}", cpeUri);
            return null;
        }
    }
}