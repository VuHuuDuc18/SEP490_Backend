using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Domain.DTOs.Response.Order;
using Domain.Helper;
using Domain.Helper.Constants;
using Entities.EntityModel;
using Infrastructure.DBContext;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.OrderService
{
    public class CustomerGetAllOrdersTest
    {
        private readonly Mock<IRepository<Order>> _orderRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<Role>> _roleManagerMock;
        private readonly Mock<IRepository<Role>> _roleRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLivestockCircleRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<LCFMSDBContext> _dbContextMock;
        private readonly Infrastructure.Services.Implements.OrderService _service;
        private readonly Guid _currentUserId = Guid.NewGuid();

        public CustomerGetAllOrdersTest()
        {
            _orderRepositoryMock = new Mock<IRepository<Order>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _userManagerMock = new Mock<UserManager<User>>(new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            _roleManagerMock = new Mock<RoleManager<Role>>(new Mock<IRoleStore<Role>>().Object, null, null, null, null);
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _breedCategoryRepositoryMock = new Mock<IRepository<BreedCategory>>();
            _imageLivestockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _roleRepositoryMock = new Mock<IRepository<Role>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _dbContextMock = new Mock<LCFMSDBContext>(new DbContextOptions<LCFMSDBContext>());

            var claims = new List<Claim>
            {
                new Claim("uid", _currentUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _service = new Infrastructure.Services.Implements.OrderService(
                _orderRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _livestockCircleRepositoryMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _userRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _breedCategoryRepositoryMock.Object,
                _imageLivestockCircleRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _dbContextMock.Object
            );
        }

        [Fact]
        public async Task CustomerGetAllOrders_Successful()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext(options);

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = _currentUserId,
                LivestockCircle = new LivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleName = "test",
                    Status = StatusConstant.GROWINGSTAT,
                    Breed = new Breed { BreedName = "Chicken", BreedCategory = new BreedCategory { Name = "Poultry", Description = "test" } },
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1" }
                },
                GoodUnitStock = 5,
                BadUnitStock = 2,
                //TotalBill = 1000,
                Status = OrderStatus.PENDING,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                PickupDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };
            var order2 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = _currentUserId,
                LivestockCircle = new LivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleName = "test1",
                    Status = StatusConstant.GROWINGSTAT,
                    Breed = new Breed { BreedName = "Duck", BreedCategory = new BreedCategory { Name = "Poultry", Description="test" } },
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn2", Address = "Test Address 2", Image = "Test Image 2" }
                },
                GoodUnitStock = 3,
                BadUnitStock = 1,
                //TotalBill = 500,
                Status = OrderStatus.APPROVED,
                CreatedDate = DateTime.UtcNow,
                PickupDate = DateTime.UtcNow.AddDays(2),
                IsActive = true
            };

            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Returns(context.Orders);

            // Act
            var result = await _service.CustomerGetAllOrders(default);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách đơn hàng thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            //Assert.Equal(order1.Id, result.Data[0].Id);
            //Assert.Equal(order2.Id, result.Data[1].Id);
            //Assert.Equal("Chicken", result.Data[0].BreedName);
            //Assert.Equal("Duck", result.Data[1].BreedName);
            //Assert.NotNull(result.Data[0].Barn);
            //Assert.NotNull(result.Data[1].Barn);
            //Assert.Equal("Barn1", result.Data[0].Barn.BarnName);
            //Assert.Equal("Barn2", result.Data[1].Barn.BarnName);
        }

        [Fact]
        public async Task CustomerGetAllOrders_NotLoggedIn()
        {
            // Arrange
            var invalidUserId = Guid.Empty;
            var claims = new List<Claim>
            {
                new Claim("uid", invalidUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var service = new Infrastructure.Services.Implements.OrderService(
                _orderRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _livestockCircleRepositoryMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _userRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _breedCategoryRepositoryMock.Object,
                _imageLivestockCircleRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _dbContextMock.Object
            );

            // Act
            var result = await service.CustomerGetAllOrders(default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Hãy đăng nhập và thử lại", result.Message);
            Assert.Contains("Hãy đăng nhập và thử lại", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CustomerGetAllOrders_NoOrdersFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext(options);

            context.Orders.AddRange(); // Không thêm đơn hàng
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Returns(context.Orders);

            // Act
            var result = await _service.CustomerGetAllOrders(default);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách đơn hàng thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CustomerGetAllOrders_ExceptionOccurs()
        {
            // Arrange
            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Throws(new Exception("Database error"));

            // Act
            var result = await _service.CustomerGetAllOrders(default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy danh sách đơn hàng", result.Message);
            Assert.Contains("Database error", result.Errors);
            Assert.Null(result.Data);
        }
    }

    // Minimal InMemory DbContext for test
    public class TestOrderDbContext : DbContext
    {
        public TestOrderDbContext(DbContextOptions<TestOrderDbContext> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
    }
}