using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>
/// Section 5 — computes overall risk from a Likelihood x Impact matrix.
/// The matrix biases toward the higher axis and escalates when both are high.
/// </summary>
public static class RiskMatrixService
{
    // Rows = Likelihood (VeryLow..Critical), Cols = Impact (Low..Critical)
    private static readonly RiskLevel[,] Matrix =
    {
        //            Low               Medium            High              Critical
        /*VeryLow */ { RiskLevel.Low,    RiskLevel.Low,    RiskLevel.Medium, RiskLevel.High },
        /*Low     */ { RiskLevel.Low,    RiskLevel.Medium, RiskLevel.Medium, RiskLevel.High },
        /*Medium  */ { RiskLevel.Low,    RiskLevel.Medium, RiskLevel.High,   RiskLevel.High },
        /*High    */ { RiskLevel.Medium, RiskLevel.High,   RiskLevel.High,   RiskLevel.Critical },
        /*Critical*/ { RiskLevel.High,   RiskLevel.High,   RiskLevel.Critical, RiskLevel.Critical },
    };

    public static RiskLevel Calculate(Likelihood likelihood, ImpactLevel impact)
        => Matrix[(int)likelihood, (int)impact];

    public static RiskLevel? CalculateOrNull(Likelihood? likelihood, ImpactLevel? impact)
        => likelihood.HasValue && impact.HasValue
            ? Calculate(likelihood.Value, impact.Value)
            : null;
}
