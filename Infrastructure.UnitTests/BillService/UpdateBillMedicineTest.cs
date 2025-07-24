using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Bill;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Assert = Xunit.Assert;
using MockQueryable.Moq;

namespace Infrastructure.UnitTests.BillService
{
    public class UpdateBillMedicineTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateBillMedicineTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _medicineRepoMock = new Mock<IRepository<Medicine>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("uid", _userId.ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                new Mock<IRepository<LivestockCircle>>().Object,
                new Mock<IRepository<Food>>().Object,
                _medicineRepoMock.Object,
                new Mock<IRepository<Breed>>().Object,
                new Mock<IRepository<Barn>>().Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenNotLoggedIn()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                new Mock<IRepository<LivestockCircle>>().Object,
                new Mock<IRepository<Food>>().Object,
                _medicineRepoMock.Object,
                new Mock<IRepository<Breed>>().Object,
                new Mock<IRepository<Barn>>().Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                httpContextAccessor.Object
            );
            var result = await service.UpdateBillMedicine(new UpdateBillMedicineDto());
            Assert.False(result.Succeeded);
            Assert.Contains("Hãy đăng nhập", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenRequestIsNull()
        {
            var result = await _service.UpdateBillMedicine(null);
            Assert.False(result.Succeeded);
            Assert.Contains("Dữ liệu yêu cầu là bắt buộc", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenNoMedicineItems()
        {
            var request = new UpdateBillMedicineDto { MedicineItems = new List<MedicineItemRequest>() };
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Phải cung cấp danh sách mặt hàng thuốc", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenBillNotFoundOrInactive()
        {
            var request = new UpdateBillMedicineDto { BillId = Guid.NewGuid(), MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = Guid.NewGuid(), Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(request.BillId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Bill)null);
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Hóa đơn không tồn tại hoặc không hoạt động", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenBillHasNonMedicineItems()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            var billItems = new List<BillItem>
            {
                new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true, MedicineId = Guid.NewGuid(), FoodId = null },
                new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true, MedicineId = null, FoodId = Guid.NewGuid() }
            };
            var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = Guid.NewGuid(), Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItems.AsQueryable().BuildMockDbSet().Object);
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Hóa đơn chứa các loại mặt hàng không phải thuốc", result.Message);
        }

        //[Fact]
        //public async Task UpdateBillMedicine_ReturnsError_WhenValidationFails()
        //{
        //    var billId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true };
        //    var billItems = new List<BillItem>();
        //    var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = Guid.NewGuid(), Quantity = 0 } } };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable().BuildMockDbSet().Object);
        //    var result = await _service.UpdateBillMedicine(request);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi khi cập nhật hóa đơn", result.Message);
        //}

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenValidateItemFails()
        {
            var billId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            var billItems = new List<BillItem>();
            var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 10 } } };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 5 };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable().BuildMockDbSet().Object);
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại, không hoạt động hoặc không đủ tồn kho.", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenMedicineNotFound()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            var billItems = new List<BillItem>();
            var medicineId = Guid.NewGuid();
            var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable().BuildMockDbSet().Object);
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Medicine)null);
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại, không hoạt động hoặc không đủ tồn kho.", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_Success()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            var billItems = new List<BillItem>();
            var medicineId = Guid.NewGuid();
            var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 2 } } };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 10 };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable().BuildMockDbSet().Object);
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
            var result = await _service.UpdateBillMedicine(request);
            Assert.True(result.Succeeded);
            Assert.Contains("thành công", result.Message);
        }

        [Fact]
        public async Task UpdateBillMedicine_ReturnsError_WhenExceptionThrown()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true };
            var billItems = new List<BillItem>();
            var medicineId = Guid.NewGuid();
            var request = new UpdateBillMedicineDto { BillId = billId, MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 2 } } };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 10 };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable().BuildMockDbSet().Object);
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).Throws(new Exception("DB error"));
            var result = await _service.UpdateBillMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn với mặt hàng thuốc", result.Message);
        }
    }
}
