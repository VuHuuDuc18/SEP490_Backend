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
    public class RequestFoodTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Food>> _foodRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public RequestFoodTest()
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
        public async Task RequestFood_ReturnsError_WhenNotLoggedIn()
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
            var result = await service.RequestFood(new CreateFoodRequestDto());
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenRequestIsNull()
        {
            var result = await _service.RequestFood(null);
            Assert.False(result.Succeeded);
            Assert.Contains("bắt buộc", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenNoFoodItems()
        {
            var request = new CreateFoodRequestDto { FoodItems = new List<FoodItemRequest>() };
            var result = await _service.RequestFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Phải cung cấp ít nhất một mặt hàng thức ăn", result.Message);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenValidationFails()
        {
            var foodId = Guid.NewGuid();
            var request = new CreateFoodRequestDto
            {
                FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 0 } }
            };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10 };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            var result = await _service.RequestFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("lớn hơn 0", result.Message);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenFoodNotFound()
        {
            var foodId = Guid.NewGuid();
            var request = new CreateFoodRequestDto
            {
                FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 1 } }
            };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Food)null);
            var result = await _service.RequestFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains($"ID {foodId}", result.Message);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenFoodStockNotEnough()
        {
            var foodId = Guid.NewGuid();
            var request = new CreateFoodRequestDto
            {
                FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 10 } }
            };
            var food = new Food { Id = foodId, IsActive = true, Stock = 5 };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            var result = await _service.RequestFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("không đủ tồn kho", result.Message);
        }

        [Fact]
        public async Task RequestFood_Success()
        {
            var foodId = Guid.NewGuid();
            var request = new CreateFoodRequestDto
            {
                LivestockCircleId = Guid.NewGuid(),
                Note = "Test",
                DeliveryDate = DateTime.Now,
                FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 2 } }
            };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10, WeighPerUnit = 1.5f };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _service.RequestFood(request);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Tạo yêu cầu thức ăn thành công", result.Message);
        }

        [Fact]
        public async Task RequestFood_ReturnsError_WhenExceptionThrown()
        {
            var foodId = Guid.NewGuid();
            var request = new CreateFoodRequestDto
            {
                LivestockCircleId = Guid.NewGuid(),
                Note = "Test",
                DeliveryDate = DateTime.Now,
                FoodItems = new List<FoodItemRequest> { new FoodItemRequest { ItemId = foodId, Quantity = 2 } }
            };
            var food = new Food { Id = foodId, IsActive = true, Stock = 10, WeighPerUnit = 1.5f };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(food);
            _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("db error"));
            var result = await _service.RequestFood(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi", result.Message);
        }
    }
}
