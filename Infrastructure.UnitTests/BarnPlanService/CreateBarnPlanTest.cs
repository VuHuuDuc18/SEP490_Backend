using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.BarnPlan;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.BarnPlanService
{
    public class CreateBarnPlanTest
    {
        private readonly Mock<IRepository<BarnPlan>> _barnPlanRepoMock;
        private readonly Mock<IRepository<BarnPlanFood>> _barnPlanFoodRepoMock;
        private readonly Mock<IRepository<BarnPlanMedicine>> _barnPlanMedicineRepoMock;
        private readonly Infrastructure.Services.Implements.BarnPlanService _service;

        public CreateBarnPlanTest()
        {
            _barnPlanRepoMock = new Mock<IRepository<BarnPlan>>();
            _barnPlanFoodRepoMock = new Mock<IRepository<BarnPlanFood>>();
            _barnPlanMedicineRepoMock = new Mock<IRepository<BarnPlanMedicine>>();
            _service = new Infrastructure.Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenStartAndEndDateNull_AndNotDaily()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = false,
                StartDate = null,
                EndDate = null
            };
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Phải có dữ liệu ngày", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenStartDateAfterEndDate()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = false,
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(1)
            };
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Thời gian kết thúc phải sau thời gian bắt đầu", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenConflictPlanExists()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = false,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };
            var conflictPlan = new BarnPlan
            {
                Id = Guid.NewGuid(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2),
                IsActive = true
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan> { conflictPlan }.AsQueryable().BuildMock());
            var ex = await Xunit.Assert.ThrowsAsync<ArgumentException>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Đã đặt kế hoạch cho ngày này", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenCommitAsyncFails_OnBarnPlan()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = true,
                Note = "Test"
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan>().AsQueryable().BuildMock());
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(-1);
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Không thể tạo kế hoạch", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenCommitAsyncFails_OnFoodPlan()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = true,
                Note = "Test",
                foodPlans = new List<FoodPlan> { new FoodPlan { FoodId = Guid.NewGuid(), Stock = 1, Note = "F" } }
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan>().AsQueryable().BuildMock());
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanFood>().AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(-1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanMedicine>().AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Không thể tạo kế hoạch thức ăn", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Throws_WhenCommitAsyncFails_OnMedicinePlan()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = true,
                Note = "Test",
                medicinePlans = new List<MedicinePlan> { new MedicinePlan { MedicineId = Guid.NewGuid(), Stock = 1, Note = "M" } }
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan>().AsQueryable().BuildMock());
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanFood>().AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanMedicine>().AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(-1);
            var ex = await Xunit.Assert.ThrowsAsync<Exception>(() => _service.CreateBarnPlan(req));
            Xunit.Assert.Contains("Không thể tạo kế hoạch thuốc", ex.Message);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Success_WithAllData()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = true,
                Note = "Test",
                foodPlans = new List<FoodPlan> { new FoodPlan { FoodId = Guid.NewGuid(), Stock = 1, Note = "F" } },
                medicinePlans = new List<MedicinePlan> { new MedicinePlan { MedicineId = Guid.NewGuid(), Stock = 1, Note = "M" } }
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan>().AsQueryable().BuildMock());
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanFood>().AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanMedicine>().AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.CreateBarnPlan(req);
            Xunit.Assert.True(result.Succeeded);
            _barnPlanRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlan>()), Moq.Times.Once);
            _barnPlanFoodRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlanFood>()), Moq.Times.Once);
            _barnPlanMedicineRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlanMedicine>()), Moq.Times.Once);
        }

        [Xunit.Fact]
        public async Task CreateBarnPlan_Success_WithNullFoodAndMedicinePlans()
        {
            var req = new CreateBarnPlanRequest
            {
                livstockCircleId = Guid.NewGuid(),
                IsDaily = true,
                Note = "Test",
                foodPlans = null,
                medicinePlans = null
            };
            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(new List<BarnPlan>().AsQueryable().BuildMock());
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanFood>().AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable()).Returns(new List<BarnPlanMedicine>().AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.CreateBarnPlan(req);
            Xunit.Assert.True(result.Succeeded);
            _barnPlanRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlan>()), Moq.Times.Once);
            _barnPlanFoodRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlanFood>()), Moq.Times.Never);
            _barnPlanMedicineRepoMock.Verify(x => x.Insert(It.IsAny<BarnPlanMedicine>()), Moq.Times.Never);
        }
    }
}
