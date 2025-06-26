using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class livetockCircleConnect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LivestockCircleId",
                table: "BarnPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BarnPlans_LivestockCircleId",
                table: "BarnPlans",
                column: "LivestockCircleId");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlans_LivestockCircles_LivestockCircleId",
                table: "BarnPlans",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlans_LivestockCircles_LivestockCircleId",
                table: "BarnPlans");

            migrationBuilder.DropIndex(
                name: "IX_BarnPlans_LivestockCircleId",
                table: "BarnPlans");

            migrationBuilder.DropColumn(
                name: "LivestockCircleId",
                table: "BarnPlans");
        }
    }
}
