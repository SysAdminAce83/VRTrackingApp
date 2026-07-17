using System;
using System.Linq;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>
/// Determines the stage-1 approver role for an exception:
/// Network Manager for network/firewall/security-appliance findings,
/// otherwise Infrastructure Manager for server/OS/application findings.
/// </summary>
public class ExceptionRoutingService
{
    private static readonly string[] NetworkAssetCategories =
        { "network", "firewall", "router", "switch", "load balancer", "loadbalancer", "appliance" };

    private static readonly string[] NetworkKeywords =
    {
        "firewall", "network", "cipher", "tls", "ssl", "smb", "rdp", "dns", "vpn",
        "port", "protocol", "ids", "ips", "waf", "certificate", "netlogon", "ldap",
        "snmp", "telnet", "ftp", "http", "openssh", "ssh"
    };

    /// <summary>Returns <see cref="AppRoles.NetworkManager"/> or <see cref="AppRoles.InfrastructureManager"/>.</summary>
    public string ResolveStage1Role(VulnerabilityInstance instance)
    {
        var asset = instance.AssetHost?.Asset;
        var category = (asset?.Category ?? "").ToLowerInvariant();
        var subCategory = (asset?.SubCategory ?? "").ToLowerInvariant();
        if (NetworkAssetCategories.Any(c => category.Contains(c) || subCategory.Contains(c)))
            return AppRoles.NetworkManager;

        var finding = instance.VulnerabilityFinding;
        var haystack = string.Join(" ", new[]
        {
            finding?.PluginName, finding?.RiskFactor, finding?.Synopsis,
            instance.ServiceName, instance.Protocol
        }.Where(s => !string.IsNullOrWhiteSpace(s))).ToLowerInvariant();

        if (NetworkKeywords.Any(k => haystack.Contains(k)))
            return AppRoles.NetworkManager;

        return AppRoles.InfrastructureManager;
    }

    /// <summary>Human label for the resolved manager role.</summary>
    public static string RoleLabel(string role) => role switch
    {
        AppRoles.NetworkManager => "Network Manager",
        AppRoles.InfrastructureManager => "Infrastructure Manager",
        AppRoles.RiskCommittee => "Risk Committee",
        AppRoles.Ciso => "CISO",
        _ => role
    };
}
