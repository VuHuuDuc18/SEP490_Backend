using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddNewAttributeBillBarnAndLSCircle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalBill",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "PreSoldDate",
                table: "LivestockCircles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SamplePrice",
                table: "LivestockCircles",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Bills",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Barns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreSoldDate",
                table: "LivestockCircles");

            migrationBuilder.DropColumn(
                name: "SamplePrice",
                table: "LivestockCircles");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Barns");

            migrationBuilder.AddColumn<float>(
                name: "TotalBill",
                table: "Orders",
                type: "real",
                nullable: true);
        }
    }
}
