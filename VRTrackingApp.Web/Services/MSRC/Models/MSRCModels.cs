using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VRTrackingApp.Web.Services.MSRC.Models;

public class UpdateSummary
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("initialReleaseDate")]
    public DateTimeOffset? InitialReleaseDate { get; set; }

    [JsonPropertyName("currentReleaseDate")]
    public DateTimeOffset? CurrentReleaseDate { get; set; }

    [JsonPropertyName("cvrfUrl")]
    public string? CvrfUrl { get; set; }

    [JsonPropertyName("csafUrl")]
    public string? CsafUrl { get; set; }

    [JsonPropertyName("cves")]
    public string[]? Cves { get; set; }

    [JsonPropertyName("kbNumbers")]
    public string[]? KbNumbers { get; set; }

    [JsonPropertyName("impact")]
    public string? Impact { get; set; }

    [JsonPropertyName("exploited")]
    public bool? Exploited { get; set; }

    [JsonPropertyName("latestRevisionNumber")]
    public int? LatestRevisionNumber { get; set; }

    [JsonPropertyName("workarounds")]
    public string? Workarounds { get; set; }

    [JsonPropertyName("faqUrl")]
    public string? FaqUrl { get; set; }

    [JsonPropertyName("affectedProducts")]
    public AffectedProduct[]? AffectedProducts { get; set; }
}

public class AffectedProduct
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("architecture")]
    public string? Architecture { get; set; }

    [JsonPropertyName("servicePack")]
    public string? ServicePack { get; set; }

    [JsonPropertyName("kbNumbers")]
    public string[]? KbNumbers { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("rebootRequired")]
    public bool? RebootRequired { get; set; }
}

public class UpdateResponse
{
    [JsonPropertyName("value")]
    public UpdateSummary[]? Value { get; set; }

    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }

    [JsonPropertyName("@odata.nextLink")]
    public string? NextLink { get; set; }
}

public class CVRFDocument
{
    [JsonPropertyName("DocumentTitle")]
    public string? DocumentTitle { get; set; }

    [JsonPropertyName("DocumentType")]
    public string? DocumentType { get; set; }

    [JsonPropertyName("DocumentPublisher")]
    public DocumentPublisher? DocumentPublisher { get; set; }

    [JsonPropertyName("DocumentTracking")]
    public DocumentTracking? DocumentTracking { get; set; }

    [JsonPropertyName("DocumentNotes")]
    public DocumentNote[]? DocumentNotes { get; set; }

    [JsonPropertyName("ProductTree")]
    public ProductTree? ProductTree { get; set; }

    [JsonPropertyName("Vulnerability")]
    public CVRFVulnerability[]? Vulnerabilities { get; set; }
}

public class DocumentPublisher
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("ContactDetails")]
    public string? ContactDetails { get; set; }
}

public class DocumentTracking
{
    [JsonPropertyName("Identification")]
    public TrackingIdentification? Identification { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Version")]
    public string? Version { get; set; }

    [JsonPropertyName("RevisionHistory")]
    public RevisionHistoryEntry[]? RevisionHistory { get; set; }

    [JsonPropertyName("InitialReleaseDate")]
    public DateTimeOffset? InitialReleaseDate { get; set; }

    [JsonPropertyName("CurrentReleaseDate")]
    public DateTimeOffset? CurrentReleaseDate { get; set; }

    [JsonPropertyName("Generator")]
    public Generator? Generator { get; set; }
}

public class TrackingIdentification
{
    [JsonPropertyName("ID")]
    public string? ID { get; set; }
}

public class RevisionHistoryEntry
{
    [JsonPropertyName("Number")]
    public string? Number { get; set; }

    [JsonPropertyName("Date")]
    public DateTimeOffset? Date { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}

public class Generator
{
    [JsonPropertyName("Date")]
    public DateTimeOffset? Date { get; set; }

    [JsonPropertyName("Engine")]
    public Engine? Engine { get; set; }
}

public class Engine
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Version")]
    public string? Version { get; set; }
}

public class DocumentNote
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Text")]
    public string? Text { get; set; }
}

public class ProductTree
{
    [JsonPropertyName("Branch")]
    public ProductBranch[]? Branches { get; set; }

    [JsonPropertyName("FullProductName")]
    public FullProductName[]? FullProductNames { get; set; }
}

public class ProductBranch
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Branch")]
    public ProductBranch[]? Branches { get; set; }

    [JsonPropertyName("FullProductName")]
    public FullProductName[]? FullProductNames { get; set; }
}

public class FullProductName
{
    [JsonPropertyName("ProductID")]
    public string? ProductID { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("ProductIDRef")]
    public string[]? ProductIDRef { get; set; }
}

public class CVRFVulnerability
{
    [JsonPropertyName("CVE")]
    public string? CVE { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Notes")]
    public VulnerabilityNote[]? Notes { get; set; }

    [JsonPropertyName("Threats")]
    public Threat[]? Threats { get; set; }

    [JsonPropertyName("CVSSScoreSets")]
    public CVSSScoreSet[]? CVSSScoreSets { get; set; }

    [JsonPropertyName("Remediations")]
    public Remediation[]? Remediations { get; set; }

    [JsonPropertyName("ProductIDs")]
    public string[]? ProductIDs { get; set; }

    [JsonPropertyName("References")]
    public Reference[]? References { get; set; }
}

public class VulnerabilityNote
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }

    [JsonPropertyName("Text")]
    public string? Text { get; set; }
}

