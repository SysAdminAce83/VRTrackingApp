using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    ServerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetStatus = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Datacenter = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BiaCriticality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confidentiality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Integrity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Availability = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetCriticality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Application = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveDirectoryComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BackupDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackupOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupLandscape = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationSystemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalPoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OnsiteResourceBackup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PocRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalPoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalPocRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CpuCoreCount = table.Column<int>(type: "int", nullable: true),
                    RamGb = table.Column<int>(type: "int", nullable: true),
                    DriveCount = table.Column<int>(type: "int", nullable: true),
                    TotalDiskSpaceGb = table.Column<int>(type: "int", nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HardwareDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hardware = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Software = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerName2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerIpAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherItResources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedundancyPrimaryDc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriticalRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriticalResources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InterDependencies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataReplicationFrequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeakTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OffPeakTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutageImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NonFinancialImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegulatoryImpact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Criticality2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisasterRecoveryRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrSetupDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedundancyDrDc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrioritizeResourceRecovery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumHardware = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumHardware2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumItResources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rpo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackupSchedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FirstSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VulnerabilityFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PluginId = table.Column<int>(type: "int", nullable: false),
                    PluginName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Cve = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Synopsis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskFactor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CvssV3BaseScore = table.Column<double>(type: "float", nullable: true),
                    CvssV3TemporalScore = table.Column<double>(type: "float", nullable: true),
                    CvssV2BaseScore = table.Column<double>(type: "float", nullable: true),
                    CvssV2TemporalScore = table.Column<double>(type: "float", nullable: true),
                    VprScore = table.Column<double>(type: "float", nullable: true),
                    EpssScore = table.Column<double>(type: "float", nullable: true),
                    StigSeverity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    References = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnerabilityFindings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    MfaSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MfaEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetAuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAuditTrails_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAuditTrails_UserAccounts_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScanUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScanCycleLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true),
                    UploadedById = table.Column<int>(type: "int", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanUploads_UserAccounts_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetFieldChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetAuditTrailId = table.Column<int>(type: "int", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetFieldChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetFieldChanges_AssetAuditTrails_AssetAuditTrailId",
                        column: x => x.AssetAuditTrailId,
                        principalTable: "AssetAuditTrails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetHosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanUploadId = table.Column<int>(type: "int", nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    OperatingSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetHosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetHosts_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssetHosts_ScanUploads_ScanUploadId",
                        column: x => x.ScanUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadAuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanUploadId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedById = table.Column<int>(type: "int", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadAuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadAuditTrails_ScanUploads_ScanUploadId",
                        column: x => x.ScanUploadId,
                        principalTable: "ScanUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadAuditTrails_UserAccounts_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VulnerabilityInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetHostId = table.Column<int>(type: "int", nullable: false),
                    VulnerabilityFindingId = table.Column<int>(type: "int", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PluginOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstFound = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastFound = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnerabilityInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VulnerabilityInstances_AssetHosts_AssetHostId",
                        column: x => x.AssetHostId,
                        principalTable: "AssetHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VulnerabilityInstances_UserAccounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VulnerabilityInstances_VulnerabilityFindings_VulnerabilityFindingId",
                        column: x => x.VulnerabilityFindingId,
                        principalTable: "VulnerabilityFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityInstanceId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExceptionRecords_UserAccounts_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExceptionRecords_VulnerabilityInstances_VulnerabilityInstanceId",
                        column: x => x.VulnerabilityInstanceId,
                        principalTable: "VulnerabilityInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemediationActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityInstanceId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExceptionExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvidenceFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemediationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemediationActions_UserAccounts_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RemediationActions_UserAccounts_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RemediationActions_VulnerabilityInstances_VulnerabilityInstanceId",
                        column: x => x.VulnerabilityInstanceId,
                        principalTable: "VulnerabilityInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAuditTrails_AssetId",
                table: "AssetAuditTrails",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAuditTrails_PerformedAt",
                table: "AssetAuditTrails",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAuditTrails_PerformedByUserId",
                table: "AssetAuditTrails",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetFieldChanges_AssetAuditTrailId",
                table: "AssetFieldChanges",
                column: "AssetAuditTrailId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHosts_AssetId",
                table: "AssetHosts",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHosts_IpAddress",
                table: "AssetHosts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHosts_ScanUploadId",
                table: "AssetHosts",
                column: "ScanUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetStatus",
                table: "Assets",
                column: "AssetStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Category",
                table: "Assets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Datacenter",
                table: "Assets",
                column: "Datacenter");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_Environment",
                table: "Assets",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HostName",
                table: "Assets",
                column: "HostName");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_IpAddress",
                table: "Assets",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_ApprovedById",
                table: "ExceptionRecords",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_VulnerabilityInstanceId",
                table: "ExceptionRecords",
                column: "VulnerabilityInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemediationActions_AssignedToId",
                table: "RemediationActions",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_RemediationActions_PerformedById",
                table: "RemediationActions",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_RemediationActions_VulnerabilityInstanceId",
                table: "RemediationActions",
                column: "VulnerabilityInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_ScanDate",
                table: "ScanUploads",
                column: "ScanDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_SourceType",
                table: "ScanUploads",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_ScanUploads_UploadedById",
                table: "ScanUploads",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_UploadAuditTrails_PerformedById",
                table: "UploadAuditTrails",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_UploadAuditTrails_ScanUploadId",
                table: "UploadAuditTrails",
                column: "ScanUploadId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_RoleId",
                table: "UserAccounts",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_UserName",
                table: "UserAccounts",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityFindings_PluginId",
                table: "VulnerabilityFindings",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityFindings_Severity",
                table: "VulnerabilityFindings",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityInstances_AssetHostId",
                table: "VulnerabilityInstances",
                column: "AssetHostId");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityInstances_OwnerId",
                table: "VulnerabilityInstances",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityInstances_Status",
                table: "VulnerabilityInstances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityInstances_VulnerabilityFindingId",
                table: "VulnerabilityInstances",
                column: "VulnerabilityFindingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetFieldChanges");

            migrationBuilder.DropTable(
                name: "ExceptionRecords");

            migrationBuilder.DropTable(
                name: "RemediationActions");

            migrationBuilder.DropTable(
                name: "UploadAuditTrails");

            migrationBuilder.DropTable(
                name: "AssetAuditTrails");

            migrationBuilder.DropTable(
                name: "VulnerabilityInstances");

            migrationBuilder.DropTable(
                name: "AssetHosts");

            migrationBuilder.DropTable(
                name: "VulnerabilityFindings");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "ScanUploads");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
