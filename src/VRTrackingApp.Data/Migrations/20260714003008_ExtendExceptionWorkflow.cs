using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRTrackingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendExceptionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionedByUserId",
                table: "ExceptionRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "ExceptionRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingAction",
                table: "ExceptionRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingExpiresAt",
                table: "ExceptionRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingReason",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ExceptionRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "ExceptionRecords",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_ActionedByUserId",
                table: "ExceptionRecords",
                column: "ActionedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_OwnerUserId",
                table: "ExceptionRecords",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExceptionRecords_State",
                table: "ExceptionRecords",
                column: "State");

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionRecords_UserAccounts_ActionedByUserId",
                table: "ExceptionRecords",
                column: "ActionedByUserId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionRecords_UserAccounts_OwnerUserId",
                table: "ExceptionRecords",
                column: "OwnerUserId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionRecords_UserAccounts_ActionedByUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionRecords_UserAccounts_OwnerUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionRecords_ActionedByUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionRecords_OwnerUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionRecords_State",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "ActionedByUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "PendingAction",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "PendingExpiresAt",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "PendingReason",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ExceptionRecords");

            migrationBuilder.DropColumn(
                name: "State",
                table: "ExceptionRecords");
        }
    }
}