public class Threat
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }
}

public class CVSSScoreSet
{
    [JsonPropertyName("BaseScoreV2")]
    public string? BaseScoreV2 { get; set; }

    [JsonPropertyName("BaseScoreV3")]
    public string? BaseScoreV3 { get; set; }

    [JsonPropertyName("TemporalScoreV2")]
    public string? TemporalScoreV2 { get; set; }

    [JsonPropertyName("TemporalScoreV3")]
    public string? TemporalScoreV3 { get; set; }

    [JsonPropertyName("VectorV2")]
    public string? VectorV2 { get; set; }

    [JsonPropertyName("VectorV3")]
    public string? VectorV3 { get; set; }
}

public class Remediation
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("URL")]
    public string? URL { get; set; }

    [JsonPropertyName("ProductIDs")]
    public string[]? ProductIDs { get; set; }
}

public class Reference
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("URL")]
    public string? URL { get; set; }
}

public class CSAFDocument
{
    [JsonPropertyName("document")]
    public CSAFDocumentMetadata? Document { get; set; }

    [JsonPropertyName("product_tree")]
    public CSAFProductTree? ProductTree { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public CSAFVulnerability[]? Vulnerabilities { get; set; }
}

public class CSAFDocumentMetadata
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("publisher")]
    public CSAFPublisher? Publisher { get; set; }

    [JsonPropertyName("tracking")]
    public CSAFTracking? Tracking { get; set; }

    [JsonPropertyName("notes")]
    public CSAFNote[]? Notes { get; set; }
}

public class CSAFPublisher
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }
}

public class CSAFTracking
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("initial_release_date")]
    public DateTimeOffset? InitialReleaseDate { get; set; }

    [JsonPropertyName("current_release_date")]
    public DateTimeOffset? CurrentReleaseDate { get; set; }

    [JsonPropertyName("revision_history")]
    public CSAFRevisionHistoryEntry[]? RevisionHistory { get; set; }
}

public class CSAFRevisionHistoryEntry
{
    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("date")]
    public DateTimeOffset? Date { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}

public class CSAFNote
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class CSAFProductTree
{
    [JsonPropertyName("branches")]
    public CSAFBranch[]? Branches { get; set; }

    [JsonPropertyName("product_groups")]
    public CSAFProductGroup[]? ProductGroups { get; set; }
}

public class CSAFBranch
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("branches")]
    public CSAFBranch[]? Branches { get; set; }
}

public class CSAFProductGroup
{
    [JsonPropertyName("group_id")]
    public string? GroupId { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("product_ids")]
    public string[]? ProductIds { get; set; }
}

public class CSAFVulnerability
{
    [JsonPropertyName("cve")]
    public string? CVE { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("notes")]
    public CSAFNote[]? Notes { get; set; }

    [JsonPropertyName("threats")]
    public CSAFThreat[]? Threats { get; set; }

    [JsonPropertyName("scores")]
    public CSAFScore[]? Scores { get; set; }

    [JsonPropertyName("remediations")]
    public CSAFRemediation[]? Remediations { get; set; }

    [JsonPropertyName("product_ids")]
    public string[]? ProductIds { get; set; }

    [JsonPropertyName("references")]
    public CSAFReference[]? References { get; set; }
}

public class CSAFThreat
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}

public class CSAFScore
{
    [JsonPropertyName("cvss_v3")]
    public CSAFCVSSv3? CvssV3 { get; set; }

    [JsonPropertyName("cvss_v2")]
    public CSAFCVSSv2? CvssV2 { get; set; }
}

public class CSAFCVSSv3
{
    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }

    [JsonPropertyName("baseScore")]
    public double? BaseScore { get; set; }

    [JsonPropertyName("baseSeverity")]
    public string? BaseSeverity { get; set; }
}

public class CSAFCVSSv2
{
    [JsonPropertyName("vectorString")]
    public string? VectorString { get; set; }

    [JsonPropertyName("baseScore")]
    public double? BaseScore { get; set; }
}

public class CSAFRemediation
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("product_ids")]
    public string[]? ProductIds { get; set; }
}

public class CSAFReference
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class MSRCEnrichmentData
{
    public string? MicrosoftAdvisoryId { get; set; }
    public string? MicrosoftBulletinId { get; set; }
    public string[]? KBNumbers { get; set; }
    public string[]? PatchDownloadUrls { get; set; }
    public bool? RequiresReboot { get; set; }
    public string? SupersededBy { get; set; }
    public string? ExploitabilityAssessment { get; set; }
    public DateTimeOffset? MicrosoftReleaseDate { get; set; }
    public AffectedProduct[]? AffectedProducts { get; set; }
    public string? Workaround { get; set; }
    public string? FAQUrl { get; set; }
    public string? CVRFId { get; set; }
    public string? CSAFId { get; set; }
}