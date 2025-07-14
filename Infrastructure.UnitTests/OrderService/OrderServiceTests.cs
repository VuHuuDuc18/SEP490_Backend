using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Domain.DTOs.Request.Order;
using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.OrderService
{
    public class OrderServiceTests
    {
        private readonly Mock<IRepository<Order>> _mockOrderRepo = new();
        private readonly Mock<IRepository<LivestockCircle>> _mockLivestockRepo = new();
        private readonly Mock<IRepository<Breed>> _mockBreedRepo = new();
        private readonly Mock<IRepository<BreedCategory>> _mockBreedCategoryRepo = new();
        private readonly Mock<IRepository<ImageLivestockCircle>> _mockImageLivestockRepo = new();
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Guid _testUserId = Guid.NewGuid();

        public OrderServiceTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Infrastructure.Services.Implements.OrderService CreateOrderServiceWithUser(bool hasUser = true, string uid = null)
        {
            var claims = new List<Claim>();
            if (hasUser)
            {
                claims.Add(new Claim("uid", uid ?? _testUserId.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            return new Infrastructure.Services.Implements.OrderService(
                _mockOrderRepo.Object,
                httpContextAccessor.Object,
                _mockLivestockRepo.Object,
                _mockUserManager.Object,
                _mockBreedRepo.Object,
                _mockBreedCategoryRepo.Object,
                _mockImageLivestockRepo.Object
            );
        }

        [Fact]
        public async Task CustomerCreateOrder_ShouldFail_WhenUserIdEmpty()
        {
            var service = CreateOrderServiceWithUser(false);
            var request = new CreateOrderRequest();

            var result = await service.CustomerCreateOrder(request);

            Assert.False(result.Succeeded);
            Assert.Equal("Hãy đăng nhập và thử lại", result.Message);
        }

        [Fact]
        public async Task CustomerCreateOrder_ShouldFail_WhenBadOrGoodStockInvalid()
        {
            var service = CreateOrderServiceWithUser();
            var request = new CreateOrderRequest
            {
                GoodUnitStock = 0,
                BadUnitStock = -1
            };

            var result = await service.CustomerCreateOrder(request);

            Assert.False(result.Succeeded);
            Assert.Contains("Số lượng con tốt hoặc con xấu phải lớn hơn 0", result.Message);
        }

        [Fact]
        public async Task CustomerCreateOrder_ShouldFail_WhenPickupDateInPast()
        {
            var service = CreateOrderServiceWithUser();
            var request = new CreateOrderRequest
            {
                GoodUnitStock = 5,
                BadUnitStock = 0,
                PickupDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CustomerCreateOrder(request);

            Assert.False(result.Succeeded);
            Assert.Contains("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        [Fact]
        public async Task CustomerCreateOrder_ShouldSucceed_WhenValidRequest()
        {
            var service = CreateOrderServiceWithUser();

            var request = new CreateOrderRequest
            {
                LivestockCircleId = Guid.NewGuid(),
                GoodUnitStock = 3,
                BadUnitStock = 1,
                PickupDate = DateTime.UtcNow.AddDays(1)
            };

            _mockOrderRepo.Setup(repo =>
                repo.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
                .Returns((IQueryable<Order>)(Order)null); // No existing order

            _mockLivestockRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
                 .ReturnsAsync(new LivestockCircle
                {
                    GoodUnitNumber = 10,
                    BadUnitNumber = 5,
                    ReleaseDate = DateTime.UtcNow
                });

            _mockOrderRepo.Setup(r => r.Insert(It.IsAny<Order>()));
            _mockOrderRepo.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>())).Returns((Task<int>)Task.CompletedTask);

            var result = await service.CustomerCreateOrder(request);

            Assert.True(result.Succeeded);
            Assert.Contains("Tạo đơn hàng thành công", result.Message);
        }
    }

}
