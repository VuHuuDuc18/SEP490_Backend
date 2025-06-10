using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initStaticRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("152f58f4-b35b-49e9-8eeb-c6ea8bb628aa"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4989), true, "Farmer", null, null },
                    { new Guid("22dd561f-98db-4272-a70c-d725756176f5"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4987), true, "Weighing Room Staff", null, null },
                    { new Guid("8bea356e-ba31-408d-8af4-847a7795229c"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4963), true, "Company Admin", null, null },
                    { new Guid("935a0aa2-ac35-4e3b-bee8-2e83cfa881b2"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4981), true, "Feed Room Staff", null, null },
                    { new Guid("9acc44be-f14d-402d-9d00-f31ee0826081"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4977), true, "Technical Staff", null, null },
                    { new Guid("c6f6f482-ba36-477b-98eb-e881d485b923"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4999), true, "Sales Staff", null, null },
                    { new Guid("d8ad3fa3-113c-48ac-8abd-658e5af95712"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(4984), true, "Medicine Room Staff", null, null },
                    { new Guid("fbcfb2f8-cf83-43a3-bd07-0a69726e239f"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(5002), true, "Customer", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("1f721ba7-08cf-415a-832f-6e6b7e01b782"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 10, 17, 44, 17, 749, DateTimeKind.Local).AddTicks(5132), "admin@a", true, "123", new Guid("8bea356e-ba31-408d-8af4-847a7795229c"), null, null, "Company Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("152f58f4-b35b-49e9-8eeb-c6ea8bb628aa"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("22dd561f-98db-4272-a70c-d725756176f5"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("935a0aa2-ac35-4e3b-bee8-2e83cfa881b2"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("9acc44be-f14d-402d-9d00-f31ee0826081"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("c6f6f482-ba36-477b-98eb-e881d485b923"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d8ad3fa3-113c-48ac-8abd-658e5af95712"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("fbcfb2f8-cf83-43a3-bd07-0a69726e239f"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1f721ba7-08cf-415a-832f-6e6b7e01b782"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("8bea356e-ba31-408d-8af4-847a7795229c"));
        }
    }
}
