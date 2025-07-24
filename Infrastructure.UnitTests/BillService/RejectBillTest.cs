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
    public class RejectBillTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Food>> _foodRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public RejectBillTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _foodRepoMock = new Mock<IRepository<Food>>();
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
                null,
                _foodRepoMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task RejectBill_ReturnsError_WhenNotLoggedIn()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                null,
                null,
                _foodRepoMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                httpContextAccessor.Object
            );
            var result = await service.RejectBill(Guid.NewGuid());
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RejectBill_ReturnsError_WhenBillNotFoundOrInactive()
        {
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Bill)null);
            var result = await _service.RejectBill(Guid.NewGuid());
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RejectBill_ReturnsError_WhenBillStatusNotRequestedOrApproved()
        {
            var bill = new Bill { Id = Guid.NewGuid(), IsActive = true, Status = "CONFIRMED", Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            var result = await _service.RejectBill(bill.Id);
            Assert.False(result.Succeeded);
            Assert.Contains("trạng thái", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RejectBill_Success_WhenStatusIsRequested()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.REQUESTED, Note = "n", Name = "n", TypeBill = "Food", Total = 5, Weight = 1 };
            var billItems = new List<BillItem>();
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.RejectBill(billId);
            if (!result.Succeeded)
            {
                var errorMsg = $"Message: {result.Message}\nErrors: {(result.Errors != null ? string.Join(", ", result.Errors) : "null")}";
                throw new Xunit.Sdk.XunitException(errorMsg);
            }
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Từ chối hóa đơn thành công", result.Message);
        }

        [Fact]
        public async Task RejectBill_Success_WhenStatusIsApproved()
        {
            var billId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.APPROVED, Note = "n", Name = "n", TypeBill = "Food", Total = 5, Weight = 1 };
            var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 2, IsActive = true } };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr).BuildMock());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            _foodRepoMock.Setup(x => x.Update(It.IsAny<Food>()));
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.RejectBill(billId);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Từ chối hóa đơn thành công", result.Message);
        }

        [Fact]
        public async Task RejectBill_ReturnsError_WhenExceptionThrown()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, Status = Domain.Helper.Constants.StatusConstant.REQUESTED, Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("db error"));
            var result = await _service.RejectBill(billId);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi", result.Message);
        }
    }
}
