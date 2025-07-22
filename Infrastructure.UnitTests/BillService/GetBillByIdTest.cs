using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Response.Bill;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using MockQueryable.Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;
using MockQueryable;

namespace Infrastructure.UnitTests.BillService
{
    public class GetBillByIdTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepoMock;
        private readonly Mock<IRepository<Barn>> _barnRepoMock;
        private readonly Mock<IRepository<Food>> _foodRepoMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepoMock;
        private readonly Mock<IRepository<Breed>> _breedRepoMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepoMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepoMock;
        private readonly Mock<IRepository<ImageBreed>> _breedImageRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public GetBillByIdTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _livestockCircleRepoMock = new Mock<IRepository<LivestockCircle>>();
            _barnRepoMock = new Mock<IRepository<Barn>>();
            _foodRepoMock = new Mock<IRepository<Food>>();
            _medicineRepoMock = new Mock<IRepository<Medicine>>();
            _breedRepoMock = new Mock<IRepository<Breed>>();
            _foodImageRepoMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepoMock = new Mock<IRepository<ImageMedicine>>();
            _breedImageRepoMock = new Mock<IRepository<ImageBreed>>();
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
                _billItemRepoMock.Object,
                _userRepoMock.Object,
                _livestockCircleRepoMock.Object,
                _foodRepoMock.Object,
                _medicineRepoMock.Object,
                _breedRepoMock.Object,
                _barnRepoMock.Object,
                null,
                null,
                _foodImageRepoMock.Object,
                _medicineImageRepoMock.Object,
                _breedImageRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        //[Fact]
        //public async Task GetBillById_ReturnsError_WhenNotLoggedIn()
        //{
        //    // Arrange
        //    var httpContextAccessor = new Mock<IHttpContextAccessor>();
        //    httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        //    var service = new Infrastructure.Services.Implements.BillService(
        //        _billRepoMock.Object,
        //        _billItemRepoMock.Object,
        //        _userRepoMock.Object,
        //        _livestockCircleRepoMock.Object,
        //        _foodRepoMock.Object,
        //        _medicineRepoMock.Object,
        //        _breedRepoMock.Object,
        //        _barnRepoMock.Object,
        //        null,
        //        null,
        //        _foodImageRepoMock.Object,
        //        _medicineImageRepoMock.Object,
        //        _breedImageRepoMock.Object,
        //        httpContextAccessor.Object
        //    );
        //    // Act
        //    var result = await service.GetBillById(Guid.NewGuid());
        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Hãy đăng nhập", result.Message);
        //}

