using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplianceGRCModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AffectedProducts",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CSAFId",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVRFId",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExploitabilityAssessment",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FAQUrl",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KBNumbers",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEnrichedAt",
                table: "VulnerabilityFindings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftAdvisoryId",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftBulletinId",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MicrosoftReleaseDate",
                table: "VulnerabilityFindings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatchDownloadUrls",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReboot",
                table: "VulnerabilityFindings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupersededBy",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Workaround",
                table: "VulnerabilityFindings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Frameworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frameworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketingLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    System = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TicketId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TicketUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkedByUserId = table.Column<int>(type: "int", nullable: true),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketingLinks_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketingLinks_UserAccounts_LinkedByUserId",
                        column: x => x.LinkedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ControlFamilies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FrameworkId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlFamilies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlFamilies_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "Frameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControlId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Framework = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FrameworkVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ControlFamily = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ControlFamilyId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Impact = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ControlFamilyId1 = table.Column<int>(type: "int", nullable: true),
                    FrameworkId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceControls_ControlFamilies_ControlFamilyId",
                        column: x => x.ControlFamilyId,
                        principalTable: "ControlFamilies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceControls_ControlFamilies_ControlFamilyId1",
                        column: x => x.ControlFamilyId1,
                        principalTable: "ControlFamilies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceControls_Frameworks_FrameworkId",
                        column: x => x.FrameworkId,
                        principalTable: "Frameworks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityFindingId = table.Column<int>(type: "int", nullable: false),
                    ComplianceControlId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EvidenceRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReviewerNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsException = table.Column<bool>(type: "bit", nullable: false),
                    ExceptionExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceReviews_ComplianceControls_ComplianceControlId",
                        column: x => x.ComplianceControlId,
                        principalTable: "ComplianceControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplianceReviews_VulnerabilityFindings_VulnerabilityFindingId",
                        column: x => x.VulnerabilityFindingId,
                        principalTable: "VulnerabilityFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FindingComplianceLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityFindingId = table.Column<int>(type: "int", nullable: false),
                    ComplianceControlId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rationale = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EvidenceRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FindingComplianceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FindingComplianceLinks_ComplianceControls_ComplianceControlId",
                        column: x => x.ComplianceControlId,
                        principalTable: "ComplianceControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FindingComplianceLinks_VulnerabilityFindings_VulnerabilityFindingId",
                        column: x => x.VulnerabilityFindingId,
                        principalTable: "VulnerabilityFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAcceptances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityFindingId = table.Column<int>(type: "int", nullable: false),
                    ComplianceControlId = table.Column<int>(type: "int", nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AcceptedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAcceptances_ComplianceControls_ComplianceControlId",
                        column: x => x.ComplianceControlId,
                        principalTable: "ComplianceControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskAcceptances_VulnerabilityFindings_VulnerabilityFindingId",
                        column: x => x.VulnerabilityFindingId,
                        principalTable: "VulnerabilityFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ControlEvidences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplianceControlId = table.Column<int>(type: "int", nullable: false),
                    FindingComplianceLinkId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlEvidences_ComplianceControls_ComplianceControlId",
                        column: x => x.ComplianceControlId,
                        principalTable: "ComplianceControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ControlEvidences_FindingComplianceLinks_FindingComplianceLinkId",
                        column: x => x.FindingComplianceLinkId,
                        principalTable: "FindingComplianceLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EvidenceAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControlEvidenceId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceAttachments_ControlEvidences_ControlEvidenceId",
                        column: x => x.ControlEvidenceId,
                        principalTable: "ControlEvidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_ControlFamily",
                table: "ComplianceControls",
                column: "ControlFamily");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_ControlFamilyId",
                table: "ComplianceControls",
                column: "ControlFamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_ControlFamilyId1",
                table: "ComplianceControls",
                column: "ControlFamilyId1");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_ControlId",
                table: "ComplianceControls",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_Framework",
                table: "ComplianceControls",
                column: "Framework");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_FrameworkId",
                table: "ComplianceControls",
                column: "FrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReviews_ComplianceControlId",
                table: "ComplianceReviews",
                column: "ComplianceControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceReviews_VulnerabilityFindingId",
                table: "ComplianceReviews",
                column: "VulnerabilityFindingId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlEvidences_ComplianceControlId",
                table: "ControlEvidences",
                column: "ComplianceControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlEvidences_FindingComplianceLinkId",
                table: "ControlEvidences",
                column: "FindingComplianceLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlFamilies_FamilyId",
                table: "ControlFamilies",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_ControlFamilies_FrameworkId",
                table: "ControlFamilies",
                column: "FrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceAttachments_ControlEvidenceId",
                table: "EvidenceAttachments",
                column: "ControlEvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FindingComplianceLinks_ComplianceControlId",
                table: "FindingComplianceLinks",
                column: "ComplianceControlId");

            migrationBuilder.CreateIndex(
                name: "IX_FindingComplianceLinks_VulnerabilityFindingId",
                table: "FindingComplianceLinks",
                column: "VulnerabilityFindingId");

            migrationBuilder.CreateIndex(
                name: "IX_FindingComplianceLinks_VulnerabilityFindingId_ComplianceControlId",
                table: "FindingComplianceLinks",
                columns: new[] { "VulnerabilityFindingId", "ComplianceControlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Frameworks_ShortName",
                table: "Frameworks",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAcceptances_ComplianceControlId",
                table: "RiskAcceptances",
                column: "ComplianceControlId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAcceptances_VulnerabilityFindingId",
                table: "RiskAcceptances",
                column: "VulnerabilityFindingId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketingLinks_ExceptionRecordId",
                table: "TicketingLinks",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketingLinks_LinkedByUserId",
                table: "TicketingLinks",
                column: "LinkedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "ComplianceReviews");

            migrationBuilder.DropTable(
                name: "EvidenceAttachments");

            migrationBuilder.DropTable(
                name: "RiskAcceptances");

            migrationBuilder.DropTable(
                name: "TicketingLinks");

            migrationBuilder.DropTable(
                name: "ControlEvidences");

            migrationBuilder.DropTable(
                name: "FindingComplianceLinks");

            migrationBuilder.DropTable(
                name: "ComplianceControls");

            migrationBuilder.DropTable(
                name: "ControlFamilies");

            migrationBuilder.DropTable(
                name: "Frameworks");

            migrationBuilder.DropColumn(
                name: "AffectedProducts",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "CSAFId",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "CVRFId",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "ExploitabilityAssessment",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "FAQUrl",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "KBNumbers",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "LastEnrichedAt",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "MicrosoftAdvisoryId",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "MicrosoftBulletinId",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "MicrosoftReleaseDate",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "PatchDownloadUrls",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "RequiresReboot",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "SupersededBy",
                table: "VulnerabilityFindings");

            migrationBuilder.DropColumn(
                name: "Workaround",
                table: "VulnerabilityFindings");
        }
    }
}
