using System.Text.Json;
using System.Text.RegularExpressions;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Resolves the registry recipe for a configuration finding, two ways:
///   1) a curated playbook from RemediationPlaybooks.json (matched by plugin id / keyword), and
///   2) a generic parse of the Nessus plugin output (expected vs. detected registry values).
/// This lets config findings like Intel BHI be checked/fixed without hard-coding each one.
/// </summary>
public partial class RegistryPlaybookStore
{
    private readonly List<RegistryPlaybook> _playbooks;
    private readonly ILogger<RegistryPlaybookStore> _log;

    public RegistryPlaybookStore(IWebHostEnvironment env, ILogger<RegistryPlaybookStore> log)
    {
        _log = log;
        var path = Path.Combine(env.ContentRootPath, "RemediationPlaybooks.json");
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var file = JsonSerializer.Deserialize<RegistryPlaybookFile>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _playbooks = file?.RegistryPlaybooks ?? new();
                _log.LogInformation("Loaded {Count} registry playbook(s) from {Path}", _playbooks.Count, path);
            }
            else
            {
                _playbooks = new();
                _log.LogWarning("Registry playbook file not found at {Path}", path);
            }
        }
        catch (Exception ex)
        {
            _playbooks = new();
            _log.LogError(ex, "Failed to load registry playbooks from {Path}", path);
        }
    }

    /// <summary>Find a curated playbook by plugin id first, then by keyword in the finding text.</summary>
    public RegistryPlaybook? ResolveCurated(int pluginId, string haystack)
    {
        var byId = _playbooks.FirstOrDefault(p => p.PluginIds.Contains(pluginId));
        if (byId is not null) return byId;

        return _playbooks.FirstOrDefault(p =>
            p.Keywords.Any(k => haystack.Contains(k, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Best-effort parse of expected registry values from Nessus plugin output.
    /// Handles common shapes like:
    ///   "HKLM\...\FeatureSettingsOverride" ... Expected/Policy/Required value: 8388608
    /// Returns null if nothing confident is found (so we fall back to curated/manual).
    /// NOTE: tune the regexes to your exact CSV output format for best coverage.
    /// </summary>
    public RegistryPlaybook? ParseFromOutput(string? pluginOutput)
    {
        if (string.IsNullOrWhiteSpace(pluginOutput)) return null;

        var settings = new List<RegistrySetting>();
        foreach (Match m in RegistryLineRegex().Matches(pluginOutput))
        {
            var hive = m.Groups["hive"].Value.ToUpperInvariant();
            var sub = m.Groups["path"].Value.Trim().Trim('\\');
            var name = m.Groups["name"].Value.Trim();
            var expected = m.Groups["expected"].Value.Trim();

            var psHive = hive switch
            {
                "HKLM" or "HKEY_LOCAL_MACHINE" => "HKLM:",
                "HKCU" or "HKEY_CURRENT_USER" => "HKCU:",
                _ => null
            };
            if (psHive is null || string.IsNullOrWhiteSpace(name)) continue;

            var setting = new RegistrySetting { Path = $"{psHive}\\{sub}", Name = name };
            if (long.TryParse(expected, out var num)) { setting.Type = "DWord"; setting.Expected = num; }
            else { setting.Type = "String"; setting.ExpectedString = expected; }
            settings.Add(setting);
        }

        if (settings.Count == 0) return null;

        return new RegistryPlaybook
        {
            Name = "Parsed from plugin output",
            RequiresReboot = true,
            Verified = false,
            Settings = settings
        };
    }

    // Matches a registry path + value pair on nearby lines.
    [GeneratedRegex(
        @"(?<hive>HKLM|HKCU|HKEY_LOCAL_MACHINE|HKEY_CURRENT_USER)[\\]{1,2}(?<path>[^\r\n""]+?)[\\]{1,2}(?<name>[A-Za-z0-9_]+)""?[^\r\n]*?(?:expected|policy|required|should be)\s*value\s*[:=]\s*(?<expected>[^\r\n,;]+)",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegistryLineRegex();
}
