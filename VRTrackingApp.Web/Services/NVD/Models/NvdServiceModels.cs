using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VRTrackingApp.Web.Services.NVD.Models;

public class NvdCveResponse
{
    [JsonPropertyName("resultsPerPage")]
    public int ResultsPerPage { get; set; }

    [JsonPropertyName("startIndex")]
    public int StartIndex { get; set; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public NvdVulnerability[]? Vulnerabilities { get; set; }
}

public class NvdVulnerability
{
    [JsonPropertyName("cve")]
    public NvdCve Cve { get; set; } = default!;
}

public class NvdCve
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("sourceIdentifier")]
    public string? SourceIdentifier { get; set; }

    [JsonPropertyName("published")]
    public string? Published { get; set; }

    [JsonPropertyName("lastModified")]
    public string? LastModified { get; set; }

    [JsonPropertyName("vulnStatus")]
    public string? VulnStatus { get; set; }

    [JsonPropertyName("cisaVulnerabilityName")]
    public string? CisaVulnerabilityName { get; set; }

    [JsonPropertyName("descriptions")]
    public NvdDescription[]? Descriptions { get; set; }

    [JsonPropertyName("metrics")]
    public NvdMetrics? Metrics { get; set; }

    [JsonPropertyName("weaknesses")]
    public NvdWeakness[]? Weaknesses { get; set; }

    [JsonPropertyName("configurations")]
    public NvdConfiguration[]? Configurations { get; set; }

    [JsonPropertyName("references")]
    public NvdReference[]? References { get; set; }
}

public class NvdDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = default!;

    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;
}

public class NvdMetrics
{
    [JsonPropertyName("cvssMetricV31")]
    public NvdCvssMetric[]? CvssMetricV31 { get; set; }

    [JsonPropertyName("cvssMetricV30")]
    public NvdCvssMetric[]? CvssMetricV30 { get; set; }

    [JsonPropertyName("cvssMetricV2")]
    public NvdCvssMetricV2[]? CvssMetricV2 { get; set; }
}

public class NvdCvssMetric
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }

    [JsonPropertyName("accessVector")]
    public string? AccessVector { get; set; }

    [JsonPropertyName("accessComplexity")]
    public string? AccessComplexity { get; set; }

    [JsonPropertyName("authentication")]
    public string? Authentication { get; set; }

    [JsonPropertyName("confidentialityImpact")]
    public string? ConfidentialityImpact { get; set; }

    [JsonPropertyName("integrityImpact")]
    public string? IntegrityImpact { get; set; }

    [JsonPropertyName("availabilityImpact")]
    public string? AvailabilityImpact { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }

    [JsonPropertyName("exploitabilityScore")]
    public double? ExploitabilityScore { get; set; }

    [JsonPropertyName("impactScore")]
    public double? ImpactScore { get; set; }

    [JsonPropertyName("cvssData")]
    public NvdCvssData? CvssData { get; set; }
}

public class NvdCvssData
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }

    [JsonPropertyName("baseScore")]
    public double? BaseScore { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }

    [JsonPropertyName("attackVector")]
    public string? AttackVector { get; set; }

    [JsonPropertyName("attackComplexity")]
    public string? AttackComplexity { get; set; }

    [JsonPropertyName("privilegesRequired")]
    public string? PrivilegesRequired { get; set; }

    [JsonPropertyName("userInteraction")]
    public string? UserInteraction { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("confidentialityImpact")]
    public string? ConfidentialityImpact { get; set; }

    [JsonPropertyName("integrityImpact")]
    public string? IntegrityImpact { get; set; }

    [JsonPropertyName("availabilityImpact")]
    public string? AvailabilityImpact { get; set; }
}

public class NvdCvssMetricV2
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }

    [JsonPropertyName("accessVector")]
    public string? AccessVector { get; set; }

    [JsonPropertyName("accessComplexity")]
    public string? AccessComplexity { get; set; }

    [JsonPropertyName("authentication")]
    public string? Authentication { get; set; }

    [JsonPropertyName("confidentialityImpact")]
    public string? ConfidentialityImpact { get; set; }

    [JsonPropertyName("integrityImpact")]
    public string? IntegrityImpact { get; set; }

    [JsonPropertyName("availabilityImpact")]
    public string? AvailabilityImpact { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }
}

public class NvdWeakness
{
    [JsonPropertyName("description")]
    public NvdWeaknessDescription[]? Description { get; set; }
}

public class NvdWeaknessDescription
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = default!;

    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;
}

public class NvdConfiguration
{
    [JsonPropertyName("nodes")]
    public NvdConfigNode[]? Nodes { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }
}

public class NvdConfigNode
{
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("negate")]
    public bool? Negate { get; set; }

    [JsonPropertyName("cpeMatch")]
    public NvdCpeMatch[]? CpeMatch { get; set; }

    [JsonPropertyName("children")]
    public NvdConfigNode[]? Children { get; set; }
}

public class NvdCpeMatch
{
    [JsonPropertyName("vulnerable")]
    public bool? Vulnerable { get; set; }

    [JsonPropertyName("criteria")]
    public string? Criteria { get; set; }

    [JsonPropertyName("matchCriteriaId")]
    public string? MatchCriteriaId { get; set; }
}

public class NvdReference
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }
}

public class NvdEnrichmentData
{
    public string? CveId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public double? CvssV31BaseScore { get; set; }
    public double? CvssV31TemporalScore { get; set; }
    public double? CvssV30BaseScore { get; set; }
    public double? CvssV2BaseScore { get; set; }
    public string? CvssV31Vector { get; set; }
    public string? CvssV31BaseSeverity { get; set; }
    public string? VulnStatus { get; set; }
    public string? CisaVulnerabilityName { get; set; }
    public string? Published { get; set; }
    public string? LastModified { get; set; }
    public string[]? CweIds { get; set; }
    public NvdReference[]? References { get; set; }
    public string? SourceIdentifier { get; set; }
}