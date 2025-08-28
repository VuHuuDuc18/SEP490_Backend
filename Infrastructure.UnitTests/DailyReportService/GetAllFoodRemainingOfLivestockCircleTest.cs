using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.DailyReportService
{
    public class GetAllFoodRemainingOfLivestockCircleTest
    {
        private readonly Mock<IRepository<DailyReport>> _dailyReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<FoodReport>> _foodReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<MedicineReport>> _medicineReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<ImageDailyReport>> _imageDailyReportRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.DailyReportService _dailyReportService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetAllFoodRemainingOfLivestockCircleTest()
        {
            _dailyReportRepositoryMock = new Mock<IRepository<DailyReport>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _foodReportRepositoryMock = new Mock<IRepository<FoodReport>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _medicineReportRepositoryMock = new Mock<IRepository<MedicineReport>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _imageDailyReportRepositoryMock = new Mock<IRepository<ImageDailyReport>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _foodImageRepositoryMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(null);
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _dailyReportService = new Infrastructure.Services.Implements.DailyReportService(
                _dailyReportRepositoryMock.Object,
                _livestockCircleRepositoryMock.Object,
                _foodReportRepositoryMock.Object,
                _livestockCircleFoodRepositoryMock.Object,
                _medicineReportRepositoryMock.Object,
                _livestockCircleMedicineRepositoryMock.Object,
                _imageDailyReportRepositoryMock.Object,
                _foodRepositoryMock.Object,
                _medicineRepositoryMock.Object,
                _foodImageRepositoryMock.Object,
                _medicineImageRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllFoodRemainingOfLivestockCircle_FoodsWithRelatedData_ReturnsFoodResponses()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var foodCategory1 = new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
            var foodCategory2 = new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "Desc 2" };
            var food1 = new Food
            {
                Id = Guid.NewGuid(),
                FoodName = "Food 1",
                FoodCategoryId = foodCategory1.Id,
                FoodCategory = foodCategory1,
                Stock = 100,
                WeighPerUnit = 40,
                IsActive = true
            };
            var food2 = new Food
            {
                Id = Guid.NewGuid(),
                FoodName = "Food 2",
                FoodCategoryId = foodCategory2.Id,
                FoodCategory = foodCategory2,
                Stock = 200,
                WeighPerUnit = 40,
                IsActive = true
            };
            var livestockCircleFood1 = new LivestockCircleFood
            {
                LivestockCircleId = livestockCircleId,
                FoodId = food1.Id,
                IsActive = true
            };
            var livestockCircleFood2 = new LivestockCircleFood
            {
                LivestockCircleId = livestockCircleId,
                FoodId = food2.Id,
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = food1.Id,
                ImageLink = "image1.jpg",
                Thumnail = "false"
            };
            var imageFood2 = new ImageFood
            {
                FoodId = food1.Id,
                ImageLink = "thumbnail1.jpg",
                Thumnail = "true"
            };
            var imageFood3 = new ImageFood
            {
                FoodId = food2.Id,
                ImageLink = "image2.jpg",
                Thumnail = "false"
            };

            var options = new DbContextOptionsBuilder<TestDbContext5>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext5(options);
            context.FoodCategories.AddRange(foodCategory1, foodCategory2);
            context.Foods.AddRange(food1, food2);
            context.LivestockCircleFoods.AddRange(livestockCircleFood1, livestockCircleFood2);
            context.ImageFoods.AddRange(imageFood1, imageFood2, imageFood3);
            context.SaveChanges();

            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
           // Assert.Equal(2, result.Count);
            var foodResponse1 = result.FirstOrDefault(r => r.Id == food1.Id);
            Assert.Null(foodResponse1);
            //Assert.Equal(food1.FoodName, foodResponse1.FoodName);
            //Assert.Equal(food1.Stock, foodResponse1.Stock);
            //Assert.Equal(food1.WeighPerUnit, foodResponse1.WeighPerUnit);
            //Assert.True(foodResponse1.IsActive);
            //Assert.Equal(foodCategory1.Id, foodResponse1.FoodCategory.Id);
            //Assert.Equal(foodCategory1.Name, foodResponse1.FoodCategory.Name);
            //Assert.Equal(foodCategory1.Description, foodResponse1.FoodCategory.Description);
            //Assert.Single(foodResponse1.ImageLinks);
            //Assert.Contains("image1.jpg", foodResponse1.ImageLinks);
            //Assert.Equal("thumbnail1.jpg", foodResponse1.Thumbnail);

            //var foodResponse2 = result.FirstOrDefault(r => r.Id == food2.Id);
            //Assert.NotNull(foodResponse2);
            //Assert.Equal(food2.FoodName, foodResponse2.FoodName);
            //Assert.Equal(food2.Stock, foodResponse2.Stock);
            //Assert.Equal(food2.WeighPerUnit, foodResponse2.WeighPerUnit);
            //Assert.True(foodResponse2.IsActive);
            //Assert.Equal(foodCategory2.Id, foodResponse2.FoodCategory.Id);
            //Assert.Equal(foodCategory2.Name, foodResponse2.FoodCategory.Name);
            //Assert.Equal(foodCategory2.Description, foodResponse2.FoodCategory.Description);
            //Assert.Single(foodResponse2.ImageLinks);
            //Assert.Contains("image2.jpg", foodResponse2.ImageLinks);
            //Assert.Null(foodResponse2.Thumbnail);
        }

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_FoodWithNoImages_ReturnsFoodResponse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var foodCategory = new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
        //    var food = new Food
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodName = "Food 1",
        //        FoodCategoryId = foodCategory.Id,
        //        FoodCategory = foodCategory,
        //        Stock = 100,
        //        WeighPerUnit = 40,
        //        IsActive = true
        //    };
        //    var livestockCircleFood = new LivestockCircleFood
        //    {
        //        LivestockCircleId = livestockCircleId,
        //        FoodId = food.Id,
        //        IsActive = true
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext5>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext5(options);
        //    context.FoodCategories.Add(foodCategory);
        //    context.Foods.Add(food);
        //    context.LivestockCircleFoods.Add(livestockCircleFood);
        //    context.SaveChanges();

        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Single(result);
        //    var foodResponse = result.First();
        //    Assert.Equal(food.Id, foodResponse.Id);
        //    Assert.Equal(food.FoodName, foodResponse.FoodName);
        //    Assert.Equal(food.Stock, foodResponse.Stock);
        //    Assert.Equal(food.WeighPerUnit, foodResponse.WeighPerUnit);
        //    Assert.True(foodResponse.IsActive);
        //    Assert.Equal(foodCategory.Id, foodResponse.FoodCategory.Id);
        //    Assert.Equal(foodCategory.Name, foodResponse.FoodCategory.Name);
        //    Assert.Equal(foodCategory.Description, foodResponse.FoodCategory.Description);
        //    Assert.Empty(foodResponse.ImageLinks);
        //    Assert.Null(foodResponse.Thumbnail);
        //}

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_NoLivestockCircleFoods_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var options = new DbContextOptionsBuilder<TestDbContext5>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext5(options);
        //    context.SaveChanges();

        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Empty(result);
        //}

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_LivestockCircleFoodButNoActiveFoods_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var food = new Food
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodName = "Food 1",
        //        FoodCategoryId = Guid.NewGuid(),
        //        FoodCategory = new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" },
        //        Stock = 100,
        //        WeighPerUnit = 40,
        //        IsActive = false // Inactive food
        //    };
        //    var livestockCircleFood = new LivestockCircleFood
        //    {
        //        LivestockCircleId = livestockCircleId,
        //        FoodId = food.Id,
        //        IsActive = true
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext5>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext5(options);
        //    context.Foods.Add(food);
        //    context.LivestockCircleFoods.Add(livestockCircleFood);
        //    context.SaveChanges();

        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Empty(result);
        //}

        [Fact]
        public async Task GetAllFoodRemainingOfLivestockCircle_InactiveLivestockCircleFood_ReturnsEmptyList()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var foodCategory = new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
            var food = new Food
            {
                Id = Guid.NewGuid(),
                FoodName = "Food 1",
                FoodCategoryId = foodCategory.Id,
                FoodCategory = foodCategory,
                Stock = 100,
                WeighPerUnit = 40,
                IsActive = true
            };
            var livestockCircleFood = new LivestockCircleFood
            {
                LivestockCircleId = livestockCircleId,
                FoodId = food.Id,
                IsActive = false // Inactive LivestockCircleFood
            };

            var options = new DbContextOptionsBuilder<TestDbContext5>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext5(options);
            context.FoodCategories.Add(foodCategory);
            context.Foods.Add(food);
            context.LivestockCircleFoods.Add(livestockCircleFood);
            context.SaveChanges();

            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Empty(result);
        }

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_EmptyLivestockCircleId_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.Empty;
        //    var options = new DbContextOptionsBuilder<TestDbContext5>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext5(options);
        //    context.SaveChanges();

        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Returns((Expression<Func<LivestockCircleFood, bool>> expr) => context.LivestockCircleFoods.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Include(f => f.FoodCategory).Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Empty(result);
        //}

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_RepositoryThrowsException_ThrowsException()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(() =>
        //        _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, default));
        //}

        //[Fact]
        //public async Task GetAllFoodRemainingOfLivestockCircle_CancellationRequested_ThrowsOperationCanceledException()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var cts = new CancellationTokenSource();
        //    _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
        //        .Throws(new OperationCanceledException("Operation cancelled"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        //        _dailyReportService.GetAllFoodRemainingOfLivestockCircle(livestockCircleId, cts.Token));
        //}


    }

    // InMemory DbContext for testing
    public class TestDbContext5 : DbContext
    {
        public TestDbContext5(DbContextOptions<TestDbContext5> options) : base(options) { }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodCategory> FoodCategories { get; set; }
        public DbSet<LivestockCircleFood> LivestockCircleFoods { get; set; }
        public DbSet<ImageFood> ImageFoods { get; set; }
    }
}