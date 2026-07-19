using System.Globalization;
using System.Xml.Linq;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services;

/// <summary>
/// Normalized, format-agnostic representation of a parsed Nessus report.
/// Produced by every parser (CSV / PDF / .nessus XML) so the ingestion,
/// deduplication and merge engine can operate on one common model
/// (Scenario 8 - format normalization).
/// </summary>
public class ParsedScan
{
    public bool Valid { get; set; }
    public string Message { get; set; } = "";
    public List<string> ValidationChecks { get; set; } = new();

    // Extracted scan metadata (Scenario 3 &amp; 4)
    public ScanMetadata? Metadata { get; set; }

    public List<ParsedHostFinding> Rows { get; set; } = new();

    public int Hosts => Rows.Select(r => r.HostKey).Distinct(StringComparer.OrdinalIgnoreCase).Count();
    public int Findings => Rows.Select(r => r.PluginId).Distinct().Count();
    public int Instances => Rows.Count;
}

public class ParsedHostFinding
{
    public string HostKey { get; set; } = "";   // hostname or IP
    public string? Ip { get; set; }
    public string? Os { get; set; }
    public int PluginId { get; set; }
    public string Name { get; set; } = "";
    public string Severity { get; set; } = "Info";
    public string? Cve { get; set; }
    public string? Synopsis { get; set; }
    public string? Description { get; set; }
    public string? Solution { get; set; }
    public string? RiskFactor { get; set; }
    public string? Stig { get; set; }
    public int? Port { get; set; }
    public string? Protocol { get; set; }
    public string? Service { get; set; }
    public string? PluginOutput { get; set; }
    public double? CvssV3Base { get; set; }
    public double? CvssV3Temp { get; set; }
    public double? CvssV2Base { get; set; }
    public double? Vpr { get; set; }
    public double? Epss { get; set; }
    public string? References { get; set; }
}

