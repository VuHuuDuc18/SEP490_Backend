using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using MockQueryable.Moq;
using Xunit;
using MockQueryable;
using Assert = Xunit.Assert;
using Org.BouncyCastle.Ocsp;

namespace Infrastructure.UnitTests.BarnPlanService
{
    public class DisableBarnPlanTest
    {
        private readonly Mock<IRepository<BarnPlan>> _barnPlanRepoMock;
        private readonly Mock<IRepository<BarnPlanFood>> _barnPlanFoodRepoMock;
        private readonly Mock<IRepository<BarnPlanMedicine>> _barnPlanMedicineRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Infrastructure.Services.Implements.BarnPlanService _service;

        public DisableBarnPlanTest()
        {
            _barnPlanRepoMock = new Mock<IRepository<BarnPlan>>();
            _barnPlanFoodRepoMock = new Mock<IRepository<BarnPlanFood>>();
            _barnPlanMedicineRepoMock = new Mock<IRepository<BarnPlanMedicine>>();
            _userRepoMock = new Mock<IRepository<User>>();

            _service = new Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object,
                _userRepoMock.Object);
        }

        [Fact]
        public async Task DisableBarnPlan_Throws_WhenNotFound()
        {
            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(() => null);

            var result = await _service.DisableBarnPlan(Guid.NewGuid());
            Xunit.Assert.False(result.Succeeded); 
            Assert.Contains("Kế khoạch không tồn tại", result.Message);
        }

        [Fact]
        public async Task DisableBarnPlan_Success_EndDateInFuture()
        {
            var id = Guid.NewGuid();
            var now = DateTime.Now;
            var barnPlan = new BarnPlan
            {
                Id = id,
                EndDate = now.AddDays(1),
                IsActive = true
            };
            var foods = new List<BarnPlanFood>
            {
                new BarnPlanFood { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };
            var medicines = new List<BarnPlanMedicine>
            {
                new BarnPlanMedicine { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(barnPlan);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanFood, bool>>>()))
                .Returns(foods.AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanMedicine, bool>>>()))
                .Returns(medicines.AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var result = await _service.DisableBarnPlan(id);

            Assert.True(result.Succeeded);
            Assert.False(barnPlan.IsActive);
            Assert.True((now - barnPlan.EndDate).TotalSeconds < 5); // EndDate được set về gần DateTime.Now
            Assert.All(foods, f => Assert.False(f.IsActive));
            Assert.All(medicines, m => Assert.False(m.IsActive));
        }

        [Fact]
        public async Task DisableBarnPlan_Success_EndDateInPast()
        {
            var id = Guid.NewGuid();
            var barnPlan = new BarnPlan
            {
                Id = id,
                EndDate = DateTime.Now.AddDays(-1),
                IsActive = true
            };
            var foods = new List<BarnPlanFood>
            {
                new BarnPlanFood { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };
            var medicines = new List<BarnPlanMedicine>
            {
                new BarnPlanMedicine { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(barnPlan);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanFood, bool>>>()))
                .Returns(foods.AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanMedicine, bool>>>()))
                .Returns(medicines.AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var result = await _service.DisableBarnPlan(id);

            Assert.True(result.Succeeded);
            Assert.False(barnPlan.IsActive);
            Assert.All(foods, f => Assert.False(f.IsActive));
            Assert.All(medicines, m => Assert.False(m.IsActive));
        }

        [Fact]
        public async Task DisableBarnPlan_Fail_CommitReturnsFalse()
        {
            var id = Guid.NewGuid();
            var barnPlan = new BarnPlan
            {
                Id = id,
                EndDate = DateTime.Now.AddDays(-1),
                IsActive = true
            };
            var foods = new List<BarnPlanFood>
            {
                new BarnPlanFood { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };
            var medicines = new List<BarnPlanMedicine>
            {
                new BarnPlanMedicine { Id = Guid.NewGuid(), BarnPlanId = id, IsActive = true }
            };

            _barnPlanRepoMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(barnPlan);
            _barnPlanFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanFood, bool>>>()))
                .Returns(foods.AsQueryable().BuildMock());
            _barnPlanFoodRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlanMedicine, bool>>>()))
                .Returns(medicines.AsQueryable().BuildMock());
            _barnPlanMedicineRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _barnPlanRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(0);

            var result = await _service.DisableBarnPlan(id);

            Assert.True(result.Succeeded);
            Assert.False(barnPlan.IsActive);
        }
    }
}