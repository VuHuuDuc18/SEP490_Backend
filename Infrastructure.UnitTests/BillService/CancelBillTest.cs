using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Wrappers;
using Entities.EntityModel;
using Infrastructure.Repository;
// Không using BillService ở đây để tránh shadow class
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BillService
{
    public class CancelBillTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public CancelBillTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            // Mock user context
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("uid", _userId.ToString())
            }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                null,
                null,
                null,
                null,
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

        //[Fact]
        //public async Task CancelBill_ReturnsError_WhenNotLoggedIn()
        //{
        //    var httpContextAccessor = new Mock<IHttpContextAccessor>();
        //    httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        //    var service = new Infrastructure.Services.Implements.BillService(
        //        _billRepoMock.Object,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        null,
        //        httpContextAccessor.Object
        //    );
        //    var result = await service.CancelBill(Guid.NewGuid());
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        [Fact]
        public async Task CancelBill_ReturnsError_WhenBillNotFoundOrInactive()
        {
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Bill)null);
            var result = await _service.CancelBill(Guid.NewGuid());
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CancelBill_ReturnsError_WhenBillNotRequested()
        {
            var bill = new Bill { Id = Guid.NewGuid(), IsActive = true, Status = "APPROVED", Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            var result = await _service.CancelBill(bill.Id);
            Assert.False(result.Succeeded);
            Assert.Contains("trạng thái", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CancelBill_Success_WhenRequested()
        {
            var bill = new Bill { Id = Guid.NewGuid(), IsActive = true, Status = Domain.Helper.Constants.StatusConstant.REQUESTED, Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.CancelBill(bill.Id);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Hủy hóa đơn thành công", result.Message);
        }

        //[Fact]
        //public async Task CancelBill_ReturnsError_WhenExceptionThrown()
        //{
        //    var bill = new Bill { Id = Guid.NewGuid(), IsActive = true, Status = Domain.Helper.Constants.StatusConstant.REQUESTED, Note = "n", Name = "n", TypeBill = "Food", Total = 1, Weight = 1 };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync(bill);
        //    _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
        //    _billRepoMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("db error"));
        //    var result = await _service.CancelBill(bill.Id);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi", result.Message);
        //}
    }
}
