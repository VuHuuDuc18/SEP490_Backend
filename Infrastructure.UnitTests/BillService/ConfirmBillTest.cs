using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Wrappers;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.BillService
{
    public class ConfirmBillTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepoMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public ConfirmBillTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _livestockCircleFoodRepoMock = new Mock<IRepository<LivestockCircleFood>>();
            _livestockCircleMedicineRepoMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _livestockCircleRepoMock = new Mock<IRepository<LivestockCircle>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("uid", _userId.ToString())
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                null,
                _livestockCircleRepoMock.Object,
                null,
                null,
                null,
                null,
                _livestockCircleFoodRepoMock.Object,
                _livestockCircleMedicineRepoMock.Object,
                null,
                null,
                null,
                _httpContextAccessorMock.Object
            );
        }

        //[Fact]
        //public async Task ConfirmBill_ReturnsError_WhenNotLoggedIn()
        //{
        //    var httpContextAccessor = new Mock<IHttpContextAccessor>();
        //    httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        //    var service = new Infrastructure.Services.Implements.BillService(
        //        _billRepoMock.Object,
        //        _billItemRepoMock.Object,
        //        null,
        //        _livestockCircleRepoMock.Object,
        //        null,
        //        null,
        //        null,
        //        null,
        //        _livestockCircleFoodRepoMock.Object,
        //        _livestockCircleMedicineRepoMock.Object,
        //        null,
        //        null,
        //        null,
        //        httpContextAccessor.Object
        //    );
        //    var result = await service.ConfirmBill(Guid.NewGuid());
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        [Fact]
        public async Task ConfirmBill_ReturnsError_WhenBillNotFoundOrInactive()
        {
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Bill)null);
            var result = await _service.ConfirmBill(Guid.NewGuid());
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ConfirmBill_ReturnsError_WhenBillNotApproved()
        {
            var bill = new Bill { Id = Guid.NewGuid(), IsActive = true, Status = "REQUESTED", Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            var result = await _service.ConfirmBill(bill.Id);
            Assert.False(result.Succeeded);
            Assert.Contains("trạng thái", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ConfirmBill_Success_WhenApproved_FoodItems()
        {
            var billId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.APPROVED, Note = "n", Name = "n", TypeBill = Domain.Helper.Constants.TypeBill.FOOD, Total = 5, Weight = 1, LivestockCircleId = lscId };
            var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 2, IsActive = true } };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
            _livestockCircleFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns(Enumerable.Empty<LivestockCircleFood>().AsQueryable().BuildMock());
            _livestockCircleFoodRepoMock.Setup(x => x.Insert(It.IsAny<LivestockCircleFood>()));
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.ConfirmBill(billId);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Xác nhận hóa đơn thành công", result.Message);
        }

        //[Fact]
        //public async Task ConfirmBill_Success_WhenApproved_MedicineItems()
        //{
        //    var billId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var lscId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.APPROVED, Note = "n", Name = "n", TypeBill = Domain.Helper.Constants.TypeBill.MEDICINE, Total = 5, Weight = 1, LivestockCircleId = lscId };
        //    var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, MedicineId = medicineId, Stock = 2, IsActive = true } };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
        //    _livestockCircleMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircleMedicine, bool>>>()))
        //        .Returns(Enumerable.Empty<LivestockCircleMedicine>().AsQueryable().BuildMock());
        //    _livestockCircleMedicineRepoMock.Setup(x => x.Insert(It.IsAny<LivestockCircleMedicine>()));
        //    _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
        //    _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
        //    var result = await _service.ConfirmBill(billId);
        //    Assert.True(result.Succeeded);
        //    Assert.True(result.Data);
        //    Assert.Equal("Xác nhận hóa đơn thành công", result.Message);
        //}

        //[Fact]
        //public async Task ConfirmBill_Success_WhenApproved_BreedItems()
        //{
        //    var billId = Guid.NewGuid();
        //    var breedId = Guid.NewGuid();
        //    var lscId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.APPROVED, Note = "n", Name = "n", TypeBill = Domain.Helper.Constants.TypeBill.BREED, Total = 5, Weight = 1, LivestockCircleId = lscId };
        //    var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, BreedId = breedId, Stock = 2, IsActive = true } };
        //    var lsc = new LivestockCircle { Id = lscId, Status = Domain.Helper.Constants.StatusConstant.GROWINGSTAT, TotalUnit = 0, GoodUnitNumber = 0, DeadUnit = 0, AverageWeight = 0 };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
        //    _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(lsc);
        //    _livestockCircleRepoMock.Setup(x => x.Update(It.IsAny<LivestockCircle>()));
        //    _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
        //    _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
        //    var result = await _service.ConfirmBill(billId);
        //    Assert.True(result.Succeeded);
        //    Assert.True(result.Data);
        //    Assert.Equal("Xác nhận hóa đơn thành công", result.Message);
        //}

        //[Fact]
        //public async Task ConfirmBill_ReturnsError_WhenExceptionThrown()
        //{
        //    var billId = Guid.NewGuid();
        //    var lscId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.APPROVED, Note = "n", Name = "n", TypeBill = Domain.Helper.Constants.TypeBill.FOOD, Total = 1, Weight = 1, LivestockCircleId = lscId };
        //    var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = Guid.NewGuid(), Stock = 2, IsActive = true } };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
        //    _livestockCircleFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Returns(Enumerable.Empty<LivestockCircleFood>().AsQueryable().BuildMock());
        //    _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
        //    _billRepoMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("db error"));
        //    var result = await _service.ConfirmBill(billId);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi", result.Message);
        //}
    }
}
