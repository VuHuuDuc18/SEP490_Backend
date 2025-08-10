using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Domain.DTOs.Request.Order;
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
    public class ApproveOrderTest
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

        public ApproveOrderTest()
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
                new Claim("uid", Guid.NewGuid().ToString())
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
        public async Task ApproveOrder_Successful()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext3(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry" ,Description ="test" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BreedId = breed.Id,
                LivestockCircleName = "test",
                Breed = breed,
                GoodUnitNumber = 10,
                BadUnitNumber = 5,
                Status = StatusConstant.GROWINGSTAT
            };
            var order = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                CustomerId = Guid.NewGuid(),
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircle.Id, null)).ReturnsAsync(livestockCircle);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.APPROVED;
                order.GoodUnitPrice = 100;
                order.BadUnitPrice = 50;
               // order.TotalBill = (order.GoodUnitStock * order.GoodUnitPrice) + (order.BadUnitStock * order.BadUnitPrice);
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _livestockCircleRepositoryMock.Setup(x => x.Update(livestockCircle)).Callback(() =>
            {
                livestockCircle.GoodUnitNumber -= order.GoodUnitStock;
                livestockCircle.BadUnitNumber -= order.BadUnitStock;
                if (livestockCircle.GoodUnitNumber == 0 && livestockCircle.BadUnitNumber == 0)
                    livestockCircle.Status = StatusConstant.DONESTAT;
            });
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var request = new ApproveOrderRequest
            {
                OrderId = order.Id,
                GoodUnitPrice = 100,
                BadUnitPrice = 50
            };

            // Act
            var result = await _service.ApproveOrder(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Cập nhật thành công", result.Message);
            //Assert.True(result.Data);
            //Assert.Equal(OrderStatus.APPROVED, order.Status);
            //Assert.Equal(100, order.GoodUnitPrice);
            //Assert.Equal(50, order.BadUnitPrice);
            //Assert.Equal(600, order.TotalBill); // 5 * 100 + 2 * 50
            //Assert.Equal(5, livestockCircle.GoodUnitNumber); // 10 - 5
            //Assert.Equal(3, livestockCircle.BadUnitNumber); // 5 - 2
            //Assert.Equal(StatusConstant.GROWINGSTAT, livestockCircle.Status);
        }

        [Fact]
        public async Task ApproveOrder_OrderNotFound()
        {
            // Arrange
            _orderRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Order)null);

            var request = new ApproveOrderRequest
            {
                OrderId = Guid.NewGuid(),
                GoodUnitPrice = 100,
                BadUnitPrice = 50
            };

            // Act
            var result = await _service.ApproveOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy đơn ", result.Message);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task ApproveOrder_ExceptionOccurs()
        {
            // Arrange
            var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.PENDING, IsActive = true };
            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.APPROVED;
                order.GoodUnitPrice = 100;
                order.BadUnitPrice = 50;
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("Database error"));

            var request = new ApproveOrderRequest
            {
                OrderId = order.Id,
                GoodUnitPrice = 100,
                BadUnitPrice = 50
            };

            // Act
            var result = await _service.ApproveOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Cập nhật thất bại", result.Message);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task ApproveOrder_LivestockCircleUpdateFails()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext3(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "test" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BreedId = breed.Id,
                Breed = breed,
                LivestockCircleName = "test",
                GoodUnitNumber = 10,
                BadUnitNumber = 5,
                Status = StatusConstant.GROWINGSTAT
            };
            var order = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                CustomerId = Guid.NewGuid(),
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircle.Id, null)).ReturnsAsync(livestockCircle);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.APPROVED;
                order.GoodUnitPrice = 100;
                order.BadUnitPrice = 50;
                //order.TotalBill = (order.GoodUnitStock * order.GoodUnitPrice) + (order.BadUnitStock * order.BadUnitPrice);
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _livestockCircleRepositoryMock.Setup(x => x.Update(livestockCircle)).Callback(() =>
            {
                livestockCircle.GoodUnitNumber -= order.GoodUnitStock;
                livestockCircle.BadUnitNumber -= order.BadUnitStock;
            });
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("Livestock update failed"));

            var request = new ApproveOrderRequest
            {
                OrderId = order.Id,
                GoodUnitPrice = 100,
                BadUnitPrice = 50
            };

            // Act
            var result = await _service.ApproveOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Cập nhật thất bại", result.Message);
            Assert.False(result.Data);
            Assert.Equal(OrderStatus.APPROVED, order.Status);
            Assert.Equal(100, order.GoodUnitPrice);
            Assert.Equal(50, order.BadUnitPrice);
            //Assert.Equal(600, order.TotalBill); // 5 * 100 + 2 * 50
            //Assert.Equal(5, livestockCircle.GoodUnitNumber); // 10 - 5
            //Assert.Equal(3, livestockCircle.BadUnitNumber); // 5 - 2
        }

        [Fact]
        public async Task ApproveOrder_AllUnitsSold()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext3(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "test" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BreedId = breed.Id,
                Breed = breed,
                LivestockCircleName = "test",
                GoodUnitNumber = 5,
                BadUnitNumber = 2,
                Status = StatusConstant.GROWINGSTAT
            };
            var order = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                CustomerId = Guid.NewGuid(),
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircle.Id, null)).ReturnsAsync(livestockCircle);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.APPROVED;
                order.GoodUnitPrice = 100;
                order.BadUnitPrice = 50;
                //order.TotalBill = (order.GoodUnitStock * order.GoodUnitPrice) + (order.BadUnitStock * order.BadUnitPrice);
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            _livestockCircleRepositoryMock.Setup(x => x.Update(livestockCircle)).Callback(() =>
            {
                livestockCircle.GoodUnitNumber -= order.GoodUnitStock;
                livestockCircle.BadUnitNumber -= order.BadUnitStock;
                if (livestockCircle.GoodUnitNumber == 0 && livestockCircle.BadUnitNumber == 0)
                    livestockCircle.Status = StatusConstant.DONESTAT;
            });
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            var request = new ApproveOrderRequest
            {
                OrderId = order.Id,
                GoodUnitPrice = 100,
                BadUnitPrice = 50
            };

            // Act
            var result = await _service.ApproveOrder(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Cập nhật thành công", result.Message);
            //Assert.True(result.Data);
            //Assert.Equal(OrderStatus.APPROVED, order.Status);
            //Assert.Equal(100, order.GoodUnitPrice);
            //Assert.Equal(50, order.BadUnitPrice);
            //Assert.Equal(600, order.TotalBill); // 5 * 100 + 2 * 50
            //Assert.Equal(0, livestockCircle.GoodUnitNumber); // 5 - 5
            //Assert.Equal(0, livestockCircle.BadUnitNumber); // 2 - 2
            //Assert.Equal(StatusConstant.DONESTAT, livestockCircle.Status);
        }
    }

    public class TestOrderDbContext3 : DbContext
    {
        public TestOrderDbContext3(DbContextOptions<TestOrderDbContext3> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<BreedCategory> BreedCategories { get; set; }
    }
}