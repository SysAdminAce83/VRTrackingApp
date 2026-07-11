namespace VRTrackingApp.Domain.Enums
{
    public enum VulnerabilityStatus
    {
        Active,
        Fixed,
        FalsePositive,
        RiskAccepted,
        Remediated
    }

    public enum ScanStatus
    {
        Processing,
        Completed,
        Failed
    }

    public enum SeverityLevel
    {
        Critical,
        High,
        Medium,
        Low,
        Info
    }
}