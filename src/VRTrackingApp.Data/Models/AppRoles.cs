namespace VRTrackingApp.Data.Models;

/// <summary>Canonical role names used across authorization, seeding and the approval chain.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string RemediationOwner = "Remediation Owner";
    public const string Auditor = "Auditor";
    public const string SecurityChampion = "SecurityChampion";

    // Exception approval chain (V2)
    public const string InfrastructureManager = "InfrastructureManager";
    public const string NetworkManager = "NetworkManager";
    public const string RiskCommittee = "RiskCommittee";
    public const string Ciso = "CISO";
}
