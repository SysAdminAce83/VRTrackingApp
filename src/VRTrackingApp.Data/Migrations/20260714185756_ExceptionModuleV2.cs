using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExceptionModuleV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AffectsAvailability",
                table: "ExceptionRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsConfidentiality",
                table: "ExceptionRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsIntegrity",
                table: "ExceptionRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessImpact",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedReason",
                table: "ExceptionRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplianceImpact",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostImpact",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentApprovalStage",
                table: "ExceptionRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerImpact",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DowntimeConstraint",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exploitability",
                table: "ExceptionRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Impact",
                table: "ExceptionRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternetExposure",
                table: "ExceptionRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Likelihood",
                table: "ExceptionRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewDate",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NonFixableReason",
                table: "ExceptionRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherReasonText",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverallRisk",
                table: "ExceptionRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionImpact",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewFrequencyDays",
                table: "ExceptionRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage1Role",
                table: "ExceptionRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ExceptionRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalJustification",
                table: "ExceptionRecords",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "AuditLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "AuditLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExceptionApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionByUserId = table.Column<int>(type: "int", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionApprovalSteps_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExceptionApprovalSteps_UserAccounts_DecisionByUserId",
                        column: x => x.DecisionByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AuthorDisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionComments_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExceptionComments_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionEvidence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    EvidenceType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionEvidence_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExceptionEvidence_UserAccounts_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionMitigations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionMitigations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionMitigations_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionReviewHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionReviewHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionReviewHistories_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExceptionReviewHistories_UserAccounts_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionSecurityControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    ControlName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionSecurityControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionSecurityControls_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExceptionRecordId = table.Column<int>(type: "int", nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ResponseText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PatchEtaDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorResponses_ExceptionRecords_ExceptionRecordId",
                        column: x => x.ExceptionRecordId,
                        principalTable: "ExceptionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_Status",
                table: "ExceptionRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionApprovalSteps_DecisionByUserId",
                table: "ExceptionApprovalSteps",
                column: "DecisionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionApprovalSteps_ExceptionRecordId",
                table: "ExceptionApprovalSteps",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionComments_ExceptionRecordId",
                table: "ExceptionComments",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionComments_UserId",
                table: "ExceptionComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionEvidence_ExceptionRecordId",
                table: "ExceptionEvidence",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionEvidence_UploadedByUserId",
                table: "ExceptionEvidence",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionMitigations_ExceptionRecordId",
                table: "ExceptionMitigations",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionReviewHistories_ExceptionRecordId",
                table: "ExceptionReviewHistories",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionReviewHistories_ReviewedByUserId",
                table: "ExceptionReviewHistories",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionSecurityControls_ExceptionRecordId",
                table: "ExceptionSecurityControls",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExceptionRecordId",
                table: "Notifications",
                column: "ExceptionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorResponses_ExceptionRecordId",
                table: "VendorResponses",
                column: "ExceptionRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExceptionApprovalSteps");

            migrationBuilder.DropTable(
                name: "ExceptionComments");

            migrationBuilder.DropTable(
                name: "ExceptionEvidence");

            migrationBuilder.DropTable(
                name: "ExceptionMitigations");

            migrationBuilder.DropTable(
                name: "ExceptionReviewHistories");

            migrationBuilder.DropTable(
                name: "ExceptionSecurityControls");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "VendorResponses");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionRecords_Status",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "AffectsAvailability",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "AffectsConfidentiality",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "AffectsIntegrity",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "BusinessImpact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ClosedReason",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ComplianceImpact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "CostImpact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "CurrentApprovalStage",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "CustomerImpact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "DowntimeConstraint",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "Exploitability",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "Impact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "InternetExposure",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "Likelihood",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "NonFixableReason",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "OtherReasonText",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "OverallRisk",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ProductionImpact",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ReviewFrequencyDays",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "Stage1Role",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "TechnicalJustification",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "AuditLogs");
        }
    }
}
