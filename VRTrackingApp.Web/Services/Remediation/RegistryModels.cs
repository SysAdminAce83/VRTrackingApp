using System.Text.Json;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>One registry value that a configuration finding expects to be set.</summary>
public class RegistrySetting
{
    /// <summary>PowerShell-style hive path, e.g. HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management</summary>
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>DWord | QWord | String</summary>
    public string Type { get; set; } = "DWord";
    /// <summary>Expected numeric value (DWord/QWord).</summary>
    public long Expected { get; set; }
    /// <summary>Expected string value (when Type == String).</summary>
    public string? ExpectedString { get; set; }

    public string ExpectedDisplay => Type == "String" ? (ExpectedString ?? "") : Expected.ToString();
}

/// <summary>
/// A recipe describing how to check/fix a configuration finding via the registry.
/// Loaded from RemediationPlaybooks.json and matched to a finding by plugin id or keyword.
/// </summary>
public class RegistryPlaybook
{
    public int[] PluginIds { get; set; } = Array.Empty<int>();
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string Name { get; set; } = "Registry configuration";
    public bool RequiresReboot { get; set; } = true;
    /// <summary>True when a firmware/microcode/BIOS update is also required (cannot be automated here).</summary>
    public bool RequiresMicrocode { get; set; }
    /// <summary>False until the security team has confirmed the exact values are correct for their environment.</summary>
    public bool Verified { get; set; }
    public List<RegistrySetting> Settings { get; set; } = new();
}

internal class RegistryPlaybookFile
{
    public List<RegistryPlaybook> RegistryPlaybooks { get; set; } = new();
}
