using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseAddImage2 : Migration
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

            migrationBuilder.CreateTable(
                name: "ImageBreeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BreedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageBreeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageBreeds_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ImageDailyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDailyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageDailyReports_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ImageFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageFoods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ImageLivestockCircles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageLivestockCircles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageLivestockCircles_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ImageMedicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsActive", "RoleName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("1aea0d98-dd4c-4b06-9665-2615a3723e18"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6396), true, "Technical Staff", null, null },
                    { new Guid("2be359cd-f34f-4ba6-a1f8-d6cd2ff9f1ee"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6376), true, "Company Admin", null, null },
                    { new Guid("91a56989-b9b1-4c37-b378-18b8903029b0"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6409), true, "Sales Staff", null, null },
                    { new Guid("c25be487-42ea-4858-892f-9b6f80ca623e"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6401), true, "Medicine Room Staff", null, null },
                    { new Guid("c3af7aa2-bda6-4f65-a2d7-10b40cd9b73e"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6399), true, "Feed Room Staff", null, null },
                    { new Guid("db19a21d-3515-421f-ae23-74188fbb4b6b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6406), true, "Worker", null, null },
                    { new Guid("f5634051-8686-4801-a75b-c3fe928ba685"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6411), true, "Customer", null, null },
                    { new Guid("f71d0ef6-3d4a-47ce-ab21-9c894217fb66"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6404), true, "Weighing Room Staff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Email", "IsActive", "Password", "RoleId", "UpdatedBy", "UpdatedDate", "UserName" },
                values: new object[] { new Guid("531ab827-2b3a-4793-a758-ca4927d7c97b"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2025, 6, 14, 14, 57, 26, 670, DateTimeKind.Local).AddTicks(6589), "admin@a", true, "123", new Guid("2be359cd-f34f-4ba6-a1f8-d6cd2ff9f1ee"), null, null, "Company Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_ImageBreeds_BreedId",
                table: "ImageBreeds",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDailyReports_DailyReportId",
                table: "ImageDailyReports",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageFoods_FoodId",
                table: "ImageFoods",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageLivestockCircles_LivestockCircleId",
                table: "ImageLivestockCircles",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageMedicines_MedicineId",
                table: "ImageMedicines",
                column: "MedicineId");

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

            migrationBuilder.DropTable(
                name: "ImageBreeds");

            migrationBuilder.DropTable(
                name: "ImageDailyReports");

            migrationBuilder.DropTable(
                name: "ImageFoods");

            migrationBuilder.DropTable(
                name: "ImageLivestockCircles");

            migrationBuilder.DropTable(
                name: "ImageMedicines");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("1aea0d98-dd4c-4b06-9665-2615a3723e18"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("91a56989-b9b1-4c37-b378-18b8903029b0"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("c25be487-42ea-4858-892f-9b6f80ca623e"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("c3af7aa2-bda6-4f65-a2d7-10b40cd9b73e"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("db19a21d-3515-421f-ae23-74188fbb4b6b"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("f5634051-8686-4801-a75b-c3fe928ba685"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("f71d0ef6-3d4a-47ce-ab21-9c894217fb66"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("531ab827-2b3a-4793-a758-ca4927d7c97b"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("2be359cd-f34f-4ba6-a1f8-d6cd2ff9f1ee"));

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
