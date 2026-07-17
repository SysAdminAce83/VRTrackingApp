using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRemediationJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RemediationJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VulnerabilityInstanceId = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetHost = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PatchId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCriticalAsset = table.Column<bool>(type: "bit", nullable: false),
                    ResultSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Log = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemediationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemediationJobs_UserAccounts_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RemediationJobs_VulnerabilityInstances_VulnerabilityInstanceId",
                        column: x => x.VulnerabilityInstanceId,
                        principalTable: "VulnerabilityInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemediationJobs_RequestedByUserId",
                table: "RemediationJobs",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RemediationJobs_State",
                table: "RemediationJobs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_RemediationJobs_VulnerabilityInstanceId",
                table: "RemediationJobs",
                column: "VulnerabilityInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RemediationJobs");
        }
    }
}
