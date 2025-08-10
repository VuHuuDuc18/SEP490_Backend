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
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using OfficeOpenXml.Filter;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.OrderService
{
    public class WorkerGetallOrderTest
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

        public WorkerGetallOrderTest()
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
        public async Task WorkerGetallOrder_Successful()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestOrderDbContext4>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestOrderDbContext4(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "Poultry description" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1", WorkerId = _currentUserId };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                LivestockCircleName = "Cycle1",
                Status = StatusConstant.GROWINGSTAT,
                BreedId = breed.Id,
                Breed = breed,
                BarnId = barn.Id,
                Barn = barn
            };
            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
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
                CustomerId = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                GoodUnitStock = 3,
                BadUnitStock = 1,
                //TotalBill = 500,
                Status = OrderStatus.APPROVED,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                PickupDate = DateTime.UtcNow.AddDays(2),
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.Barns.Add(barn);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            var ordersQueryable = context.Orders
                .Include(x => x.LivestockCircle)
                .ThenInclude(x => x.Breed)
                .ThenInclude(x => x.BreedCategory)
                .Include(x => x.LivestockCircle)
                .ThenInclude(x => x.Barn)
                .AsQueryable();

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Returns(ordersQueryable);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 1,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "asc" }
            };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            Assert.Equal(order2.Id, result.Data.Items[0].Id); // Latest order (desc)
            Assert.Equal("Chicken", result.Data.Items[0].BreedName);
            Assert.Equal("Poultry", result.Data.Items[0].BreedCategory);
            Assert.NotNull(result.Data.Items[0].Barn);
            Assert.Equal("Barn1", result.Data.Items[0].Barn.BarnName);
        }

        [Fact]
        public async Task WorkerGetallOrder_RequestNull()
        {
            // Act
            var result = await _service.WorkerGetallOrder(null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được để trống", result.Message);
            Assert.Contains("Yêu cầu không được để trống", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task WorkerGetallOrder_InvalidPageIndex()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task WorkerGetallOrder_InvalidPageSize()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task WorkerGetallOrder_InvalidFilterField()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField",  Value = "test" } }
            };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
            Assert.Contains("Trường hợp lệ", result.Errors.FirstOrDefault());
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task WorkerGetallOrder_InvalidSearchField()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message);
            Assert.Contains("Trường hợp lệ", result.Errors.FirstOrDefault());
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task WorkerGetallOrder_InvalidSortField()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ: InvalidField", result.Message);
            Assert.Contains("Trường hợp lệ", result.Errors.FirstOrDefault());
            Assert.Null(result.Data);
        }

        //[Fact]
        //public async Task WorkerGetallOrder_NoMatchingOrders()
        //{
        //    // Arrange
        //    var options = new DbContextOptionsBuilder<TestOrderDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestOrderDbContext4(options);

        //    var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "Poultry description" };
        //    var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
        //    var barn = new Barn { Id = Guid.NewGuid(), BarnName = "Barn1", Address = "Test Address 1", Image = "Test Image 1", WorkerId = Guid.NewGuid() }; // Different WorkerId
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleName = "Cycle1",
        //        Status = StatusConstant.GROWINGSTAT,
        //        BreedId = breed.Id,
        //        Breed = breed,
        //        BarnId = barn.Id,
        //        Barn = barn
        //    };
        //    var order = new Order
        //    {
        //        Id = Guid.NewGuid(),
        //        CustomerId = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircle.Id,
        //        LivestockCircle = livestockCircle,
        //        GoodUnitStock = 5,
        //        BadUnitStock = 2,
        //        TotalBill = 1000,
        //        Status = OrderStatus.PENDING,
        //        CreatedDate = DateTime.UtcNow,
        //        PickupDate = DateTime.UtcNow.AddDays(1),
        //        IsActive = true
        //    };

        //    context.BreedCategories.Add(breedCategory);
        //    context.Breeds.Add(breed);
        //    context.Barns.Add(barn);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.Orders.Add(order);
        //    await context.SaveChangesAsync();

        //    var ordersQueryable = context.Orders
        //        .Include(x => x.LivestockCircle)
        //        .ThenInclude(x => x.Breed)
        //        .ThenInclude(x => x.BreedCategory)
        //        .Include(x => x.LivestockCircle)
        //        .ThenInclude(x => x.Barn)
        //        .AsQueryable();

        //    _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
        //        .Returns(ordersQueryable);

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "asc" }
        //    };

        //    // Act
        //    var result = await _service.WorkerGetallOrder(request);

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Equal("Lấy dữ liệu thành công.", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Empty(result.Data.Items);
        //}

        [Fact]
        public async Task WorkerGetallOrder_ExceptionOccurs()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "asc" }
            };

            _orderRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Order, bool>>>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _service.WorkerGetallOrder(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy danh sách: Database error", result.Message);
            Assert.Null(result.Data);
        }
    }

    public class TestOrderDbContext4 : DbContext
    {
        public TestOrderDbContext4(DbContextOptions<TestOrderDbContext4> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<BreedCategory> BreedCategories { get; set; }
        public DbSet<Barn> Barns { get; set; }
    }
}