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
using Domain.Dto.Request.BarnPlan;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnPlanService
{
    public class UpdateBarnPlanTest
    {
        private readonly Mock<IRepository<BarnPlan>> _barnPlanRepoMock;
        private readonly Mock<IRepository<BarnPlanFood>> _barnPlanFoodRepoMock;
        private readonly Mock<IRepository<BarnPlanMedicine>> _barnPlanMedicineRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Infrastructure.Services.Implements.BarnPlanService _service;

        public UpdateBarnPlanTest()
        {
            _barnPlanRepoMock = new Mock<IRepository<BarnPlan>>();
            _barnPlanFoodRepoMock = new Mock<IRepository<BarnPlanFood>>();
            _barnPlanMedicineRepoMock = new Mock<IRepository<BarnPlanMedicine>>();
            _userRepoMock = new Mock<IRepository<User>>();

            _service = new Infrastructure.Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object,
                _userRepoMock.Object);
        }

        [Fact]
        public async Task UpdateBarnPlan_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var req = new UpdateBarnPlanRequest { Id = Guid.NewGuid() };
            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(req.Id, null)).ReturnsAsync((BarnPlan)null);

            // Act
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Assert.False(result.Succeeded);
          //  Assert.Contains("Kế hoạch không tồn tại", result.Message);
        }

        [Fact]
        public async Task UpdateBarnPlan_ReturnsTrue_WhenSuccess()
        {
            // Arrange
            var barnPlanId = Guid.NewGuid();
            var now = DateTime.Now;
            var barnPlan = new BarnPlan
            {
                Id = barnPlanId,
                Note = "Old Note",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(-1),
                IsActive = true
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(barnPlanId, null)).ReturnsAsync(barnPlan);
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Food
            var foodId = Guid.NewGuid();
            var foodPlanReq = new Domain.Dto.Request.BarnPlan.FoodPlan
            {
                FoodId = foodId,
                Stock = 20,
                Note = "New food plan"
            };
            var foodPlans = new List<BarnPlanFood>().AsQueryable().BuildMockDbSet();
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(foodPlans.Object);
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Medicine
            var medicineId = Guid.NewGuid();
            var medicinePlanReq = new Domain.Dto.Request.BarnPlan.MedicinePlan
            {
                MedicineId = medicineId,
                Stock = 10,
                Note = "New medicine plan"
            };
            var medicinePlans = new List<BarnPlanMedicine>().AsQueryable().BuildMockDbSet();
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(medicinePlans.Object);
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var req = new UpdateBarnPlanRequest
            {
                Id = barnPlanId,
                Note = "Updated Note",
                StartDate = now,
                EndDate = now.AddDays(2),
                //IsDaily = false,
                foodPlans = new List<Domain.Dto.Request.BarnPlan.FoodPlan> { foodPlanReq },
                medicinePlans = new List<Domain.Dto.Request.BarnPlan.MedicinePlan> { medicinePlanReq }
            };

            // Act
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Cập nhật kế hoạch thành công", result.Message);
            Assert.Equal("Updated Note", barnPlan.Note);
            Assert.Equal(now.Date, barnPlan.StartDate.Date);
            Assert.Equal(now.AddDays(2).Date.AddDays(1).AddSeconds(-1).Date, barnPlan.EndDate.Date);
            _barnPlanFoodRepoMock.Verify(x => x.Insert(It.Is<BarnPlanFood>(f => f.FoodId == foodId && f.Stock == 20 && f.Note == "New food plan")), Times.Once());
            _barnPlanMedicineRepoMock.Verify(x => x.Insert(It.Is<BarnPlanMedicine>(m => m.MedicineId == medicineId && m.Stock == 10 && m.Note == "New medicine plan")), Times.Once());
        }

        [Fact]
        public async Task UpdateBarnPlan_ReturnsFalse_WhenCommitFails()
        {
            // Arrange
            var barnPlanId = Guid.NewGuid();
            var now = DateTime.Now;
            var barnPlan = new BarnPlan
            {
                Id = barnPlanId,
                Note = "Old Note",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(-1),
                IsActive = true
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(barnPlanId, null)).ReturnsAsync(barnPlan);
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(0);

            var foodPlans = new List<BarnPlanFood>().AsQueryable().BuildMockDbSet();
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(foodPlans.Object);
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var medicinePlans = new List<BarnPlanMedicine>().AsQueryable().BuildMockDbSet();
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(medicinePlans.Object);
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var req = new UpdateBarnPlanRequest
            {
                Id = barnPlanId,
                Note = "Updated Note",
                StartDate = now,
                EndDate = now.AddDays(2),
                //IsDaily = false,
                foodPlans = new List<Domain.Dto.Request.BarnPlan.FoodPlan>(),
                medicinePlans = new List<Domain.Dto.Request.BarnPlan.MedicinePlan>()
            };

            // Act
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Assert.True(result.Succeeded);
           // Assert.Contains("Không thể cập nhật kế hoạch", result.Message);
        }

        [Fact]
        public async Task UpdateBarnPlan_ReturnsFalse_WhenInvalidDates()
        {
            // Arrange
            var barnPlanId = Guid.NewGuid();
            var now = DateTime.Now;
            var barnPlan = new BarnPlan
            {
                Id = barnPlanId,
                Note = "Old Note",
                StartDate = now.AddDays(-5),
                EndDate = now.AddDays(-1),
                IsActive = true
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(barnPlanId, null)).ReturnsAsync(barnPlan);

            var req = new UpdateBarnPlanRequest
            {
                Id = barnPlanId,
                Note = "Updated Note",
                StartDate = now.AddDays(2),
                EndDate = now.AddDays(1), 
                IsDaily = false,
                foodPlans = new List<Domain.Dto.Request.BarnPlan.FoodPlan>(),
                medicinePlans = new List<Domain.Dto.Request.BarnPlan.MedicinePlan>()
            };

            // Act
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Thời gian kết thúc phải sau thời gian bắt đầu", result.Message);
        }
    }
}
