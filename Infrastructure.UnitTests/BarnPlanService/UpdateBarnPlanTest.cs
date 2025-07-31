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
            _service = new Infrastructure.Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object, _userRepoMock.Object);
        }

        [Fact]
        public async Task UpdateBarnPlan_Throws_WhenNotFound()
        {
            // Arrange
            var req = new UpdateBarnPlanRequest { Id = Guid.NewGuid() };
            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(req.Id, null)).ReturnsAsync((BarnPlan)null);

            // Act & Assert
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.UpdateBarnPlan(req));
            Xunit.Assert.Contains("Kế hoạch không tồn tại", ex.Message);
        }

        [Fact]
        public async Task UpdateBarnPlan_Returns_True_WhenSuccess()
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

            // Act
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
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Xunit.Assert.True(result.Succeeded);
            Xunit.Assert.Equal("Updated Note", barnPlan.Note);
            Xunit.Assert.Equal(now.Date, barnPlan.StartDate.Date);
            Xunit.Assert.Equal(now.AddDays(2).Date.AddDays(1).AddSeconds(-1).Date, barnPlan.EndDate.Date);
        }

        [Fact]
        public async Task UpdateBarnPlan_Returns_False_WhenCommitFail()
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

            // Food
            var foodPlans = new List<BarnPlanFood>().AsQueryable().BuildMockDbSet();
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(foodPlans.Object);
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Medicine
            var medicinePlans = new List<BarnPlanMedicine>().AsQueryable().BuildMockDbSet();
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(medicinePlans.Object);
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Act
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
            var result = await _service.UpdateBarnPlan(req);

            // Assert
            Xunit.Assert.False(result.Succeeded);
        }
    }
}
