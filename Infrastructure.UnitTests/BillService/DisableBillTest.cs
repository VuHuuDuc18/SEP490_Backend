using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Wrappers;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BillService
{
    public class DisableBillTest
    {
        private readonly Mock<IRepository<Entities.EntityModel.Bill>> _billRepoMock;
        private readonly Infrastructure.Services.Implements.BillService _service;

        public DisableBillTest()
        {
            _billRepoMock = new Mock<IRepository<Entities.EntityModel.Bill>>();
            // Các repo khác không cần thiết cho DisableBill
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                Mock.Of<IRepository<BillItem>>(),
                Mock.Of<IRepository<User>>(),
                Mock.Of<IRepository<LivestockCircle>>(),
                Mock.Of<IRepository<Food>>(),
                Mock.Of<IRepository<Medicine>>(),
                Mock.Of<IRepository<Breed>>(),
                Mock.Of<IRepository<Barn>>(),
                Mock.Of<IRepository<LivestockCircleFood>>(),
                Mock.Of<IRepository<LivestockCircleMedicine>>(),
                Mock.Of<IRepository<ImageFood>>(),
                Mock.Of<IRepository<ImageMedicine>>(),
                Mock.Of<IRepository<ImageBreed>>(),
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>()
            );
        }

        [Fact]
        public async Task DisableBill_ReturnsError_WhenBillNotFoundOrInactive()
        {
            // Arrange
            var billId = Guid.NewGuid();
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync((Bill)null);

            // Act
            var result = await _service.DisableBill(billId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message);
        }

        [Fact]
        public async Task DisableBill_ReturnsTrue_WhenSuccess()
        {
            // Arrange
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.DisableBill(billId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Vô hiệu hóa hóa đơn thành công", result.Message);
            Assert.False(bill.IsActive);
            _billRepoMock.Verify(x => x.Update(bill), Times.Once);
            _billRepoMock.Verify(x => x.CommitAsync(default), Times.Once);
        }

        //[Fact]
        //public async Task DisableBill_ReturnsFalse_WhenExceptionThrown()
        //{
        //    // Arrange
        //    var billId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
        //    _billRepoMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("DB error"));

        //    // Act
        //    var result = await _service.DisableBill(billId);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi khi vô hiệu hóa hóa đơn", result.Message);
        //    Assert.NotNull(result.Errors);
        //}
    }
}
