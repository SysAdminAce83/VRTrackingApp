using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScanDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "ScanUploads",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "ScanUploads",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Md5Hash",
                table: "ScanUploads",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScanGroupId",
                table: "ScanUploads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeduplicationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanUploadId = table.Column<int>(type: "int", nullable: false),
                    VulnerabilityKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    VulnerabilityInstanceId = table.Column<int>(type: "int", nullable: true),
                    PluginId = table.Column<int>(type: "int", nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Cve = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchedExistingInstanceId = table.Column<int>(type: "int", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeduplicationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeduplicationLogs_ScanUploads_ScanUploadId",
                        column: x => x.ScanUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeduplicationLogs_VulnerabilityInstances_VulnerabilityInstanceId",
                        column: x => x.VulnerabilityInstanceId,
                        principalTable: "VulnerabilityInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScanGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NessusScanUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ScannerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PolicyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PolicyId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ScanStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScanEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScanCycleLabel = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IngestState = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalUploads = table.Column<int>(type: "int", nullable: false),
                    TotalFindings = table.Column<int>(type: "int", nullable: false),
                    TotalInstances = table.Column<int>(type: "int", nullable: false),
                    TotalHosts = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScanMetadatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanUploadId = table.Column<int>(type: "int", nullable: false),
                    NessusScanUuid = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ScannerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PolicyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PolicyId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ScanStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScanEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScanTarget = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Preference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanMetadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanMetadatas_ScanUploads_ScanUploadId",
                        column: x => x.ScanUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngestionAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanUploadId = table.Column<int>(type: "int", nullable: false),
                    ScanGroupId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedById = table.Column<int>(type: "int", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DuplicateStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewFindings = table.Column<int>(type: "int", nullable: false),
                    ExistingFindings = table.Column<int>(type: "int", nullable: false),
                    ReopenedFindings = table.Column<int>(type: "int", nullable: false),
                    RemediatedFindings = table.Column<int>(type: "int", nullable: false),
                    RejectedFindings = table.Column<int>(type: "int", nullable: false),
                    ProcessingMs = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProcessingLog = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngestionAudits_ScanGroups_ScanGroupId",
                        column: x => x.ScanGroupId,
                        principalTable: "ScanGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IngestionAudits_ScanUploads_ScanUploadId",
                        column: x => x.ScanUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngestionAudits_UserAccounts_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScanIngestionLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanGroupId = table.Column<int>(type: "int", nullable: false),
                    OwnerUploadId = table.Column<int>(type: "int", nullable: true),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockedByUserId = table.Column<int>(type: "int", nullable: true),
                    LockedById = table.Column<int>(type: "int", nullable: true),
                    AcquiredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanIngestionLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanIngestionLocks_ScanGroups_ScanGroupId",
                        column: x => x.ScanGroupId,
                        principalTable: "ScanGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScanIngestionLocks_ScanUploads_OwnerUploadId",
                        column: x => x.OwnerUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScanIngestionLocks_UserAccounts_LockedById",
                        column: x => x.LockedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_FileHash",
                table: "ScanUploads",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_Format",
                table: "ScanUploads",
                column: "Format");

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_ScanGroupId",
                table: "ScanUploads",
                column: "ScanGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DeduplicationLogs_PluginId_HostName_Port_Protocol",
                table: "DeduplicationLogs",
                columns: new[] { "PluginId", "HostName", "Port", "Protocol" });

            migrationBuilder.CreateIndex(
                name: "IX_DeduplicationLogs_ScanUploadId",
                table: "DeduplicationLogs",
                column: "ScanUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_DeduplicationLogs_VulnerabilityInstanceId",
                table: "DeduplicationLogs",
                column: "VulnerabilityInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeduplicationLogs_VulnerabilityKey",
                table: "DeduplicationLogs",
                column: "VulnerabilityKey");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionAudits_PerformedAt",
                table: "IngestionAudits",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionAudits_PerformedById",
                table: "IngestionAudits",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionAudits_ScanGroupId",
                table: "IngestionAudits",
                column: "ScanGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionAudits_ScanUploadId",
                table: "IngestionAudits",
                column: "ScanUploadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanGroups_IngestState",
                table: "ScanGroups",
                column: "IngestState");

            migrationBuilder.CreateIndex(
                name: "IX_ScanGroups_NessusScanUuid",
                table: "ScanGroups",
                column: "NessusScanUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ScanGroups_ScanKey",
                table: "ScanGroups",
                column: "ScanKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanIngestionLocks_LockedById",
                table: "ScanIngestionLocks",
                column: "LockedById");

            migrationBuilder.CreateIndex(
                name: "IX_ScanIngestionLocks_OwnerUploadId",
                table: "ScanIngestionLocks",
                column: "OwnerUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanIngestionLocks_ScanGroupId",
                table: "ScanIngestionLocks",
                column: "ScanGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanIngestionLocks_State",
                table: "ScanIngestionLocks",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_ScanMetadatas_ScanUploadId",
                table: "ScanMetadatas",
                column: "ScanUploadId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScanUploads_ScanGroups_ScanGroupId",
                table: "ScanUploads",
                column: "ScanGroupId",
                principalTable: "ScanGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScanUploads_ScanGroups_ScanGroupId",
                table: "ScanUploads");

            migrationBuilder.DropTable(
                name: "DeduplicationLogs");

            migrationBuilder.DropTable(
                name: "IngestionAudits");

            migrationBuilder.DropTable(
                name: "ScanIngestionLocks");

            migrationBuilder.DropTable(
                name: "ScanMetadatas");

            migrationBuilder.DropTable(
                name: "ScanGroups");

            migrationBuilder.DropIndex(
                name: "IX_ScanUploads_FileHash",
                table: "ScanUploads");

            migrationBuilder.DropIndex(
                name: "IX_ScanUploads_Format",
                table: "ScanUploads");

            migrationBuilder.DropIndex(
                name: "IX_ScanUploads_ScanGroupId",
                table: "ScanUploads");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "ScanUploads");

            migrationBuilder.DropColumn(
                name: "Md5Hash",
                table: "ScanUploads");

            migrationBuilder.DropColumn(
                name: "ScanGroupId",
                table: "ScanUploads");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "ScanUploads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
