using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConstraintCategory : Migration
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
                name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                table: "DailyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_DailyReports_ReportId",
                table: "FoodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FoodReports_Foods_FoodId",
                table: "FoodReports");

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
                name: "FK_MedicineReports_DailyReports_ReportId",
                table: "MedicineReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineReports_Medicines_MedicineId",
                table: "MedicineReports");

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
                keyValue: new Guid("2f538bb0-4e20-4167-acb9-26401704fd15"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3a25dacf-ad6f-4d90-84c8-bbea0b7d4c70"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("605b5716-7e31-4ae1-8452-81d37afef64a"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("9744870e-9ead-4de7-ba84-93d24cc94761"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("b11c88da-e04e-411b-82aa-4a8513e8a1b3"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("edcf2512-4566-4756-a81c-e242db232bb8"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("fc090a30-2bd2-45dc-9b6c-82afe01e85c8"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8bfc68ff-e787-4db0-abea-d8ae79f8cd8a"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("22b1c643-9ea0-4f99-8d70-9a64710c2323"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("3fc6c5e2-2c5b-4ad5-8315-49cf460e285b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1229), true, "Sales Staff", null, null },
                    { new Guid("638012d8-276f-4b15-9732-dcb752f88ae0"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1223), true, "Weighing Room Staff", null, null },
                    { new Guid("9310321f-9552-4b67-80fe-422bd381aa16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1255), true, "Customer", null, null },
                    { new Guid("b526c879-c306-44a3-8ae8-e53524b1dc9c"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1220), true, "Medicine Room Staff", null, null },
                    { new Guid("cdd291e1-8cc4-4fe4-ae17-544c75a4b8f1"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1226), true, "Worker", null, null },
                    { new Guid("e06dfb58-93a3-4cbd-8f67-3be1fd503600"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1218), true, "Feed Room Staff", null, null },
                    { new Guid("f97fcdce-9310-4ddd-8353-7db431bbb303"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(709), true, "Company Admin", null, null },
                    { new Guid("fe5a1a71-30ba-461c-a32f-30d6980da2c1"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1215), true, "Technical Staff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("0b766c58-b121-48c1-9456-350af3ee2bab"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 25, 53, 473, DateTimeKind.Local).AddTicks(1450), "admin@a", true, "123", new Guid("f97fcdce-9310-4ddd-8353-7db431bbb303"), null, null, "Company Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_MedicineCategoryId",
                table: "Medicines",
                column: "MedicineCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_BreedCategoryId",
                table: "Breeds",
                column: "BreedCategoryId");

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

            migrationBuilder.DropIndex(
                name: "IX_Medicines_MedicineCategoryId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods");

            migrationBuilder.DropIndex(
                name: "IX_Breeds_BreedCategoryId",
                table: "Breeds");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3fc6c5e2-2c5b-4ad5-8315-49cf460e285b"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("638012d8-276f-4b15-9732-dcb752f88ae0"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("9310321f-9552-4b67-80fe-422bd381aa16"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("b526c879-c306-44a3-8ae8-e53524b1dc9c"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("cdd291e1-8cc4-4fe4-ae17-544c75a4b8f1"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("e06dfb58-93a3-4cbd-8f67-3be1fd503600"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("fe5a1a71-30ba-461c-a32f-30d6980da2c1"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0b766c58-b121-48c1-9456-350af3ee2bab"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("f97fcdce-9310-4ddd-8353-7db431bbb303"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("22b1c643-9ea0-4f99-8d70-9a64710c2323"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7228), true, "Company Admin", null, null },
                    { new Guid("2f538bb0-4e20-4167-acb9-26401704fd15"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7253), true, "Worker", null, null },
                    { new Guid("3a25dacf-ad6f-4d90-84c8-bbea0b7d4c70"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7248), true, "Feed Room Staff", null, null },
                    { new Guid("605b5716-7e31-4ae1-8452-81d37afef64a"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7251), true, "Weighing Room Staff", null, null },
                    { new Guid("9744870e-9ead-4de7-ba84-93d24cc94761"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7245), true, "Technical Staff", null, null },
                    { new Guid("b11c88da-e04e-411b-82aa-4a8513e8a1b3"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7250), true, "Medicine Room Staff", null, null },
                    { new Guid("edcf2512-4566-4756-a81c-e242db232bb8"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7269), true, "Customer", null, null },
                    { new Guid("fc090a30-2bd2-45dc-9b6c-82afe01e85c8"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7255), true, "Sales Staff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("8bfc68ff-e787-4db0-abea-d8ae79f8cd8a"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 13, 17, 8, 6, 954, DateTimeKind.Local).AddTicks(7570), "admin@a", true, "123", new Guid("22b1c643-9ea0-4f99-8d70-9a64710c2323"), null, null, "Company Admin" });

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
