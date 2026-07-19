using System.Globalization;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services;

public class ScanParseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int HostsImported { get; set; }
    public int FindingsImported { get; set; }
    public int InstancesImported { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ScanPreviewRow
{
    public string Host { get; set; } = "";
    public int PluginId { get; set; }
    public string Name { get; set; } = "";
    public string Severity { get; set; } = "";
    public string? Cve { get; set; }
}

public class ScanPreview
{
    public bool Valid { get; set; }
    public string Message { get; set; } = "";
    public List<string> ValidationChecks { get; set; } = new();
    public int Hosts { get; set; }
    public int Findings { get; set; }
    public int Instances { get; set; }
    public List<ScanPreviewRow> Sample { get; set; } = new();
}

/// <summary>
/// Parses Nessus CSV exports into the normalized data model and validates file safety.
/// PDF is accepted and stored for manual review (deep PDF parsing is a follow-up).
/// </summary>
public class ScanImportService
{
    private readonly VRTrackingAppContext _db;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public ScanImportService(VRTrackingAppContext db)
    {
        _db = db;
    }

    public bool IsAllowedFile(string fileName, long size, string contentType)
    {
        if (size <= 0 || size > MaxFileSize) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not (".csv" or ".pdf" or ".txt" or ".nessus" or ".xml")) return false;
        // Do not trust client-supplied type blindly; we re-check by extension + magic bytes later.
        return true;
    }

    public async Task<ScanParseResult> ImportCsvAsync(Stream stream, ScanUpload scan, string originalFileName)
    {
        var result = new ScanParseResult();
        try
        {
            string content;
            using (var sr = new StreamReader(stream, leaveOpen: true))
                content = await sr.ReadToEndAsync();
            var lines = content.Split('\n');
            var location = DetectLocation(content);

            var headerLine = lines.Length > 0 ? lines[0] : null;
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                result.Message = "The CSV file is empty.";
                return result;
            }

            var headers = ParseCsvLine(headerLine);
            var col = MapColumns(headers);

            if (col.Host < 0 || col.PluginId < 0)
            {
                result.Message = "CSV is missing required columns 'Host' and/or 'Plugin ID'. " +
                                 "A valid Nessus export is required.";
                return result;
            }

            var seenFindings = new Dictionary<int, VulnerabilityFinding>();
            var seenHosts = new Dictionary<string, AssetHost>(StringComparer.OrdinalIgnoreCase);
            var seenAssets = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
            var lineNo = 0;

            for (var idx = 1; idx < lines.Length; idx++)
            {
                var line = lines[idx];
                lineNo++;
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line);
                if (fields.Count <= col.PluginId) continue;

                try
                {
                    var hostKey = fields[col.Host];
                    if (string.IsNullOrWhiteSpace(hostKey)) continue;

                    if (!seenHosts.TryGetValue(hostKey, out var host))
                    {
                        host = new AssetHost
                        {
                            ScanUploadId = scan.Id,
                            HostName = hostKey,
                            IpAddress = hostKey,
                            OperatingSystem = col.OS >= 0 ? fields[col.OS] : null,
                            CreatedAt = DateTime.UtcNow
                        };
                        host.Asset = await GetOrCreateAssetAsync(seenAssets, hostKey, hostKey,
                            host.OperatingSystem, location);
                        _db.AssetHosts.Add(host);
                        seenHosts[hostKey] = host;
                    }

                    if (!int.TryParse(Safe(fields, col.PluginId), out var pluginId))
                        pluginId = Math.Abs(fields[col.PluginId].GetHashCode());

                    if (!seenFindings.TryGetValue(pluginId, out var finding))
                    {
                        finding = new VulnerabilityFinding
                        {
                            PluginId = pluginId,
                            PluginName = Safe(fields, col.Name),
                            Cve = col.Cve >= 0 ? CleanCve(Safe(fields, col.Cve)) : null,
                            Severity = NormalizeSeverity(Safe(fields, col.Risk)),
                            Synopsis = col.Synopsis >= 0 ? Safe(fields, col.Synopsis) : null,
                            Description = col.Description >= 0 ? Safe(fields, col.Description) : null,
                            Solution = col.Solution >= 0 ? Safe(fields, col.Solution) : null,
                            RiskFactor = col.RiskFactor >= 0 ? Safe(fields, col.RiskFactor) : null,
                            CvssV3BaseScore = ToDouble(Safe(fields, col.CvssV3Base)),
                            CvssV3TemporalScore = ToDouble(Safe(fields, col.CvssV3Temp)),
                            CvssV2BaseScore = ToDouble(Safe(fields, col.CvssV2Base)),
                            CvssV2TemporalScore = ToDouble(Safe(fields, col.CvssV2Temp)),
                            VprScore = ToDouble(Safe(fields, col.Vpr)),
                            EpssScore = ToDouble(Safe(fields, col.Epss)),
                            StigSeverity = col.Stig >= 0 ? Safe(fields, col.Stig) : null,
                            References = col.SeeAlso >= 0 ? Safe(fields, col.SeeAlso) : null,
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.VulnerabilityFindings.Add(finding);
                        seenFindings[pluginId] = finding;
                        result.FindingsImported++;
                    }

                    var instance = new VulnerabilityInstance
                    {
                        AssetHost = host,
                        VulnerabilityFinding = finding,
                        Port = ToInt(Safe(fields, col.Port)),
                        Protocol = col.Protocol >= 0 ? Safe(fields, col.Protocol) : null,
                        ServiceName = col.Name >= 0 ? Safe(fields, col.Name) : null,
                        PluginOutput = col.PluginOutput >= 0 ? Safe(fields, col.PluginOutput) : null,
                        Status = "Open",
                        FirstFound = scan.ScanDate ?? DateTime.UtcNow,
                        LastFound = scan.ScanDate ?? DateTime.UtcNow
                    };
                    _db.VulnerabilityInstances.Add(instance);
                    result.InstancesImported++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Line {lineNo}: {ex.Message}");
                }
            }

            result.HostsImported = seenHosts.Count;
            result.Success = true;
            result.Message = $"Imported {result.HostsImported} host(s), " +
                             $"{result.FindingsImported} finding(s), {result.InstancesImported} instance(s).";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Parsing failed: {ex.Message}";
        }

        return result;
    }

    /// <summary>Parses a CSV into a preview WITHOUT persisting anything (validation-before-save).</summary>
    public async Task<ScanPreview> PreviewCsvAsync(Stream stream)
    {
        var preview = new ScanPreview();
        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                preview.Message = "The CSV file is empty.";
                return preview;
            }

            var headers = ParseCsvLine(headerLine);
            var col = MapColumns(headers);
            preview.ValidationChecks.Add("File readable as UTF-8 text ✓");
            preview.ValidationChecks.Add($"Header columns detected: {headers.Count}");

            if (col.Host < 0 || col.PluginId < 0)
            {
                preview.Message = "CSV is missing required columns 'Host' and/or 'Plugin ID'.";
                preview.ValidationChecks.Add("Required columns (Host, Plugin ID) ✗");
                return preview;
            }
            preview.ValidationChecks.Add("Required columns (Host, Plugin ID) ✓");

            var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var plugins = new HashSet<int>();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line);
                if (fields.Count <= col.PluginId) continue;
                var hostKey = Safe(fields, col.Host);
                if (string.IsNullOrWhiteSpace(hostKey)) continue;

                hosts.Add(hostKey);
                int.TryParse(Safe(fields, col.PluginId), out var pid);
                plugins.Add(pid);
                preview.Instances++;

                if (preview.Sample.Count < 15)
                {
                    preview.Sample.Add(new ScanPreviewRow
                    {
                        Host = hostKey,
                        PluginId = pid,
                        Name = Safe(fields, col.Name),
                        Severity = NormalizeSeverity(Safe(fields, col.Risk)),
                        Cve = col.Cve >= 0 ? CleanCve(Safe(fields, col.Cve)) : null
                    });
                }
            }

            preview.Hosts = hosts.Count;
            preview.Findings = plugins.Count;
            preview.Valid = preview.Instances > 0;
            preview.ValidationChecks.Add($"Parsed {preview.Instances} finding row(s) ✓");
            preview.Message = preview.Valid
                ? $"Ready to import: {preview.Hosts} host(s), {preview.Findings} finding(s), {preview.Instances} instance(s)."
                : "No valid finding rows were detected.";
        }
        catch (Exception ex)
        {
            preview.Message = $"Validation failed: {ex.Message}";
        }
        return preview;
    }

    /// <summary>
    /// Extracts structured findings from a Nessus PDF export. PDF text layout varies by
    /// Nessus version, so this is a best-effort heuristic: it reads the "Vulnerabilities By Host"
    /// table for (host, plugin, severity) rows and the "Vulnerability Details" section to enrich
    /// each finding with CVE/description/severity. Returns null data if nothing could be read.
    /// </summary>
    public async Task<PdfParseData?> ParsePdfAsync(Stream stream)
    {
        byte[] bytes;
        if (stream is MemoryStream ms && ms.TryGetBuffer(out var buf))
        {
            bytes = new byte[buf.Count];
            Buffer.BlockCopy(buf.Array!, buf.Offset, bytes, 0, buf.Count);
        }
        else
        {
            var copy = new MemoryStream();
            await stream.CopyToAsync(copy);
            bytes = copy.ToArray();
        }

        var data = new PdfParseData();
        try
        {
            using var pdf = new PdfDocument(new PdfReader(new MemoryStream(bytes)));
            var sb = new System.Text.StringBuilder();
            for (var i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                sb.AppendLine(PdfTextExtractor.GetTextFromPage(page, new LocationTextExtractionStrategy()));
            }

            var text = sb.ToString();
            data.RawText = text;
            ParseNessusPdf(text.Split('\n'), data);
        }
        catch (Exception ex)
        {
            data.Errors.Add($"PDF read failed: {ex.Message}");
        }
        return data;
    }

    public async Task<ScanPreview> PreviewPdfAsync(Stream stream)
    {
        var preview = new ScanPreview();
        var data = await ParsePdfAsync(stream);
        if (data == null || data.Rows.Count == 0)
        {
            preview.Message = data?.Errors.Count > 0 ? string.Join("; ", data.Errors) : "No scannable findings found in the PDF.";
            preview.ValidationChecks.Add("Structured findings detected ✗");
            return preview;
        }
        preview.Hosts = data.Rows.Select(r => r.Host).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        preview.Findings = data.Rows.Select(r => r.PluginId).Distinct().Count();
        preview.Instances = data.Rows.Count;
        preview.Valid = preview.Instances > 0;
        preview.ValidationChecks.Add("File type PDF ✓");
        preview.ValidationChecks.Add($"Parsed {preview.Instances} finding row(s) from PDF ✓");
        preview.Message = preview.Valid
            ? $"Ready to import: {preview.Hosts} host(s), {preview.Findings} finding(s), {preview.Instances} instance(s)."
            : "No valid finding rows were detected.";
        return preview;
    }



    /// <summary>
    /// Normalizes a CSV report into the common ParsedScan model used by the
    /// ingestion/deduplication engine. Keeps CSV/PDF/.nessus on one comparable shape (Scenario 8).
    /// Does not persist anything.
    /// </summary>
    public async Task<ParsedScan> ParseCsvToModelAsync(Stream stream)
    {
        var scan = new ParsedScan();
        try
        {
            string text;
            using (var sr = new StreamReader(stream, leaveOpen: true))
                text = await sr.ReadToEndAsync();
            var lines = text.Split('\n');
            if (lines.Length == 0) { scan.Message = "Empty file."; return scan; }

            var col = MapColumns(ParseCsvLine(lines[0]));
            if (col.Host < 0 || col.PluginId < 0)
            {
                scan.Message = "CSV missing required columns 'Host' and/or 'Plugin ID'.";
                return scan;
            }
            var location = DetectLocation(text);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                var f = ParseCsvLine(lines[i]);
                if (f.Count <= col.PluginId) continue;
                var hostKey = Safe(f, col.Host);
                if (string.IsNullOrWhiteSpace(hostKey)) continue;
                int.TryParse(Safe(f, col.PluginId), out var pid);
                scan.Rows.Add(new ParsedHostFinding
                {
                    HostKey = hostKey,
                    Ip = hostKey,
                    Os = col.OS >= 0 ? Safe(f, col.OS) : null,
                    PluginId = pid,
                    Name = Safe(f, col.Name),
                    Severity = NormalizeSeverity(Safe(f, col.Risk)),
                    Cve = col.Cve >= 0 ? CleanCve(Safe(f, col.Cve)) : null,
                    Synopsis = col.Synopsis >= 0 ? Safe(f, col.Synopsis) : null,
                    Description = col.Description >= 0 ? Safe(f, col.Description) : null,
                    Solution = col.Solution >= 0 ? Safe(f, col.Solution) : null,
                    RiskFactor = col.RiskFactor >= 0 ? Safe(f, col.RiskFactor) : null,
                    Stig = col.Stig >= 0 ? Safe(f, col.Stig) : null,
                    Port = ToInt(Safe(f, col.Port)),
                    Protocol = col.Protocol >= 0 ? Safe(f, col.Protocol) : null,
                    Service = col.Name >= 0 ? Safe(f, col.Name) : null,
                    PluginOutput = col.PluginOutput >= 0 ? Safe(f, col.PluginOutput) : null,
                    CvssV3Base = ToDouble(Safe(f, col.CvssV3Base)),
                    CvssV3Temp = ToDouble(Safe(f, col.CvssV3Temp)),
                    CvssV2Base = ToDouble(Safe(f, col.CvssV2Base)),
                    Vpr = ToDouble(Safe(f, col.Vpr)),
                    Epss = ToDouble(Safe(f, col.Epss)),
                    References = col.SeeAlso >= 0 ? Safe(f, col.SeeAlso) : null,
                });
            }
            scan.Metadata = new ScanMetadata { ScanTarget = location };
            scan.Valid = scan.Rows.Count > 0;
            scan.Message = scan.Valid
                ? $"Ready to ingest: {scan.Hosts} host(s), {scan.Findings} finding(s), {scan.Instances} instance(s)."
                : "No valid finding rows detected.";
        }
        catch (Exception ex) { scan.Message = $"CSV parse failed: {ex.Message}"; }
        return scan;
    }

    /// <summary>Normalizes a parsed PDF report into the common ParsedScan model.</summary>
    public async Task<ParsedScan> ParsePdfToModelAsync(Stream stream)
    {
        var scan = new ParsedScan();
        var data = await ParsePdfAsync(stream);
        if (data == null || data.Rows.Count == 0)
        {
            scan.Message = data?.Errors.Count > 0 ? string.Join("; ", data.Errors) : "No scannable findings found in the PDF.";
            return scan;
        }
        foreach (var r in data.Rows)
        {
            data.Meta.TryGetValue(r.PluginId, out var pm);
            scan.Rows.Add(new ParsedHostFinding
            {
                HostKey = r.Host,
                Ip = r.Ip,
                Os = r.Os,
                PluginId = r.PluginId,
                Name = r.Name,
                Severity = NormalizeSeverity(pm?.Severity ?? r.Severity),
                Cve = pm?.Cve,
                Synopsis = pm?.Synopsis,
                Description = pm?.Description,
                Solution = pm?.Solution,
                RiskFactor = pm?.RiskFactor,
                Stig = pm?.Stig,
                Port = ParsePort(pm?.Port),
                Protocol = ParseProtocol(pm?.Port),
                Service = r.Name,
            });
        }
        scan.Metadata = new ScanMetadata { ScanTarget = DetectLocation(data.RawText ?? "") };
        scan.Valid = scan.Rows.Count > 0;
        scan.Message = scan.Valid
            ? $"Ready to ingest: {scan.Hosts} host(s), {scan.Findings} finding(s), {scan.Instances} instance(s) from PDF."
            : "No valid finding rows detected.";
        return scan;
    }

    /// <summary>Text reports carry no structured findings; returns an empty (invalid) ParsedScan.</summary>
    public Task<ParsedScan> ParseTxtToModelAsync(Stream stream)
    {
        var scan = new ParsedScan { Message = "Text reports contain host inventory only and no structured findings." };
        return Task.FromResult(scan);
    }

    public async Task<ScanParseResult> ImportPdfAsync(Stream stream, ScanUpload scan)
    {
        var result = new ScanParseResult();
        var data = await ParsePdfAsync(stream);
        if (data == null || data.Rows.Count == 0)
        {
            result.Success = false;
            result.Message = data?.Errors.Count > 0 ? string.Join("; ", data.Errors) : "No scannable findings found in the PDF.";
            return result;
        }

        var seenFindings = new Dictionary<int, VulnerabilityFinding>();
        var seenHosts = new Dictionary<string, AssetHost>(StringComparer.OrdinalIgnoreCase);
        var seenAssets = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        var location = DetectLocation(data.RawText ?? "");

        foreach (var row in data.Rows)
        {
            try
            {
                if (!seenHosts.TryGetValue(row.Host, out var host))
                {
                    host = new AssetHost
                    {
                        ScanUploadId = scan.Id,
                        HostName = row.Host,
                        IpAddress = !string.IsNullOrWhiteSpace(row.Ip) ? row.Ip! : row.Host,
                        CreatedAt = DateTime.UtcNow
                    };
                    host.Asset = await GetOrCreateAssetAsync(seenAssets, row.Host,
                        !string.IsNullOrWhiteSpace(row.Ip) ? row.Ip! : row.Host, row.Os, location);
                    _db.AssetHosts.Add(host);
                    seenHosts[row.Host] = host;
                }

                if (!seenFindings.TryGetValue(row.PluginId, out var finding))
                {
                    var meta = data.Meta.TryGetValue(row.PluginId, out var m) ? m : null;
                    var severity = meta?.Severity ?? row.Severity;
                    finding = new VulnerabilityFinding
                    {
                        PluginId = row.PluginId,
                        PluginName = row.Name,
                        Cve = meta?.Cve,
                        Severity = NormalizeSeverity(severity),
                        Synopsis = meta?.Synopsis,
                        Description = meta?.Description,
                        Solution = meta?.Solution,
                        RiskFactor = meta?.RiskFactor,
                        StigSeverity = meta?.Stig,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.VulnerabilityFindings.Add(finding);
                    seenFindings[row.PluginId] = finding;
                    result.FindingsImported++;
                }

                data.Meta.TryGetValue(row.PluginId, out var pm);
                _db.VulnerabilityInstances.Add(new VulnerabilityInstance
                {
                    AssetHost = host,
                    VulnerabilityFinding = finding,
                    Port = ParsePort(pm?.Port),
                    Protocol = ParseProtocol(pm?.Port),
                    ServiceName = row.Name,
                    Status = "Open",
                    FirstFound = scan.ScanDate ?? DateTime.UtcNow,
                    LastFound = scan.ScanDate ?? DateTime.UtcNow
                });
                result.InstancesImported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row ({row.Host}, {row.PluginId}): {ex.Message}");
            }
        }

        result.HostsImported = seenHosts.Count;
        result.Success = true;
        result.Message = $"Imported {result.HostsImported} host(s), " +
                         $"{result.FindingsImported} finding(s), {result.InstancesImported} instance(s) from PDF.";
        return result;
    }

    /// <summary>
    /// Plain-text reports have no structured findings, but often list hosts. This extracts
    /// host/IP/OS/location lines so the asset inventory is populated. Findings are not imported.
    /// </summary>
    public async Task<ScanPreview> PreviewTxtAsync(Stream stream)
    {
        var (hosts, location) = await ExtractTxtHostsAsync(stream);
        var preview = new ScanPreview
        {
            Valid = hosts.Count > 0,
            Message = hosts.Count > 0
                ? $"Text report: {hosts.Count} host(s) detected for the asset inventory." +
                  (location != null ? $" Location: {location}." : "")
                : "No host information could be detected in the text file."
        };
        preview.ValidationChecks.Add("File type TXT ✓");
        preview.ValidationChecks.Add(hosts.Count > 0 ? $"Detected {hosts.Count} host(s) ✓" : "No host lines detected ✗");
        return preview;
    }

    public async Task<ScanParseResult> ImportTxtAsync(Stream stream, ScanUpload scan)
    {
        var result = new ScanParseResult();
        var (hosts, location) = await ExtractTxtHostsAsync(stream);
        if (hosts.Count == 0)
        {
            result.Success = false;
            result.Message = "No host information could be detected in the text file.";
            return result;
        }

        var seenHosts = new Dictionary<string, AssetHost>(StringComparer.OrdinalIgnoreCase);
        var seenAssets = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in hosts)
        {
            try
            {
                if (!seenHosts.TryGetValue(h.Name, out var host))
                {
                    host = new AssetHost
                    {
                        ScanUploadId = scan.Id,
                        HostName = h.Name,
                        IpAddress = !string.IsNullOrWhiteSpace(h.Ip) ? h.Ip! : h.Name,
                        OperatingSystem = h.Os,
                        CreatedAt = DateTime.UtcNow
                    };
                    host.Asset = await GetOrCreateAssetAsync(seenAssets, h.Name,
                        !string.IsNullOrWhiteSpace(h.Ip) ? h.Ip! : h.Name, h.Os, location);
                    _db.AssetHosts.Add(host);
                    seenHosts[h.Name] = host;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Host {h.Name}: {ex.Message}");
            }
        }

        result.HostsImported = seenHosts.Count;
        result.Success = true;
        result.Message = $"Added {result.HostsImported} host(s) to the asset inventory from text report.";
        return result;
    }

    private async Task<(List<(string Name, string? Ip, string? Os)> Hosts, string? Location)> ExtractTxtHostsAsync(Stream stream)
    {
        string content;
        using (var sr = new StreamReader(stream, leaveOpen: true))
            content = await sr.ReadToEndAsync();

        var hostRe = new Regex(@"^(?:Host|Hostname|DNS Name|Computer Name|Machine|Server)[:\s]+(.+)$", RegexOptions.IgnoreCase);
        var ipRe = new Regex(@"^(?:IP|IP Address|IPv4 Address)[:\s]+([\d.]{7,})$", RegexOptions.IgnoreCase);
        var osRe = new Regex(@"^(?:OS|Operating System)[:\s]+(.+)$", RegexOptions.IgnoreCase);

        var hosts = new List<(string, string?, string?)>();
        string? singleIp = null, singleOs = null;
        foreach (var raw in content.Split('\n'))
        {
            var line = raw.Trim();
            var hm = hostRe.Match(line);
            if (hm.Success) { hosts.Add((hm.Groups[1].Value.Trim(), null, null)); continue; }
            var im = ipRe.Match(line);
            if (im.Success) { if (singleIp == null) singleIp = im.Groups[1].Value.Trim(); continue; }
            var om = osRe.Match(line);
            if (om.Success) { if (singleOs == null) singleOs = om.Groups[1].Value.Trim(); }
        }

        foreach (var h in hosts)
            hosts[hosts.IndexOf(h)] = (h.Item1, singleIp, singleOs);

        return (hosts, DetectLocation(content));
    }

    private static readonly Regex CveRe = new(@"CVE-\d{4}-\d+", RegexOptions.Compiled);
    private static readonly HashSet<string> Headings = new(StringComparer.OrdinalIgnoreCase)
    {
        "Synopsis", "Description", "Solution", "Risk Factor", "See Also", "References",
        "Plugin Output", "Plugin Information", "STIG Severity", "Scan Information",
        "Host Information", "Vulnerabilities", "TABLE OF CONTENTS", "CVE",
        "VPR Score", "EPSS Score", "Port / Service", "Published"
    };

    private enum CaptureMode { None, Risk, Stig, Solution, Description, Synopsis, Plugin }

    private class ParsedHost
    {
        public string Name { get; set; } = "";
        public string? Ip { get; set; }
        public string? Os { get; set; }
    }

    /// <summary>
    /// Unified Nessus-PDF parser. Handles the "Vulnerabilities by Host" detail layout
    /// (plugin lines like "320863 - Name", host info via DNS Name/IP, and per-finding
    /// detail blocks for Risk Factor / CVE / Plugin Output / Solution / Description), as
    /// well as older tabular ("pid name severity") and "Plugin ID:" formats.
    /// </summary>
    private static void ParseNessusPdf(string[] lines, PdfParseData data)
    {
        ParsedHost? curHost = null;
        PdfRow? current = null;
        var mode = CaptureMode.None;
        var block = new System.Text.StringBuilder();

        string HostName() => curHost?.Name ?? "Unknown";

        void FinalizeBlock()
        {
            if (block.Length > 0 && current != null)
            {
                var txt = block.ToString().Trim();
                var meta = EnsureMeta(current.PluginId);
                if (mode == CaptureMode.Solution) meta.Solution = txt;
                else if (mode == CaptureMode.Description) meta.Description = txt;
                else if (mode == CaptureMode.Synopsis) meta.Synopsis = txt;
            }
            block.Clear();
            mode = CaptureMode.None;
        }

        void StartHeading(string h)
        {
            var s = h.Trim();
            if (s.Equals("Risk Factor", StringComparison.OrdinalIgnoreCase)) mode = CaptureMode.Risk;
            else if (s.Equals("STIG Severity", StringComparison.OrdinalIgnoreCase)) mode = CaptureMode.Stig;
            else if (s.Equals("Plugin Output", StringComparison.OrdinalIgnoreCase)) mode = CaptureMode.Plugin;
            else if (s.Equals("Solution", StringComparison.OrdinalIgnoreCase)) { mode = CaptureMode.Solution; block.Clear(); }
            else if (s.Equals("Description", StringComparison.OrdinalIgnoreCase)) { mode = CaptureMode.Description; block.Clear(); }
            else if (s.Equals("Synopsis", StringComparison.OrdinalIgnoreCase)) { mode = CaptureMode.Synopsis; block.Clear(); }
        }

        void ApplySingle(string val)
        {
            if (current == null) return;
            var meta = EnsureMeta(current.PluginId);
            if (mode == CaptureMode.Risk) { current.Severity = val; meta.Severity = val; }
            else if (mode == CaptureMode.Stig)
            {
                if (Regex.IsMatch(val, @"(CAT [IV]+)|(I{1,3})", RegexOptions.IgnoreCase))
                    meta.Stig = val.ToUpperInvariant();
            }
            else if (mode == CaptureMode.Plugin)
            {
                var pm = Regex.Match(val, @"^(\w+)/(\d+)(?:/(\w+))?$", RegexOptions.IgnoreCase);
                if (pm.Success) meta.Port = $"{pm.Groups[2].Value} / {pm.Groups[1].Value}";
            }
        }

        PdfFindingMeta EnsureMeta(int pid)
            => data.Meta.TryGetValue(pid, out var m) ? m : data.Meta[pid] = new PdfFindingMeta();

        foreach (var raw in lines)
        {
            var line = raw.Trim();

            if (mode != CaptureMode.None && (string.IsNullOrWhiteSpace(line) || IsHeading(line) || IsFooter(line)))
            {
                FinalizeBlock();
                if (string.IsNullOrWhiteSpace(line) || IsFooter(line)) continue;
                // fall through to process the heading
            }
            else if (mode != CaptureMode.None)
            {
                if (mode is CaptureMode.Solution or CaptureMode.Description or CaptureMode.Synopsis)
                    block.AppendLine(line);
                else { ApplySingle(line); mode = CaptureMode.None; }
                continue;
            }

            if (IsFooter(line)) continue;

            if (line.StartsWith("DNS Name:", StringComparison.OrdinalIgnoreCase))
            {
                curHost = new ParsedHost { Name = line["DNS Name:".Length..].Trim() };
                continue;
            }
            if (line.StartsWith("IP:", StringComparison.OrdinalIgnoreCase))
            {
                if (curHost != null) curHost.Ip = line[3..].Trim();
                continue;
            }
            if (line.StartsWith("OS:", StringComparison.OrdinalIgnoreCase))
            {
                if (curHost != null) curHost.Os = line[3..].Trim();
                continue;
            }
            if (IsHeading(line)) { StartHeading(line); continue; }

            var plug = PluginLine(line);
            if (plug.HasValue)
            {
                current = new PdfRow
                {
                    Host = HostName(),
                    Ip = curHost?.Ip,
                    Os = curHost?.Os,
                    PluginId = plug.Value.Pid,
                    Name = plug.Value.Name
                };
                if (plug.Value.Severity != null)
                {
                    current.Severity = plug.Value.Severity;
                    EnsureMeta(current.PluginId).Severity = plug.Value.Severity;
                }
                data.Rows.Add(current);
                continue;
            }

            if (curHost == null && Regex.IsMatch(line, @"^[\w\.\-]{2,}$"))
                curHost = new ParsedHost { Name = line };

            foreach (Match c in CveRe.Matches(line))
                if (current != null)
                {
                    var meta = EnsureMeta(current.PluginId);
                    meta.Cve = meta.Cve == null ? c.Value : meta.Cve + "," + c.Value;
                }
        }

        FinalizeBlock();
    }

    private static bool IsHeading(string line)
    {
        var s = line.Trim();
        if (s.Length == 0) return false;
        if (Headings.Contains(s)) return true;
        if (s.StartsWith("CVSS", StringComparison.OrdinalIgnoreCase)) return true;
        if (s.StartsWith("Note that Nessus", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static bool IsFooter(string line)
        => Regex.IsMatch(line, @"^[\w\.\-]+\s+\d{1,4}\s*$");

    private static (int Pid, string Name, string? Severity)? PluginLine(string line)
    {
        var m1 = Regex.Match(line, @"^(\d{3,6})\s+-\s+(.+)$");
        if (m1.Success)
            return (int.Parse(m1.Groups[1].Value), m1.Groups[2].Value.Trim(), null);
        var m2 = Regex.Match(line, @"^(\d{2,6})\s+(.+?)\s+(Critical|High|Medium|Low|None|Info)\s*$",
            RegexOptions.IgnoreCase);
        if (m2.Success)
            return (int.Parse(m2.Groups[1].Value), m2.Groups[2].Value.Trim(), m2.Groups[3].Value);
        return null;
    }

    private static int? ParsePort(string? portService)
    {
        if (string.IsNullOrWhiteSpace(portService)) return null;
        var m = Regex.Match(portService, @"^(\d+)");
        return m.Success && int.TryParse(m.Groups[1].Value, out var p) ? p : null;
    }

    private static string? ParseProtocol(string? portService)
    {
        if (string.IsNullOrWhiteSpace(portService)) return null;
        var m = Regex.Match(portService, @"/\s*(\w+)");
        return m.Success ? m.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Finds the canonical asset for a host (matched by hostname, then IP) or creates it,
    /// avoiding duplicates across frequent uploads. A local <paramref name="cache"/> prevents
    /// double-creation within a single import before the context is saved.
    /// </summary>
    private async Task<Asset> GetOrCreateAssetAsync(Dictionary<string, Asset> cache,
        string hostName, string ip, string? os, string? location)
    {
        var hn = (hostName ?? "").Trim();
        var ipN = (ip ?? "").Trim();
        var key = !string.IsNullOrEmpty(hn) ? hn.ToLowerInvariant() : ipN.ToLowerInvariant();
        if (string.IsNullOrEmpty(key)) key = "unknown";

        if (cache.TryGetValue(key, out var cached)) return cached;

        Asset? existing = null;
        if (!string.IsNullOrEmpty(hn))
            existing = await _db.Assets.FirstOrDefaultAsync(a => a.HostName != null && a.HostName.ToLower() == hn.ToLower());
        if (existing == null && !string.IsNullOrEmpty(ipN))
            existing = await _db.Assets.FirstOrDefaultAsync(a => a.IpAddress != null && a.IpAddress.ToLower() == ipN.ToLower());

        if (existing == null)
        {
            existing = new Asset
            {
                HostName = hn,
                IpAddress = ipN,
                OperatingSystem = os,
                Location = location,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Assets.Add(existing);
        }
        else
        {
            existing.LastSeen = DateTime.UtcNow;
            if (string.IsNullOrEmpty(existing.OperatingSystem) && !string.IsNullOrEmpty(os))
                existing.OperatingSystem = os;
            if (string.IsNullOrEmpty(existing.Location) && !string.IsNullOrEmpty(location))
                existing.Location = location;
        }

        cache[key] = existing;
        return existing;
    }

    /// <summary>
    /// Best-effort location extraction from raw report text (e.g. "Location: DC-East").
    /// Returns null when no location hint is present so the user can fill it in manually.
    /// </summary>
    private static string? DetectLocation(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var m = Regex.Match(text,
            @"(?:Location|Site|Datacenter|Data Center|Facility|Region|Building|Office|Branch)[:\s]+(.{2,60})",
            RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        var value = m.Groups[1].Value.Trim().TrimEnd('.').Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value.Length <= 255 ? value : value[..255];
    }

    public class PdfRow
    {
        public string Host { get; set; } = "";
        public string? Ip { get; set; }
        public string? Os { get; set; }
        public int PluginId { get; set; }
        public string Name { get; set; } = "";
        public string Severity { get; set; } = "";
    }

    public class PdfFindingMeta
    {
        public string? Severity { get; set; }
        public string? Cve { get; set; }
        public string? Description { get; set; }
        public string? Solution { get; set; }
        public string? Synopsis { get; set; }
        public string? RiskFactor { get; set; }
        public string? Stig { get; set; }
        public string? Port { get; set; }
    }

    public class PdfParseData
    {
        public List<PdfRow> Rows { get; set; } = new();
        public Dictionary<int, PdfFindingMeta> Meta { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string? RawText { get; set; }
    }
    private static ColumnMap MapColumns(List<string> headers)
    {
        var m = new ColumnMap();
        for (var i = 0; i < headers.Count; i++)
        {
            var h = headers[i].Trim().ToLowerInvariant();
            if (h.Contains("plugin id")) m.PluginId = i;
            else if (h is "cve" or "cves") m.Cve = i;
            else if (h.Contains("cvss v3.0 base")) m.CvssV3Base = i;
            else if (h.Contains("cvss v3.0 temporal")) m.CvssV3Temp = i;
            else if (h.Contains("cvss v2.0 base")) m.CvssV2Base = i;
            else if (h.Contains("cvss v2.0 temporal")) m.CvssV2Temp = i;
            else if (h is "vpr" or "vpr score") m.Vpr = i;
            else if (h is "epss" or "epss score") m.Epss = i;
            else if (h.Contains("risk factor")) m.RiskFactor = i;
            else if (h is "risk") m.Risk = i;
            else if (h == "host") m.Host = i;
            else if (h == "protocol") m.Protocol = i;
            else if (h == "port") m.Port = i;
            else if (h == "name") m.Name = i;
            else if (h == "synopsis") m.Synopsis = i;
            else if (h == "description") m.Description = i;
            else if (h == "solution") m.Solution = i;
            else if (h.Contains("see also")) m.SeeAlso = i;
            else if (h.Contains("plugin output")) m.PluginOutput = i;
            else if (h.Contains("stig")) m.Stig = i;
            else if (h.Contains("operating system") || h == "os") m.OS = i;
        }
        return m;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { result.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result;
    }

    private static string Safe(List<string> fields, int index)
        => index >= 0 && index < fields.Count ? fields[index]?.Trim() ?? string.Empty : string.Empty;

    private static string NormalizeSeverity(string value)
    {
        var v = (value ?? string.Empty).Trim().ToLowerInvariant();
        return v switch
        {
            "critical" => "Critical",
            "high" => "High",
            "medium" => "Medium",
            "low" => "Low",
            "none" or "info" or "informational" => "Info",
            _ => string.IsNullOrEmpty(v) ? "Info" : char.ToUpper(v[0]) + v[1..]
        };
    }

    private static string? CleanCve(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0].Trim() : null;
    }

    private static double? ToDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    private static int? ToInt(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private class ColumnMap
    {
        public int PluginId = -1;
        public int Cve = -1;
        public int CvssV3Base = -1;
        public int CvssV3Temp = -1;
        public int CvssV2Base = -1;
        public int CvssV2Temp = -1;
        public int Vpr = -1;
        public int Epss = -1;
        public int Risk = -1;
        public int RiskFactor = -1;
        public int Host = -1;
        public int Protocol = -1;
        public int Port = -1;
        public int Name = -1;
        public int Synopsis = -1;
        public int Description = -1;
        public int Solution = -1;
        public int SeeAlso = -1;
        public int PluginOutput = -1;
        public int Stig = -1;
        public int OS = -1;
    }
}


