using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarnPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Barns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barns_User_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "BreedCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreedCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicineCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BreedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BreedCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breeds_BreedCategories_BreedCategoryId",
                        column: x => x.BreedCategoryId,
                        principalTable: "BreedCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoodCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    WeighPerUnit = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Foods_FoodCategories_FoodCategoryId",
                        column: x => x.FoodCategoryId,
                        principalTable: "FoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicineCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicines_MedicineCategories_MedicineCategoryId",
                        column: x => x.MedicineCategoryId,
                        principalTable: "MedicineCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

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
                name: "LivestockCircles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalUnit = table.Column<int>(type: "int", nullable: false),
                    DeadUnit = table.Column<int>(type: "int", nullable: false),
                    AverageWeight = table.Column<float>(type: "real", nullable: false),
                    GoodUnitNumber = table.Column<int>(type: "int", nullable: false),
                    BadUnitNumber = table.Column<int>(type: "int", nullable: false),
                    BreedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechicalStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestockCircles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestockCircles_Barns_BarnId",
                        column: x => x.BarnId,
                        principalTable: "Barns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LivestockCircles_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LivestockCircles_User_TechicalStaffId",
                        column: x => x.TechicalStaffId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "BarnPlanFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarnPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<float>(type: "real", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnPlanFoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarnPlanFoods_BarnPlans_BarnPlanId",
                        column: x => x.BarnPlanId,
                        principalTable: "BarnPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BarnPlanFoods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
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
                name: "BarnPlanMedicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarnPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stock = table.Column<float>(type: "real", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarnPlanMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarnPlanMedicines_BarnPlans_BarnPlanId",
                        column: x => x.BarnPlanId,
                        principalTable: "BarnPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
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

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bills_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Bills_User_UserRequestId",
                        column: x => x.UserRequestId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeadUnit = table.Column<int>(type: "int", nullable: false),
                    GoodUnit = table.Column<int>(type: "int", nullable: false),
                    BadUnit = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReports_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
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
                name: "LivestockCircleFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remaining = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestockCircleFoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestockCircleFoods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LivestockCircleMedicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remaining = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestockCircleMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestockCircleMedicines_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LivestockCircleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodUnitStock = table.Column<int>(type: "int", nullable: false),
                    BadUnitStock = table.Column<int>(type: "int", nullable: false),
                    TotalBill = table.Column<float>(type: "real", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Orders_User_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "BillItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BreedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillItems_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BillItems_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillItems_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FoodReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodReports_DailyReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FoodReports_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
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
                name: "MedicineReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineReports_DailyReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "DailyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MedicineReports_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarnPlanFoods_BarnPlanId",
                table: "BarnPlanFoods",
                column: "BarnPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnPlanFoods_FoodId",
                table: "BarnPlanFoods",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnPlanMedicines_BarnPlanId",
                table: "BarnPlanMedicines",
                column: "BarnPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_BarnPlanMedicines_MedicineId",
                table: "BarnPlanMedicines",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Barns_WorkerId",
                table: "Barns",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_BillId",
                table: "BillItems",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_BreedId",
                table: "BillItems",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_FoodId",
                table: "BillItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_BillItems_MedicineId",
                table: "BillItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_LivestockCircleId",
                table: "Bills",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_UserRequestId",
                table: "Bills",
                column: "UserRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_BreedCategoryId",
                table: "Breeds",
                column: "BreedCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_LivestockCircleId",
                table: "DailyReports",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodReports_FoodId",
                table: "FoodReports",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodReports_ReportId",
                table: "FoodReports",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Foods_FoodCategoryId",
                table: "Foods",
                column: "FoodCategoryId");

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

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircleFoods_FoodId",
                table: "LivestockCircleFoods",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircleFoods_LivestockCircleId",
                table: "LivestockCircleFoods",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircleMedicines_LivestockCircleId",
                table: "LivestockCircleMedicines",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircleMedicines_MedicineId",
                table: "LivestockCircleMedicines",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircles_BarnId",
                table: "LivestockCircles",
                column: "BarnId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircles_BreedId",
                table: "LivestockCircles",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockCircles_TechicalStaffId",
                table: "LivestockCircles",
                column: "TechicalStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineReports_MedicineId",
                table: "MedicineReports",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineReports_ReportId",
                table: "MedicineReports",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_MedicineCategoryId",
                table: "Medicines",
                column: "MedicineCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LivestockCircleId",
                table: "Orders",
                column: "LivestockCircleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarnPlanFoods");

            migrationBuilder.DropTable(
                name: "BarnPlanMedicines");

            migrationBuilder.DropTable(
                name: "BillItems");

            migrationBuilder.DropTable(
                name: "FoodReports");

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

            migrationBuilder.DropTable(
                name: "LivestockCircleFoods");

            migrationBuilder.DropTable(
                name: "LivestockCircleMedicines");

            migrationBuilder.DropTable(
                name: "MedicineReports");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "BarnPlans");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "DailyReports");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "FoodCategories");

            migrationBuilder.DropTable(
                name: "LivestockCircles");

            migrationBuilder.DropTable(
                name: "MedicineCategories");

            migrationBuilder.DropTable(
                name: "Barns");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "BreedCategories");
        }
    }
}
