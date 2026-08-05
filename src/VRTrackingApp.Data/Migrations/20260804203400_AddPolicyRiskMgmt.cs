using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyRiskMgmt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ComplianceControls_ControlFamily",
                table: "ComplianceControls");

            migrationBuilder.DropColumn(
                name: "ControlFamily",
                table: "ComplianceControls");

            migrationBuilder.CreateTable(
                name: "ControlLibraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ControlId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ControlName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ControlDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlOwner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Evidence = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TestSteps = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RiskAddressed = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ControlLibraries_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ControlLibraries_UserAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Policies_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Policies_UserAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Risks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessImpact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Likelihood = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Risks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Risks_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Risks_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Risks_UserAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Standards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Standards_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Standards_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Standards_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Standards_UserAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Procedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StandardId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Procedures_Standards_StandardId",
                        column: x => x.StandardId,
                        principalTable: "Standards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Procedures_UserAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Procedures_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Procedures_UserAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ControlLibraries_ControlId",
                table: "ControlLibraries",
                column: "ControlId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ControlLibraries_CreatedById",
                table: "ControlLibraries",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ControlLibraries_UpdatedById",
                table: "ControlLibraries",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CreatedById",
                table: "Policies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_OwnerUserId",
                table: "Policies",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Title",
                table: "Policies",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_UpdatedById",
                table: "Policies",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_CreatedById",
                table: "Procedures",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_OwnerUserId",
                table: "Procedures",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_StandardId",
                table: "Procedures",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_Title",
                table: "Procedures",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Procedures_UpdatedById",
                table: "Procedures",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_CreatedById",
                table: "Risks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_OwnerUserId",
                table: "Risks",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskName",
                table: "Risks",
                column: "RiskName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Risks_UpdatedById",
                table: "Risks",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_CreatedById",
                table: "Standards",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_OwnerUserId",
                table: "Standards",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_PolicyId",
                table: "Standards",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_Title",
                table: "Standards",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_UpdatedById",
                table: "Standards",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControlLibraries");

            migrationBuilder.DropTable(
                name: "Procedures");

            migrationBuilder.DropTable(
                name: "Risks");

            migrationBuilder.DropTable(
                name: "Standards");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.AddColumn<string>(
                name: "ControlFamily",
                table: "ComplianceControls",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceControls_ControlFamily",
                table: "ComplianceControls",
                column: "ControlFamily");
        }
    }
}
