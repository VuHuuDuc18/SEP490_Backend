using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
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
    public class CustomerGetPagiantionListTest
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
        private readonly Mock<Infrastructure.Services.IEmailService> _emailService = new Mock<Infrastructure.Services.IEmailService>();
        public CustomerGetPagiantionListTest()
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
        public async Task CustomerGetPagiantionList_Successful()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext1(options);

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = _currentUserId,
                LivestockCircle = new LivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleName = "Cycle1", // Thêm thuộc tính mới
                    Status = StatusConstant.GROWINGSTAT, // Thêm thuộc tính mới
                    Breed = new Breed { BreedName = "Chicken", BreedCategory = new BreedCategory { Name = "Poultry", Description = "Poultry description" } }, // Thêm Description
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1" }
                },
                GoodUnitStock = 5,
                BadUnitStock = 2,
                //TotalBill = 1000,
                Status = OrderStatus.PENDING,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
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
                    LivestockCircleName = "Cycle2", // Thêm thuộc tính mới
                    Status = StatusConstant.DONESTAT, // Thêm thuộc tính mới
                    Breed = new Breed { BreedName = "Duck", BreedCategory = new BreedCategory { Name = "Poultry", Description = "Poultry description" } }, // Thêm Description
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn2", Address = "Test Address 2", Image = "Test Image 2" }
                },
                GoodUnitStock = 3,
                BadUnitStock = 1,
                //TotalBill = 500,
                Status = OrderStatus.APPROVED,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                PickupDate = DateTime.UtcNow.AddDays(2),
                IsActive = true
            };

            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Returns(context.Orders);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 1,
                Sort = new SearchObjectForCondition { Field = "CreateDate", Value = "desc" }
            };

            // Act
            var result = await _service.CustomerGetPagiantionList(request, default);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            //Assert.Equal(2, result.Data.TotalItems); // Tổng số đơn hàng
            //Assert.Single(result.Data.Items);
            //Assert.Equal(order2.Id, result.Data.Items[0].Id); // Kiểm tra order mới nhất (desc)
            //Assert.Equal("Duck", result.Data.Items[0].BreedName);
            //Assert.Equal("Poultry", result.Data.Items[0].BreedCategory);
            //Assert.NotNull(result.Data.Items[0].Barn);
            //Assert.Equal("Barn2", result.Data.Items[0].Barn.BarnName);
        }

        [Fact]
        public async Task CustomerGetPagiantionList_RequestNull()
        {
            // Arrange
            // No request object

            // Act
            var result = await _service.CustomerGetPagiantionList(null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CustomerGetPagiantionList_InvalidPageIndex()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _service.CustomerGetPagiantionList(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CustomerGetPagiantionList_InvalidPageSize()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _service.CustomerGetPagiantionList(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CustomerGetPagiantionList_InvalidFilterField()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext1(options);

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = _currentUserId,
                LivestockCircle = new LivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleName = "Cycle1",
                    Status = StatusConstant.GROWINGSTAT,
                    Breed = new Breed { BreedName = "Chicken", BreedCategory = new BreedCategory { Name = "Poultry", Description = "Poultry description" } },
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1" }
                },
                GoodUnitStock = 5,
                BadUnitStock = 2,
                //TotalBill = 1000,
                Status = OrderStatus.PENDING,
                CreatedDate = DateTime.UtcNow,
                PickupDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };

            context.Orders.Add(order1);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Returns(context.Orders);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _service.CustomerGetPagiantionList(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ: InvalidField", result.Message);
            Assert.Contains("Trường hợp lệ", result.Errors.FirstOrDefault());
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task CustomerGetPagiantionList_InvalidSortField()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext1(options);

            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = _currentUserId,
                LivestockCircle = new LivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleName = "Cycle1",
                    Status = StatusConstant.GROWINGSTAT,
                    Breed = new Breed { BreedName = "Chicken", BreedCategory = new BreedCategory { Name = "Poultry", Description = "Poultry description" } },
                    Barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1" }
                },
                GoodUnitStock = 5,
                BadUnitStock = 2,
                //TotalBill = 1000,
                Status = OrderStatus.PENDING,
                CreatedDate = DateTime.UtcNow,
                PickupDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };

            context.Orders.Add(order1);
            await context.SaveChangesAsync();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Returns(context.Orders);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _service.CustomerGetPagiantionList(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ: InvalidField", result.Message);
            Assert.Contains("Trường hợp lệ", result.Errors.FirstOrDefault());
            Assert.Null(result.Data);
        }

        //[Fact]
        //public async Task CustomerGetPagiantionList_ExceptionOccurs()
        //{
        //    // Arrange
        //    var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>())).Throws(new Exception("Database error"));

        //    // Act
        //    var result = await _service.CustomerGetPagiantionList(request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách đơn hàng", result.Message);
        //    Assert.Contains("Database error", result.Errors);
        //    Assert.Null(result.Data);
        //}
    }

    // Minimal InMemory DbContext for test
    public class TestOrderDbContext1 : DbContext
    {
        public TestOrderDbContext1(DbContextOptions<TestOrderDbContext1> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
    }
}