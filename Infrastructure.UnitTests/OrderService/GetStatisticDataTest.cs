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
    public class GetStatisticDataTest
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

        public GetStatisticDataTest()
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
        public async Task GetStatisticData_Successful()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestStatisticDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestStatisticDbContext(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "Poultry description" }; // Thêm Description
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BreedId = breed.Id,
                Breed = breed,
                LivestockCircleName = "Cycle1", // Thêm LivestockCircleName
                Status = StatusConstant.GROWINGSTAT, // Thêm Status
                ReleaseDate = DateTime.UtcNow.AddDays(-5)
            };
            var order1 = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = StatusConstant.REQUESTED,
                GoodUnitPrice = 100,
                BadUnitPrice = 50,
                //TotalBill = 600,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                IsActive = true
            };
            var order2 = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                GoodUnitStock = 3,
                BadUnitStock = 1,
                Status = StatusConstant.REQUESTED,
                GoodUnitPrice = 120,
                BadUnitPrice = 60,
                //TotalBill = 420,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.AddRange(order1, order2);
            await context.SaveChangesAsync();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.LivestockCircles);
            _orderRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.Orders);
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.Breeds);
            _breedCategoryRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.BreedCategories);

            var request = new StatisticsOrderRequest
            {
                From = DateTime.UtcNow.AddDays(-4),
                To = DateTime.UtcNow
            };

            // Act
            var result = await _service.GetStatisticData(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
          //  Assert.Equal("Succeeded", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Datas);
            var data = result.Data.Datas[0];
            Assert.Equal("Chicken", data.BreedName);
            Assert.Equal("Poultry", data.BreedCategoryName);
            Assert.Equal(8, data.GoodUnitStockSold); // 5 + 3
            Assert.Equal(3, data.BadUnitStockSold);  // 2 + 1
            Assert.Equal(110, (int)data.AverageGoodUnitPrice); // (100 + 120) / 2
            Assert.Equal(55, (int)data.AverageBadUnitPrice);   // (50 + 60) / 2
            Assert.Equal(1045, data.Revenue); 
            //Assert.Equal(1020, result.Data.TotalRevenue);
            //Assert.Equal(8, result.Data.TotalGoodUnitStockSold);
            //Assert.Equal(3, result.Data.TotalBadUnitStockSold);
        }

        [Fact]
        public async Task GetStatisticData_InvalidDateRange()
        {
            // Arrange
            var request = new StatisticsOrderRequest
            {
                From = DateTime.UtcNow.AddDays(1), // Ngày sau
                To = DateTime.UtcNow.AddDays(-1)   // Ngày trước
            };

            // Act
            var result = await _service.GetStatisticData(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("gày bắt đầu phải trước hiện tại", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetStatisticData_NoDataInRange()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestStatisticDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestStatisticDbContext(options);

            var breedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Poultry", Description = "Poultry description" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Chicken", BreedCategoryId = breedCategory.Id, BreedCategory = breedCategory };
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BreedId = breed.Id,
                Breed = breed,
                LivestockCircleName = "Cycle1",
                Status = StatusConstant.GROWINGSTAT,
                ReleaseDate = DateTime.UtcNow.AddDays(-5)
            };
            var order = new Order
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircle.Id,
                LivestockCircle = livestockCircle,
                GoodUnitStock = 5,
                BadUnitStock = 2,
                Status = StatusConstant.REQUESTED,
                GoodUnitPrice = 100,
                BadUnitPrice = 50,
                //TotalBill = 600,
                CreatedDate = DateTime.UtcNow.AddDays(-10), // Ngoài phạm vi
                IsActive = true
            };

            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.LivestockCircles.Add(livestockCircle);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.LivestockCircles);
            _orderRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.Orders);
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.Breeds);
            _breedCategoryRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.BreedCategories);

            var request = new StatisticsOrderRequest
            {
                From = DateTime.UtcNow.AddDays(-4),
                To = DateTime.UtcNow
            };

            // Act
            var result = await _service.GetStatisticData(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
           // Assert.Equal("Succeeded", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Datas);
            Assert.Equal(0, result.Data.TotalRevenue);
            Assert.Equal(0, result.Data.TotalGoodUnitStockSold);
            Assert.Equal(0, result.Data.TotalBadUnitStockSold);
        }

        //[Fact]
        //public async Task GetStatisticData_ExceptionOccurs()
        //{
        //    // Arrange
        //    var request = new StatisticsOrderRequest
        //    {
        //        From = DateTime.UtcNow.AddDays(-4),
        //        To = DateTime.UtcNow
        //    };
        //    _orderRepositoryMock.Setup(x => x.GetQueryable()).Throws(new Exception("Database error"));

        //    // Act
        //    var result = await _service.GetStatisticData(request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Database error", result.Message);
        //    Assert.Null(result.Data);
        //}
    }

    // Minimal InMemory DbContext for test
    public class TestStatisticDbContext : DbContext
    {
        public TestStatisticDbContext(DbContextOptions<TestStatisticDbContext> options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<BreedCategory> BreedCategories { get; set; }
    }
}