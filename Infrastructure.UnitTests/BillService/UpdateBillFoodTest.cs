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
using MockQueryable;

namespace Infrastructure.UnitTests.BillService
{
    public class UpdateBillFoodTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Food>> _foodRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateBillFoodTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _foodRepoMock = new Mock<IRepository<Food>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("uid", _userId.ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                new Mock<IRepository<LivestockCircle>>().Object,
                _foodRepoMock.Object,
                new Mock<IRepository<Medicine>>().Object,
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
        public async Task UpdateBillFood_ReturnsError_WhenNotLoggedIn()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                new Mock<IRepository<LivestockCircle>>().Object,
                _foodRepoMock.Object,
                new Mock<IRepository<Medicine>>().Object,
                new Mock<IRepository<Breed>>().Object,
                new Mock<IRepository<Barn>>().Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                httpContextAccessor.Object
            );
            var result = await service.UpdateBillFood(new UpdateBillFoodDto());
            Assert.False(result.Succeeded);
            Assert.Contains("Hãy đăng nhập", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenRequestIsNull()
        {
            var result = await _service.UpdateBillFood(null);
            Assert.False(result.Succeeded);
            Assert.Contains("Dữ liệu yêu cầu là bắt buộc", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenNoFoodItems()
        {
            var request = new UpdateBillFoodDto { FoodItems = new List<FoodItemRequest>() };
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Phải cung cấp danh sách mặt hàng thức ăn", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenBillNotFoundOrInactive()
        {
            var request = new UpdateBillFoodDto { BillId = Guid.NewGuid(), DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = Guid.NewGuid(), Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(request.BillId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Bill)null);
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Hóa đơn không tồn tại hoặc không hoạt động", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenBillHasNonFoodItems()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, DeliveryDate = DateTime.Now};
            var billItems = new List<BillItem>
    {
        new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true, FoodId = Guid.NewGuid(), MedicineId = null },
        new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true, FoodId = null, MedicineId = Guid.NewGuid() }
    };
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = Guid.NewGuid(), Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<BillItem, bool>> expr) => billItems.AsQueryable().Where(expr));
            var result = await _service.UpdateBillFood(request);
            if (!result.Succeeded)
                Console.WriteLine("Actual message: " + result.Message);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenValidationFails()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true , DeliveryDate = DateTime.Now };
            var billItems = new List<BillItem>();
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = Guid.NewGuid(), Quantity = 0 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable());
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenValidateItemFails()
        {
            // Giả lập ValidateItem trả về lỗi bằng cách tạo một bill với food không đủ stock
            var billId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true,DeliveryDate = DateTime.Now, };
            var billItems = new List<BillItem>();
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 10 } } };
            var food = new Food { Id = foodId, IsActive = true, Stock = 5 };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenFoodNotFound()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, DeliveryDate = DateTime.Now, IsActive = true };
            var billItems = new List<BillItem>();
            var foodId = Guid.NewGuid();
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 1 } } };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Food?)null);
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_Success()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, DeliveryDate = DateTime.Now, IsActive = true };
            var billItems = new List<BillItem>();
            var foodId = Guid.NewGuid();
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 2 } } };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10, WeighPerUnit = 1.5f };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Food?)food);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
            .Returns(billItems.AsQueryable().BuildMock());
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
            var result = await _service.UpdateBillFood(request);
            Assert.True(result.Succeeded);
            Assert.Contains("thành công", result.Message);
        }

        [Fact]
        public async Task UpdateBillFood_ReturnsError_WhenExceptionThrown()
        {
            var billId = Guid.NewGuid();
            var bill = new Bill { Id = billId, DeliveryDate = DateTime.Now, IsActive = true };
            var billItems = new List<BillItem>();
            var foodId = Guid.NewGuid();
            var request = new UpdateBillFoodDto { BillId = billId, DeliveryDate = DateTime.Now, FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 2 } } };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10, WeighPerUnit = 1.5f };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(billItems.AsQueryable());
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Food?)food);
            _billRepoMock.Setup(x => x.Update(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).Throws(new Exception("DB error"));
            var result = await _service.UpdateBillFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi cập nhật hóa đơn với mặt hàng thức ăn", result.Message);
        }
    }
}
