using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using MockQueryable.Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Response.BarnPlan;

namespace Infrastructure.UnitTests.BarnPlanService
{
    public class GetByIdTest
    {
        private readonly Mock<IRepository<BarnPlan>> _barnPlanRepoMock;
        private readonly Mock<IRepository<BarnPlanFood>> _barnPlanFoodRepoMock;
        private readonly Mock<IRepository<BarnPlanMedicine>> _barnPlanMedicineRepoMock;
        private readonly Infrastructure.Services.Implements.BarnPlanService _service;

        public GetByIdTest()
        {
            _barnPlanRepoMock = new Mock<IRepository<BarnPlan>>();
            _barnPlanFoodRepoMock = new Mock<IRepository<BarnPlanFood>>();
            _barnPlanMedicineRepoMock = new Mock<IRepository<BarnPlanMedicine>>();
            _service = new Infrastructure.Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object);
        }

        [Fact]
        public async Task GetById_Throws_WhenNotFound()
        {
            // Arrange
            var barnPlanId = Guid.NewGuid();
            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(barnPlanId, null)).ReturnsAsync((BarnPlan)null);

            // Act & Assert
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.GetById(barnPlanId));
            Xunit.Assert.Contains("Không tìm thấy kế hoạch cho chuồng", ex.Message);
        }

        [Fact]
        public async Task GetById_Returns_CorrectData()
        {
            // Arrange
            var barnPlanId = Guid.NewGuid();
            var now = DateTime.Now;
            var barnPlan = new BarnPlan
            {
                Id = barnPlanId,
                Note = "Test Note",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(1),
                IsActive = true
            };
            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(barnPlanId, null)).ReturnsAsync(barnPlan);

            // Food
            var foodId = Guid.NewGuid();
            var food = new Food { Id = foodId, FoodName = "Corn" };
            var foodPlan = new BarnPlanFood
            {
                Id = Guid.NewGuid(),
                BarnPlanId = barnPlanId,
                FoodId = foodId,
                Food = food,
                Stock = 10,
                Note = "Feed in morning",
                IsActive = true
            };
            var foodPlans = new List<BarnPlanFood> { foodPlan }.AsQueryable().BuildMockDbSet();
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanFood, bool>>>()))
                .Returns(foodPlans.Object);

            // Medicine
            var medicineId = Guid.NewGuid();
            var medicine = new Medicine { Id = medicineId, MedicineName = "Antibiotic" };
            var medicinePlan = new BarnPlanMedicine
            {
                Id = Guid.NewGuid(),
                BarnPlanId = barnPlanId,
                MedicineId = medicineId,
                Medicine = medicine,
                Stock = 5,
                Note = "After meal",
                IsActive = true
            };
            var medicinePlans = new List<BarnPlanMedicine> { medicinePlan }.AsQueryable().BuildMockDbSet();
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanMedicine, bool>>>()))
                .Returns(medicinePlans.Object);

            // Act
            var result = await _service.GetById(barnPlanId);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(barnPlanId, result.Data.Id);
            Xunit.Assert.Equal("Test Note", result.Data.Note);
            Xunit.Assert.Equal(barnPlan.StartDate, result.Data.StartDate);
            Xunit.Assert.Equal(barnPlan.EndDate, result.Data.EndDate);
            Xunit.Assert.NotNull(result.Data.foodPlans);
            Xunit.Assert.Single(result.Data.foodPlans);
            Xunit.Assert.Equal(foodId, result.Data.foodPlans[0].FoodId);
            Xunit.Assert.Equal("Corn", result.Data.foodPlans[0].FoodName);
            Xunit.Assert.Equal(10, result.Data.foodPlans[0].Stock);
            Xunit.Assert.Equal("Feed in morning", result.Data.foodPlans[0].Note);
            Xunit.Assert.NotNull(result.Data.medicinePlans);
            Xunit.Assert.Single(result.Data.medicinePlans);
            Xunit.Assert.Equal(medicineId, result.Data.medicinePlans[0].MedicineId);
            Xunit.Assert.Equal("Antibiotic", result.Data.medicinePlans[0].MedicineName);
            Xunit.Assert.Equal(5, result.Data.medicinePlans[0].Stock);
            Xunit.Assert.Equal("After meal", result.Data.medicinePlans[0].Note);
        }
    }
}
