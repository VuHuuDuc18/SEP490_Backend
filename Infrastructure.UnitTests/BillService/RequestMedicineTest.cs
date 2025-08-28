using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Wrappers;
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
    public class RequestMedicineTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public RequestMedicineTest()
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

        //[Fact]
        //public async Task RequestMedicine_ReturnsError_WhenNotLoggedIn()
        //{
        //    var service = new Infrastructure.Services.Implements.BillService(
        //        _billRepoMock.Object,
        //        _billItemRepoMock.Object,
        //        new Mock<IRepository<User>>().Object,
        //        new Mock<IRepository<LivestockCircle>>().Object,
        //        new Mock<IRepository<Food>>().Object,
        //        _medicineRepoMock.Object,
        //        new Mock<IRepository<Breed>>().Object,
        //        new Mock<IRepository<Barn>>().Object,
        //        new Mock<IRepository<LivestockCircleFood>>().Object,
        //        new Mock<IRepository<LivestockCircleMedicine>>().Object,
        //        new Mock<IRepository<ImageFood>>().Object,
        //        new Mock<IRepository<ImageMedicine>>().Object,
        //        new Mock<IRepository<ImageBreed>>().Object,
        //        new Mock<IHttpContextAccessor>().Object
        //    );
        //    var result = await service.RequestMedicine(new CreateMedicineRequestDto());
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Hãy đăng nhập", result.Message);
        //}

        //[Fact]
        //public async Task RequestMedicine_ReturnsError_WhenRequestIsNull()
        //{
        //    var result = await _service.RequestMedicine(null);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Dữ liệu yêu cầu là bắt buộc", result.Message);
        //}

        [Fact]
        public async Task RequestMedicine_ReturnsError_WhenNoMedicineItems()
        {
            var request = new CreateMedicineRequestDto { MedicineItems = new List<MedicineItemRequest>() };
            var result = await _service.RequestMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Phải cung cấp ít nhất một mặt hàng thuốc", result.Message);
        }

        [Fact]
        public async Task RequestMedicine_ReturnsError_WhenValidationFails()
        {
            var medicineId = Guid.NewGuid();
            var request = new CreateMedicineRequestDto
            {
                MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 0 } }
            };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 10 };
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            var result = await _service.RequestMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("lớn hơn 0", result.Message);
        }

        [Fact]
        public async Task RequestMedicine_ReturnsError_WhenMedicineNotFound()
        {
            var medicineId = Guid.NewGuid();
            var request = new CreateMedicineRequestDto
            {
                MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 2 } }
            };
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Medicine)null);
            var result = await _service.RequestMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message);
        }

        [Fact]
        public async Task RequestMedicine_ReturnsError_WhenMedicineStockNotEnough()
        {
            var medicineId = Guid.NewGuid();
            var request = new CreateMedicineRequestDto
            {
                MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 20 } }
            };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 5 };
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            var result = await _service.RequestMedicine(request);
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại, không hoạt động hoặc không đủ tồn kho", result.Message);
        }

        [Fact]
        public async Task RequestMedicine_Success()
        {
            var medicineId = Guid.NewGuid();
            var request = new CreateMedicineRequestDto
            {
                LivestockCircleId = Guid.NewGuid(),
                Note = "Test",
                DeliveryDate = DateTime.Now,
                MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 2 } }
            };
            var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 10 };
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
            _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
            var result = await _service.RequestMedicine(request);
            Assert.True(result.Succeeded);
            Assert.Contains("thành công", result.Message);
        }

        //[Fact]
        //public async Task RequestMedicine_ReturnsError_WhenExceptionThrown()
        //{
        //    var medicineId = Guid.NewGuid();
        //    var request = new CreateMedicineRequestDto
        //    {
        //        LivestockCircleId = Guid.NewGuid(),
        //        Note = "Test",
        //        DeliveryDate = DateTime.Now,
        //        MedicineItems = new List<MedicineItemRequest> { new MedicineItemRequest { ItemId = medicineId, Quantity = 2 } }
        //    };
        //    var medicine = new Medicine { Id = medicineId, IsActive = true, Stock = 10 };
        //    _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(medicine);
        //    _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>())).Throws(new Exception("DB error"));
        //    var result = await _service.RequestMedicine(request);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi khi tạo yêu cầu thuốc", result.Message);
        //}
    }
}