        [Fact]
        public async Task GetBillById_ReturnsError_WhenBillNotFound()
        {
            // Arrange
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Bill)null);
            // Act
            var result = await _service.GetBillById(Guid.NewGuid());
            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Hóa đơn không tồn tại", result.Message);
        }

        [Fact]
        public async Task GetBillById_ReturnsBill_WhenSuccess_FoodType()
        {
            // Arrange
            var billId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var userRequestId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var bill = new Bill { Id = billId, UserRequestId = userRequestId, LivestockCircleId = lscId, TypeBill = "Food", IsActive = true };
            var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 5, IsActive = true } };
            var food = new Food { Id = foodId, FoodName = "Cám", WeighPerUnit = 1, IsActive = true };
            var foodImages = new List<ImageFood> { new ImageFood { Id = Guid.NewGuid(), FoodId = foodId, Thumnail = "true", ImageLink = "img.jpg" } };
            var user = new User { Id = userRequestId, FullName = "User", Email = "user@email.com" };
            var lsc = new LivestockCircle { Id = lscId, BarnId = barnId, LivestockCircleName = "Lứa 1" };
            var barn = new Barn { Id = barnId, BarnName = "Barn1", Address = "Addr", Image = "img", WorkerId = workerId };
            var worker = new User { Id = workerId, FullName = "Worker", Email = "worker@email.com" };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
            _userRepoMock.Setup(x => x.GetByIdAsync(userRequestId, null)).ReturnsAsync(user);
            _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(lsc);
            _barnRepoMock.Setup(x => x.GetByIdAsync(barnId, null)).ReturnsAsync(barn);
            _userRepoMock.Setup(x => x.GetByIdAsync(workerId, null)).ReturnsAsync(worker);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItems.AsQueryable().BuildMock());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, null)).ReturnsAsync(food);
            _foodImageRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
                .Returns(foodImages.AsQueryable().BuildMock());
            // Act
            var result = await _service.GetBillById(billId);
            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Lấy hóa đơn thành công", result.Message);
            Assert.Single(result.Data.BillItem);
            Assert.NotNull(result.Data.BillItem[0].Food);
            Assert.Equal("Cám", result.Data.BillItem[0].Food.FoodName);
            Assert.Equal("img.jpg", result.Data.BillItem[0].Food.Thumbnail);
        }

        [Fact]
        public async Task GetBillById_ReturnsBill_WhenSuccess_MedicineType()
        {
            // Arrange
            var billId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var userRequestId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var bill = new Bill { Id = billId, UserRequestId = userRequestId, LivestockCircleId = lscId, TypeBill = "Medicine", IsActive = true };
            var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, MedicineId = medicineId, Stock = 3, IsActive = true } };
            var medicine = new Medicine { Id = medicineId, MedicineName = "Thuốc A", IsActive = true };
            var medicineImages = new List<ImageMedicine> { new ImageMedicine { Id = Guid.NewGuid(), MedicineId = medicineId, Thumnail = "true", ImageLink = "med.jpg" } };
            var user = new User { Id = userRequestId, FullName = "User", Email = "user@email.com" };
            var lsc = new LivestockCircle { Id = lscId, BarnId = barnId, LivestockCircleName = "Lứa 1" };
            var barn = new Barn { Id = barnId, BarnName = "Barn1", Address = "Addr", Image = "img", WorkerId = workerId };
            var worker = new User { Id = workerId, FullName = "Worker", Email = "worker@email.com" };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
            _userRepoMock.Setup(x => x.GetByIdAsync(userRequestId, null)).ReturnsAsync(user);
            _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(lsc);
            _barnRepoMock.Setup(x => x.GetByIdAsync(barnId, null)).ReturnsAsync(barn);
            _userRepoMock.Setup(x => x.GetByIdAsync(workerId, null)).ReturnsAsync(worker);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItems.AsQueryable().BuildMock());
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, null)).ReturnsAsync(medicine);
            _medicineImageRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns(medicineImages.AsQueryable().BuildMock());
            // Act
            var result = await _service.GetBillById(billId);
            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.BillItem);
            Assert.NotNull(result.Data.BillItem[0].Medicine);
            Assert.Equal("Thuốc A", result.Data.BillItem[0].Medicine.MedicineName);
            Assert.Equal("med.jpg", result.Data.BillItem[0].Medicine.Thumbnail);
        }

        [Fact]
        public async Task GetBillById_ReturnsBill_WhenSuccess_BreedType()
        {
            // Arrange
            var billId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var userRequestId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var workerId = Guid.NewGuid();
            var bill = new Bill { Id = billId, UserRequestId = userRequestId, LivestockCircleId = lscId, TypeBill = "Breed", IsActive = true };
            var billItems = new List<BillItem> { new BillItem { Id = Guid.NewGuid(), BillId = billId, BreedId = breedId, Stock = 2, IsActive = true } };
            var breed = new Breed { Id = breedId, BreedName = "Heo", IsActive = true };
            var breedImages = new List<ImageBreed> { new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId, Thumnail = "true", ImageLink = "heo.jpg" } };
            var user = new User { Id = userRequestId, FullName = "User", Email = "user@email.com" };
            var lsc = new LivestockCircle { Id = lscId, BarnId = barnId, LivestockCircleName = "Lứa 1" };
            var barn = new Barn { Id = barnId, BarnName = "Barn1", Address = "Addr", Image = "img", WorkerId = workerId };
            var worker = new User { Id = workerId, FullName = "Worker", Email = "worker@email.com" };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
            _userRepoMock.Setup(x => x.GetByIdAsync(userRequestId, null)).ReturnsAsync(user);
            _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(lsc);
            _barnRepoMock.Setup(x => x.GetByIdAsync(barnId, null)).ReturnsAsync(barn);
            _userRepoMock.Setup(x => x.GetByIdAsync(workerId, null)).ReturnsAsync(worker);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItems.AsQueryable().BuildMock());
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, null)).ReturnsAsync(breed);
            _breedImageRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImages.AsQueryable().BuildMock());
            // Act
            var result = await _service.GetBillById(billId);
            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.BillItem);
            Assert.NotNull(result.Data.BillItem[0].Breed);
            Assert.Equal("Heo", result.Data.BillItem[0].Breed.BreedName);
            Assert.Equal("heo.jpg", result.Data.BillItem[0].Breed.Thumbnail);
        }

        //[Fact]
        //public async Task GetBillById_ReturnsBill_WithoutInactiveBillItems()
        //{
        //    // Arrange
        //    var billId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var userRequestId = Guid.NewGuid();
        //    var lscId = Guid.NewGuid();
        //    var barnId = Guid.NewGuid();
        //    var workerId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, UserRequestId = userRequestId, LivestockCircleId = lscId, TypeBill = "Food", IsActive = true };
        //    var billItems = new List<BillItem> {
        //        new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 5, IsActive = true },
        //        new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 2, IsActive = false }
        //    };
        //    var food = new Food { Id = foodId, FoodName = "Cám", WeighPerUnit = 1, IsActive = true };
        //    var foodImages = new List<ImageFood> { new ImageFood { Id = Guid.NewGuid(), FoodId = foodId, Thumnail = "true", ImageLink = "img.jpg" } };
        //    var user = new User { Id = userRequestId, FullName = "User", Email = "user@email.com" };
        //    var lsc = new LivestockCircle { Id = lscId, BarnId = barnId, LivestockCircleName = "Lứa 1" };
        //    var barn = new Barn { Id = barnId, BarnName = "Barn1", Address = "Addr", Image = "img", WorkerId = workerId };
        //    var worker = new User { Id = workerId, FullName = "Worker", Email = "worker@email.com" };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);
        //    _userRepoMock.Setup(x => x.GetByIdAsync(userRequestId, null)).ReturnsAsync(user);
        //    _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(lsc);
        //    _barnRepoMock.Setup(x => x.GetByIdAsync(barnId, null)).ReturnsAsync(barn);
        //    _userRepoMock.Setup(x => x.GetByIdAsync(workerId, null)).ReturnsAsync(worker);
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns(billItems.AsQueryable().BuildMock());
        //    _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, null)).ReturnsAsync(food);
        //    _foodImageRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(foodImages.AsQueryable().BuildMock());
        //    // Act
        //    var result = await _service.GetBillById(billId);
        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.NotNull(result.Data);
        //    Assert.Single(result.Data.BillItem); // chỉ lấy item active
        //}
    }
}
