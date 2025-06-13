using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
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
                name: "BreedCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "FoodCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "MedicineCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_BarnPlanFoods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_BarnPlanMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id"
                        );
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id"
                        );
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
                        name: "FK_Barns_Users_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Users",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_LivestockCircles_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_Bills_Users_UserRequestId",
                        column: x => x.UserRequestId,
                        principalTable: "Users",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_LivestockCircleFoods_LivestockCircles_LivestockCircleId",
                        column: x => x.LivestockCircleId,
                        principalTable: "LivestockCircles",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_LivestockCircleMedicines_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_Orders_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_FoodReports_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id"
                        );
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
                        principalColumn: "Id"
                        );
                    table.ForeignKey(
                        name: "FK_MedicineReports_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id"
                        );
                });

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
                name: "IX_MedicineReports_MedicineId",
                table: "MedicineReports",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineReports_ReportId",
                table: "MedicineReports",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LivestockCircleId",
                table: "Orders",
                column: "LivestockCircleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
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
                name: "BreedCategories");

            migrationBuilder.DropTable(
                name: "FoodCategories");

            migrationBuilder.DropTable(
                name: "FoodReports");

            migrationBuilder.DropTable(
                name: "LivestockCircleFoods");

            migrationBuilder.DropTable(
                name: "LivestockCircleMedicines");

            migrationBuilder.DropTable(
                name: "MedicineCategories");

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
                name: "LivestockCircles");

            migrationBuilder.DropTable(
                name: "Barns");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
