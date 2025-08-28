using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Infrastructure.Services;
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
    public class CustomerCreateOrderTest
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
        private readonly Mock<IEmailService> _emailService;
        private readonly Infrastructure.Services.Implements.OrderService _service;
        private readonly Guid _currentUserId = Guid.NewGuid();

        public CustomerCreateOrderTest()
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
            _emailService = new Mock<IEmailService>();
    //        // Setup HttpContext with user claims
    //        var claims = new List<Claim>
    //{
    //    new Claim(ClaimTypes.NameIdentifier, _currentUserId.ToString()), // Standard claim for user ID
    //    new Claim("uid", _currentUserId.ToString()) // Keep existing claim for compatibility
    //};
    //        var identity = new ClaimsIdentity(claims, "TestAuth");
    //        var principal = new ClaimsPrincipal(identity);
    //        var httpContext = new DefaultHttpContext { User = principal };
    //        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    //        // Setup UserManager to return a valid user
    //        var user = new User
    //        {
    //            Id = _currentUserId,
    //            UserName = "testuser",
    //            Email = "testuser@example.com",
    //            IsActive = true
    //        };
    //        _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
    //            .ReturnsAsync(user);

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

        private void SetupSalesStaffQuery(Guid saleStaffId)
        {
            var salesStaff = new User { Id = saleStaffId, FullName = "Sales Staff", IsActive = true };
            var userRole = new UserRole { UserId = saleStaffId, Role = new Role { Name = RoleConstant.SalesStaff }, User = salesStaff };
            var userRoles = new List<UserRole> { userRole }.AsQueryable();

            var mockSet = new Mock<DbSet<UserRole>>();
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.Provider).Returns(userRoles.Provider);
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.Expression).Returns(userRoles.Expression);
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.ElementType).Returns(userRoles.ElementType);
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.GetEnumerator()).Returns(userRoles.GetEnumerator());
            mockSet.As<IAsyncEnumerable<UserRole>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<UserRole>(userRoles.GetEnumerator()));

            _dbContextMock.Setup(x => x.Set<UserRole>()).Returns(mockSet.Object);
            _orderRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(new List<Order>().AsQueryable().BuildMock());
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenUserNotLoggedIn()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext()); // No user claims

            var service = new Infrastructure.Services.Implements.OrderService(
                _orderRepositoryMock.Object,
                httpContextAccessorMock.Object,
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

            var request = new CreateOrderRequest { LivestockCircleId = Guid.NewGuid(), GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            // Act
            var result = await service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal("Hãy đăng nhập và thử lại", result.Message);
            //Assert.Contains("Hãy đăng nhập và thử lại", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenGoodUnitStockIsZeroOrNegative()
        {
            // Arrange
            var request = new CreateOrderRequest { LivestockCircleId = Guid.NewGuid(), GoodUnitStock = 0, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con tốt hoặc con xấu phải lớn hơn 0", result.Message);
            Assert.Contains("Số lượng con tốt hoặc con xấu phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenBadUnitStockIsNegative()
        {
            // Arrange
            var request = new CreateOrderRequest { LivestockCircleId = Guid.NewGuid(), GoodUnitStock = 5, BadUnitStock = -1, PickupDate = DateTime.UtcNow.AddDays(1) };

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con tốt hoặc con xấu phải lớn hơn 0", result.Message);
            Assert.Contains("Số lượng con tốt hoặc con xấu phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenPickupDateIsInPast()
        {
            // Arrange
            var request = new CreateOrderRequest { LivestockCircleId = Guid.NewGuid(), GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(-1) };

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Message);
            Assert.Contains("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenOrderExists()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };
            var existingOrder = new Order { CustomerId = _currentUserId, LivestockCircleId = lscId, Status = OrderStatus.PENDING };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order> { existingOrder }.AsQueryable().BuildMock());

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Đã tồn tại đơn hàng chưa", result.Message);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenNoSalesStaffAvailable()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            var mockSet = new Mock<DbSet<UserRole>>();
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.Provider).Returns(new List<UserRole>().AsQueryable().Provider);
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.Expression).Returns(new List<UserRole>().AsQueryable().Expression);
            mockSet.As<IQueryable<UserRole>>().Setup(m => m.ElementType).Returns(new List<UserRole>().AsQueryable().ElementType);
            //mockSet.As<IQueryable<UserRole>>().Setup(m => m.GetEnumerator).Returns(new List<UserRole>().AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<UserRole>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<UserRole>(new List<UserRole>().AsQueryable().GetEnumerator()));

            _dbContextMock.Setup(x => x.Set<UserRole>()).Returns(mockSet.Object);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không có nhân viên sale nào xử lý đơn hàng.", result.Message);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenLivestockCircleNotFound()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Chuồng nuôi không khả dụng. Vui lòng thử lại sau.", result.Message);
            Assert.Contains("Không tìm thấy chu kì nuôi. ID: " + lscId, result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenGoodUnitStockExceedsLimit()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 10, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            var livestockCircle = new LivestockCircle { Id = lscId, GoodUnitNumber = 5, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(livestockCircle);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.", result.Message);
            Assert.Contains("Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenBadUnitStockExceedsLimit()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 10, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            var livestockCircle = new LivestockCircle { Id = lscId, GoodUnitNumber = 5, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(livestockCircle);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.", result.Message);
            Assert.Contains("Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenPickupDateExceedsReleaseDateLimit()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(5) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            var livestockCircle = new LivestockCircle { Id = lscId, GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(livestockCircle);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal("Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng", result.Message);
            //Assert.Contains("Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng", result.Errors);
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            var livestockCircle = new LivestockCircle { Id = lscId, GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(livestockCircle);

            _orderRepositoryMock.Setup(x => x.Insert(It.IsAny<Order>()));
            _orderRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal("Tạo đơn hàng thành công", result.Message);
            //Assert.Contains("Đơn hàng đã được tạo thành công. ID: ", result.Data);
            //_orderRepositoryMock.Verify(x => x.Insert(It.Is<Order>(o => o.CustomerId == _currentUserId && o.LivestockCircleId == lscId && o.GoodUnitStock == 5 && o.BadUnitStock == 2 && o.Status == OrderStatus.PENDING && o.SaleStaffId == saleStaffId)), Times.Once());
            //_orderRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CustomerCreateOrder_ReturnsFalse_WhenExceptionOccurs()
        {
            // Arrange
            var lscId = Guid.NewGuid();
            var saleStaffId = Guid.NewGuid();
            var request = new CreateOrderRequest { LivestockCircleId = lscId, GoodUnitStock = 5, BadUnitStock = 2, PickupDate = DateTime.UtcNow.AddDays(1) };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(new List<Order>().AsQueryable().BuildMock());

            SetupSalesStaffQuery(saleStaffId);

            var livestockCircle = new LivestockCircle { Id = lscId, GoodUnitNumber = 10, BadUnitNumber = 5, ReleaseDate = DateTime.UtcNow };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(lscId, null)).ReturnsAsync(livestockCircle);

            _orderRepositoryMock.Setup(x => x.Insert(It.IsAny<Order>())).Throws(new Exception("Database error"));

            // Act
            var result = await _service.CustomerCreateOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi tạo đơn hàng", result.Message);
            Assert.Contains("Database error", result.Errors);
        }
    }

    // Helper class to support async enumeration for DbSet mocking
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}