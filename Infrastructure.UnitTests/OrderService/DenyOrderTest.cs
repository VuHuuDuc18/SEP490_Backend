using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
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
    public class DenyOrderTest
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

        public DenyOrderTest()
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
        public async Task DenyOrder_Successful()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                LivestockCircleId = Guid.NewGuid(),
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.DENIED;
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _service.DenyOrder(order.Id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Cập nhật thành công", result.Message);
            //Assert.True(result.Data);
            //Assert.Equal(OrderStatus.DENIED, order.Status);
        }

        [Fact]
        public async Task DenyOrder_OrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync((Order)null);

            // Act
            var result = await _service.DenyOrder(orderId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy đơn ", result.Message);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task DenyOrder_ExceptionOccurs()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                LivestockCircleId = Guid.NewGuid(),
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(order.Id, null)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(x => x.Update(order)).Callback(() =>
            {
                order.Status = OrderStatus.DENIED;
            });
            _orderRepositoryMock.Setup(x => x.CommitAsync(default)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DenyOrder(order.Id);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Cập nhật thất bại", result.Message);
            Assert.False(result.Data);
            Assert.Equal(OrderStatus.DENIED, order.Status); // Status is updated before the commit fails
        }
    }
}