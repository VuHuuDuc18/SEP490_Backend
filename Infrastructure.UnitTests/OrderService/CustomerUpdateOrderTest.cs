using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.DTOs.Request.Order;
using Domain.Helper;
using Domain.Helper.Constants;
using Entities.EntityModel;
using Infrastructure.DBContext;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.OrderService
{
    public class CustomerUpdateOrderTest
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
        private readonly Mock<Infrastructure.Services.IEmailService> _emailService = new Mock<Infrastructure.Services.IEmailService>();
        private readonly Guid _currentUserId = Guid.NewGuid();

        public CustomerUpdateOrderTest()
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
            _emailService = new Mock<Infrastructure.Services.IEmailService>();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _currentUserId.ToString()),
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
                _dbContextMock.Object,
                _emailService.Object
            );
        }

        [Fact]
        public async Task CustomerUpdateOrder_SuccessfulUpdate()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>()));
            _orderRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Cập nhật đơn hàng thành công", result.Message);
            Assert.Contains("Đơn hàng đã được cập nhật thành công. ID: " + orderId, result.Data);
            Assert.Equal(3, order.GoodUnitStock);
            Assert.Equal(1, order.BadUnitStock);
            Assert.Equal(request.PickupDate, order.PickupDate);
            _orderRepositoryMock.Verify(x => x.Update(order), Times.Once());
            _orderRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CustomerUpdateOrder_OrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync((Order)null);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Đơn hàng không tồn tại hoặc đã bị xóa.", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_NotOwner()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };
            var otherUserId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerId = otherUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Bạn không thể sửa đơn hàng này.", result.Message);
            Assert.Contains("Không thể sửa đơn hàng của người khác.", result.Errors);
        }

        [Fact]
        public async Task CustomerUpdateOrder_NotPendingStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.APPROVED,
                IsActive = true
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Đơn hàng không thể cập nhật với trạng thái hiện tại.", result.Message);
            Assert.Contains("Đơn hàng không thể cập nhật với trạng thái hiện tại - " + OrderStatus.APPROVED, result.Errors);
        }

        [Fact]
        public async Task CustomerUpdateOrder_GoodUnitStockExceedsLimit()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 11, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con tốt vượt quá số lượng con trong chuồng nuôi", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_BadUnitStockExceedsLimit()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 6, PickupDate = DateTime.UtcNow.AddDays(1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con xấu vượt quá số lượng con trong chuồng nuôi", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_PickupDateInPast()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(-1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_BadUnit()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(-1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_PickupDate()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(-1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_PickupDateNull()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(-1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        [Fact]
        public async Task CustomerUpdateOrder_GoodUnit()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(-1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
        }

        //[Fact]
        //public async Task CustomerUpdateOrder_PickupDateExceedsReleaseDateLimit()
        //{
        //    // Arrange
        //    var orderId = Guid.NewGuid();
        //    var livestockCircleId = Guid.NewGuid();
        //    var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(4) };
        //    var order = new Order
        //    {
        //        Id = orderId,
        //        CustomerId = _currentUserId,
        //        LivestockCircleId = livestockCircleId,
        //        GoodUnitStock = 5,
        //        BadUnitStock = 2,
        //        Status = OrderStatus.PENDING,
        //        IsActive = true,
        //        LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
        //    };

        //    _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);

        //    // Act
        //    var result = await _service.CustomerUpdateOrder(request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng", result.Message);
        //}

        [Fact]
        public async Task CustomerUpdateOrder_ExceptionOccurs()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var request = new UpdateOrderRequest { OrderId = orderId, GoodUnitStock = 3, BadUnitStock = 1, PickupDate = DateTime.UtcNow.AddDays(1) };
            var order = new Order
            {
                Id = orderId,
                CustomerId = _currentUserId,
                LivestockCircleId = livestockCircleId,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = OrderStatus.PENDING,
                IsActive = true,
                LivestockCircle = new LivestockCircle { GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, null)).ReturnsAsync(order);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(order.LivestockCircle);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).Throws(new Exception("Database error"));

            // Act
            var result = await _service.CustomerUpdateOrder(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi cập nhật đơn hàng", result.Message);
            Assert.Contains("Database error", result.Errors);
        }
    }
}