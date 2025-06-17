using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseAddImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanFoods_BarnPlans_BarnPlanId",
                table: "BarnPlanFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanFoods_Foods_FoodId",
                table: "BarnPlanFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanMedicines_BarnPlans_BarnPlanId",
                table: "BarnPlanMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                table: "BarnPlanMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_Barns_Users_WorkerId",
                table: "Barns");

            migrationBuilder.DropForeignKey(
                name: "FK_BillItems_Bills_BillId",
                table: "BillItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_LivestockCircles_LivestockCircleId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Users_UserRequestId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Breeds_BreedCategories_BreedCategoryId",
                table: "Breeds");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                table: "DailyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_DailyReports_ReportId",
                table: "FoodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_Foods_FoodId",
                table: "FoodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleFoods_Foods_FoodId",
                table: "LivestockCircleFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleMedicines_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                table: "LivestockCircleMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Barns_BarnId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Breeds_BreedId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Users_TechicalStaffId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineReports_DailyReports_ReportId",
                table: "MedicineReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineReports_Medicines_MedicineId",
                table: "MedicineReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicines_MedicineCategories_MedicineCategoryId",
                table: "Medicines");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LivestockCircles_LivestockCircleId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Role_RoleId",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("1c55cb77-3231-4e53-a6bc-6283e801584e"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("278a5b96-fff1-4ca2-aafb-d4e3a34c8199"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3ff7b9b8-90c1-422e-965f-839253e65f38"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("7e8c02b9-0c99-475d-8606-061a2072d372"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("82e46aa3-2b1d-4ddc-9402-0868b0ce8417"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("84502880-8661-439b-a58c-57209a66bf37"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d8b030ab-3d10-4a64-b0a9-46cccb7d160f"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("2b4803ba-3c80-4e31-b240-02c8d954bf3b"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("37557cd8-7b8f-4543-b296-12adb3b5f8de"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("3cae6d90-f805-4ecc-99be-7ab99be2d226"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1309), true, "Customer", null, null },
                    { new Guid("3e6bca2d-9b75-4ad3-9159-a1fbbd9ad6ab"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1308), true, "Sales Staff", null, null },
                    { new Guid("838e2cc1-cf5f-49f3-87c2-8226403031ee"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1304), true, "Weighing Room Staff", null, null },
                    { new Guid("a88b9793-0a15-479e-81f1-623e97a2105d"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1269), true, "Company Admin", null, null },
                    { new Guid("ab55211b-b677-47a8-882b-405156c40d49"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1290), true, "Feed Room Staff", null, null },
                    { new Guid("d37cde57-531f-4990-b766-3461750e87a0"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1306), true, "Worker", null, null },
                    { new Guid("d3eb1ae9-46a5-4859-8fe7-49d1e71c157b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1292), true, "Medicine Room Staff", null, null },
                    { new Guid("f2789395-47df-4536-9904-bbed0db60ce2"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1288), true, "Technical Staff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("110f78d4-bb2d-4f11-98aa-bbe4d7c310d1"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 49, 10, 570, DateTimeKind.Local).AddTicks(1483), "admin@a", true, "123", new Guid("a88b9793-0a15-479e-81f1-623e97a2105d"), null, null, "Company Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanFoods_BarnPlans_BarnPlanId",
                table: "BarnPlanFoods",
                column: "BarnPlanId",
                principalTable: "BarnPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanFoods_Foods_FoodId",
                table: "BarnPlanFoods",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanMedicines_BarnPlans_BarnPlanId",
                table: "BarnPlanMedicines",
                column: "BarnPlanId",
                principalTable: "BarnPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                table: "BarnPlanMedicines",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Barns_Users_WorkerId",
                table: "Barns",
                column: "WorkerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_BillItems_Bills_BillId",
                table: "BillItems",
                column: "BillId",
                principalTable: "Bills",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_LivestockCircles_LivestockCircleId",
                table: "Bills",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Users_UserRequestId",
                table: "Bills",
                column: "UserRequestId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Breeds_BreedCategories_BreedCategoryId",
                table: "Breeds",
                column: "BreedCategoryId",
                principalTable: "BreedCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                table: "DailyReports",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodReports_DailyReports_ReportId",
                table: "FoodReports",
                column: "ReportId",
                principalTable: "DailyReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FoodReports_Foods_FoodId",
                table: "FoodReports",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleFoods_Foods_FoodId",
                table: "LivestockCircleFoods",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleFoods",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleMedicines_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleMedicines",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                table: "LivestockCircleMedicines",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Barns_BarnId",
                table: "LivestockCircles",
                column: "BarnId",
                principalTable: "Barns",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Breeds_BreedId",
                table: "LivestockCircles",
                column: "BreedId",
                principalTable: "Breeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Users_TechicalStaffId",
                table: "LivestockCircles",
                column: "TechicalStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineReports_DailyReports_ReportId",
                table: "MedicineReports",
                column: "ReportId",
                principalTable: "DailyReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineReports_Medicines_MedicineId",
                table: "MedicineReports",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicines_MedicineCategories_MedicineCategoryId",
                table: "Medicines",
                column: "MedicineCategoryId",
                principalTable: "MedicineCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LivestockCircles_LivestockCircleId",
                table: "Orders",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Role_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanFoods_BarnPlans_BarnPlanId",
                table: "BarnPlanFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanFoods_Foods_FoodId",
                table: "BarnPlanFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanMedicines_BarnPlans_BarnPlanId",
                table: "BarnPlanMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                table: "BarnPlanMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_Barns_Users_WorkerId",
                table: "Barns");

            migrationBuilder.DropForeignKey(
                name: "FK_BillItems_Bills_BillId",
                table: "BillItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_LivestockCircles_LivestockCircleId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Users_UserRequestId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Breeds_BreedCategories_BreedCategoryId",
                table: "Breeds");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                table: "DailyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_DailyReports_ReportId",
                table: "FoodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_Foods_FoodId",
                table: "FoodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleFoods_Foods_FoodId",
                table: "LivestockCircleFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleFoods");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleMedicines_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                table: "LivestockCircleMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Barns_BarnId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Breeds_BreedId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_LivestockCircles_Users_TechicalStaffId",
                table: "LivestockCircles");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineReports_DailyReports_ReportId",
                table: "MedicineReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineReports_Medicines_MedicineId",
                table: "MedicineReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicines_MedicineCategories_MedicineCategoryId",
                table: "Medicines");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LivestockCircles_LivestockCircleId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Role_RoleId",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3cae6d90-f805-4ecc-99be-7ab99be2d226"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3e6bca2d-9b75-4ad3-9159-a1fbbd9ad6ab"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("838e2cc1-cf5f-49f3-87c2-8226403031ee"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("ab55211b-b677-47a8-882b-405156c40d49"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d37cde57-531f-4990-b766-3461750e87a0"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d3eb1ae9-46a5-4859-8fe7-49d1e71c157b"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("f2789395-47df-4536-9904-bbed0db60ce2"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("110f78d4-bb2d-4f11-98aa-bbe4d7c310d1"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a88b9793-0a15-479e-81f1-623e97a2105d"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("1c55cb77-3231-4e53-a6bc-6283e801584e"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(79), true, "Technical Staff", null, null },
                    { new Guid("278a5b96-fff1-4ca2-aafb-d4e3a34c8199"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(81), true, "Feed Room Staff", null, null },
                    { new Guid("37557cd8-7b8f-4543-b296-12adb3b5f8de"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(58), true, "Company Admin", null, null },
                    { new Guid("3ff7b9b8-90c1-422e-965f-839253e65f38"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(98), true, "Customer", null, null },
                    { new Guid("7e8c02b9-0c99-475d-8606-061a2072d372"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(86), true, "Worker", null, null },
                    { new Guid("82e46aa3-2b1d-4ddc-9402-0868b0ce8417"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(88), true, "Sales Staff", null, null },
                    { new Guid("84502880-8661-439b-a58c-57209a66bf37"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(83), true, "Medicine Room Staff", null, null },
                    { new Guid("d8b030ab-3d10-4a64-b0a9-46cccb7d160f"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(85), true, "Weighing Room Staff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("2b4803ba-3c80-4e31-b240-02c8d954bf3b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 4, 44, 40, 868, DateTimeKind.Local).AddTicks(261), "admin@a", true, "123", new Guid("37557cd8-7b8f-4543-b296-12adb3b5f8de"), null, null, "Company Admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanFoods_BarnPlans_BarnPlanId",
                table: "BarnPlanFoods",
                column: "BarnPlanId",
                principalTable: "BarnPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanFoods_Foods_FoodId",
                table: "BarnPlanFoods",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanMedicines_BarnPlans_BarnPlanId",
                table: "BarnPlanMedicines",
                column: "BarnPlanId",
                principalTable: "BarnPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                table: "BarnPlanMedicines",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Barns_Users_WorkerId",
                table: "Barns",
                column: "WorkerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillItems_Bills_BillId",
                table: "BillItems",
                column: "BillId",
                principalTable: "Bills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_LivestockCircles_LivestockCircleId",
                table: "Bills",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Users_UserRequestId",
                table: "Bills",
                column: "UserRequestId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Breeds_BreedCategories_BreedCategoryId",
                table: "Breeds",
                column: "BreedCategoryId",
                principalTable: "BreedCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                table: "DailyReports",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodReports_DailyReports_ReportId",
                table: "FoodReports",
                column: "ReportId",
                principalTable: "DailyReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodReports_Foods_FoodId",
                table: "FoodReports",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Foods_FoodCategories_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId",
                principalTable: "FoodCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleFoods_Foods_FoodId",
                table: "LivestockCircleFoods",
                column: "FoodId",
                principalTable: "Foods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleFoods",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleMedicines_LivestockCircles_LivestockCircleId",
                table: "LivestockCircleMedicines",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                table: "LivestockCircleMedicines",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Barns_BarnId",
                table: "LivestockCircles",
                column: "BarnId",
                principalTable: "Barns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Breeds_BreedId",
                table: "LivestockCircles",
                column: "BreedId",
                principalTable: "Breeds",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LivestockCircles_Users_TechicalStaffId",
                table: "LivestockCircles",
                column: "TechicalStaffId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineReports_DailyReports_ReportId",
                table: "MedicineReports",
                column: "ReportId",
                principalTable: "DailyReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineReports_Medicines_MedicineId",
                table: "MedicineReports",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicines_MedicineCategories_MedicineCategoryId",
                table: "Medicines",
                column: "MedicineCategoryId",
                principalTable: "MedicineCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LivestockCircles_LivestockCircleId",
                table: "Orders",
                column: "LivestockCircleId",
                principalTable: "LivestockCircles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Role_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id");
        }
    }
}
