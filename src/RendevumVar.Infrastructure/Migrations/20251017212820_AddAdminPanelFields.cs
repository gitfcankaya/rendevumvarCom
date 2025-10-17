using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RendevumVar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPanelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Salons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "Salons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Salons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Salons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing Tenants to use first user of their tenant as owner
            migrationBuilder.Sql(@"
                UPDATE T
                SET T.OwnerId = (
                    SELECT TOP 1 U.Id 
                    FROM Users U 
                    WHERE U.TenantId = T.Id 
                    ORDER BY U.CreatedAt
                )
                FROM Tenants T
                WHERE T.OwnerId = '00000000-0000-0000-0000-000000000000'
            ");

            // Set Salons to Approved status for existing records
            migrationBuilder.Sql(@"
                UPDATE Salons
                SET Status = 1, ApprovedAt = GETUTCDATE()
                WHERE Status = 0
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_OwnerId",
                table: "Tenants",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Users_OwnerId",
                table: "Tenants",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Users_OwnerId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_OwnerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Salons");
        }
    }
}