/// <summary>
/// Parses native Nessus XML (.nessus) exports. This is the *richest* source because it
/// carries the authoritative scan metadata: uuid, scannerName, policyName, policyID,
/// scan start/end timestamps - exactly what is needed to detect "same scan, different
/// file" (Scenario 4) and to compute a stable ScanGroup key.
/// </summary>
public static class NessusXmlParser
{
    public static async Task<ParsedScan> ParseAsync(Stream stream)
    {
        var scan = new ParsedScan();
        try
        {
            XDocument doc;
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                ms.Position = 0;
                doc = XDocument.Load(ms);
            }

            var root = doc.Root;
            if (root == null || root.Name != "NessusClientData_v2")
            {
                scan.Message = "Not a valid Nessus (.nessus) XML file.";
                return scan;
            }

            var report = root.Element("Report");
            if (report == null)
            {
                scan.Message = "Missing <Report> element.";
                return scan;
            }

            var nameAttr = report.Attribute("name")?.Value;
            scan.ValidationChecks.Add("Nessus report element found ✓");

            // ---- Scan metadata: from Preferences -> ServerPreferences ----
            var prefs = root.Descendants("ServerPreferences").FirstOrDefault();
            var meta = new ScanMetadata { ScanTarget = nameAttr };
            if (prefs != null)
            {
                string? Pref(string key) => prefs.Elements("preference")
                    .FirstOrDefault(p => string.Equals(p.Element("name")?.Value, key, StringComparison.OrdinalIgnoreCase))
                    ?.Element("value")?.Value;

                meta.NessusScanUuid = Pref("report_uuid");
                meta.ScannerName = Pref("scanner_name") ?? Pref("host");
                meta.PolicyName = Pref("policy_name") ?? Pref("policy");
                meta.PolicyId = Pref("policy_id");
                meta.ScanStart = ParseUtc(Pref("scan_start"));
                meta.ScanEnd = ParseUtc(Pref("scan_end"));
                if (string.IsNullOrWhiteSpace(meta.ScanTarget))
                    meta.ScanTarget = Pref("targets");
            }
            scan.Metadata = meta;
            scan.ValidationChecks.Add(
                string.IsNullOrWhiteSpace(meta.NessusScanUuid)
                    ? "No Nessus scan UUID present (will fall back to composite key)"
                    : $"Nessus scan UUID detected: {meta.NessusScanUuid} ✓");

            // ---- Findings: ReportHost -> ReportItem ----
            foreach (var host in report.Elements("ReportHost"))
            {
                var hostProps = host.Element("HostProperties");
                var hostName = host.Attribute("name")?.Value ?? "";
                var ip = hostProps?.Elements("tag")
                    .FirstOrDefault(t => t.Attribute("name")?.Value == "host-ip")?.Value;
                var os = hostProps?.Elements("tag")
                    .FirstOrDefault(t => t.Attribute("name")?.Value == "operating-system")?.Value;
                if (string.IsNullOrWhiteSpace(hostName) && !string.IsNullOrWhiteSpace(ip))
                    hostName = ip!;

                foreach (var item in host.Elements("ReportItem"))
                {
                    if (!int.TryParse(item.Attribute("pluginID")?.Value, out var pid)) continue;
                    var severity = item.Element("risk_factor")?.Value ?? SeverityFromPluginFamily(item);
                    var row = new ParsedHostFinding
                    {
                        HostKey = hostName,
                        Ip = ip,
                        Os = os,
                        PluginId = pid,
                        Name = item.Attribute("pluginName")?.Value ?? "",
                        Severity = NormalizeSeverity(severity),
                        Cve = ExtractCve(item.Element("cve")?.Value),
                        Synopsis = item.Element("synopsis")?.Value,
                        Description = item.Element("description")?.Value,
                        Solution = item.Element("solution")?.Value,
                        RiskFactor = item.Element("risk_factor")?.Value,
                        Stig = item.Element("stig_severity")?.Value,
                        Port = ParsePort(item.Attribute("port")?.Value),
                        Protocol = item.Attribute("protocol")?.Value?.ToLowerInvariant(),
                        Service = item.Attribute("svc_name")?.Value,
                        PluginOutput = item.Element("plugin_output")?.Value,
                        CvssV3Base = ToDouble(item.Element("cvss3_base_score")?.Value),
                        CvssV3Temp = ToDouble(item.Element("cvss3_temporal_score")?.Value),
                        CvssV2Base = ToDouble(item.Element("cvss_base_score")?.Value),
                        Vpr = ToDouble(item.Element("vpr_score")?.Value),
                        Epss = ToDouble(item.Element("epss_score")?.Value),
                    };
                    scan.Rows.Add(row);
                }
            }

            scan.Valid = scan.Rows.Count > 0;
            scan.Message = scan.Valid
                ? $"Ready to ingest: {scan.Hosts} host(s), {scan.Findings} finding(s), {scan.Instances} instance(s)."
                : "No vulnerabilities were found in the report.";
            scan.ValidationChecks.Add($"Parsed {scan.Instances} finding row(s) ✓");
        }
        catch (Exception ex)
        {
            scan.Message = $"Parse failed: {ex.Message}";
        }
        return scan;
    }

    private static DateTime? ParseUtc(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        // Nessus timestamps look like: Mon Jul 14 12:00:00 2025
        if (DateTime.TryParseExact(v, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d))
            return d;
        if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out d))
            return d;
        return null;
    }

    private static int? ParsePort(string? v)
        => int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) && p > 0 ? p : null;

    private static double? ToDouble(string? v)
        => double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static string? ExtractCve(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0].Trim() : null;
    }

    private static string SeverityFromPluginFamily(XElement item)
    {
        // Nessus plugin families without risk_factor are typically info/plugin.
        return "Info";
    }

    private static string NormalizeSeverity(string value)
    {
        var v = (value ?? "").Trim().ToLowerInvariant();
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
}
