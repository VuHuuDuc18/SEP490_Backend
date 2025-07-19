using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.LivestockCircle;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnService
{
    public class GetReleaseBarnDetailTest
    {
        private readonly Mock<IRepository<Barn>> _barnRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BarnService _barnService;

        public GetReleaseBarnDetailTest()
        {
            _barnRepositoryMock = new Mock<IRepository<Barn>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _barnService = new Infrastructure.Services.Implements.BarnService(
                _barnRepositoryMock.Object,
                _userRepositoryMock.Object,
                _livestockCircleRepositoryMock.Object,
                _imageLiveStockCircleRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _cloudinaryCloudServiceMock.Object);
        }

        [Fact]
        public async Task GetReleaseBarnDetail_LivestockCircleNotFound_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var livestockCircles = new List<LivestockCircle>().AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Act
            var result = await _barnService.GetReleaseBarnDetail(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Không tìm thấy thông tin chuồng nuôi.", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetReleaseBarnDetail_LivestockCircleNotReleaseStatus_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                Status = StatusConstant.GROWINGSTAT, // Not RELEASE status
                IsActive = true
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Act
            var result = await _barnService.GetReleaseBarnDetail(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Không thế lấy thông tin chuồng nuôi.", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetReleaseBarnDetail_LivestockCircleInactive_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                Status = StatusConstant.RELEASESTAT,
                IsActive = false // Inactive
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Act
            var result = await _barnService.GetReleaseBarnDetail(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Không thế lấy thông tin chuồng nuôi.", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetReleaseBarnDetail_Success_ReturnsCompleteData()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            // Create test data
            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                IsActive = true
            };

            var breedCategory = new BreedCategory
            {
                Id = breedCategoryId,
                Name = "Test Breed Category",
                Description = "Test",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
                BreedCategoryId = breedCategoryId,
                BreedCategory = breedCategory,
                IsActive = true
            };

            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                BarnId = barnId,
                Barn = barn,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Test Livestock Circle",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                ReleaseDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true
            };

            // Setup InMemory DbContext for LivestockCircle
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext2(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            // Setup livestock circle images
            var circleImages = new List<ImageLivestockCircle>
            {
                new ImageLivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    ImageLink = "circle-image1.jpg",
                    Thumnail = "false",
                    IsActive = true
                },
                new ImageLivestockCircle
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    ImageLink = "circle-image2.jpg",
                    Thumnail = "true",
                    IsActive = true
                }
            };

            var circleImagesQueryable = circleImages.AsQueryable();
            var circleImagesMock = circleImagesQueryable.BuildMock();
            _imageLiveStockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageLivestockCircle, bool>>>()))
                .Returns(circleImagesMock);

            // Setup breed images
            var breedImages = new List<ImageBreed>
            {
                new ImageBreed
                {
                    Id = Guid.NewGuid(),
                    BreedId = breedId,
                    ImageLink = "breed-image1.jpg",
                    Thumnail = "false",
                    IsActive = true
                },
                new ImageBreed
                {
                    Id = Guid.NewGuid(),
                    BreedId = breedId,
                    ImageLink = "breed-image2.jpg",
                    Thumnail = "true",
                    IsActive = true
                }
            };

            var breedImagesQueryable = breedImages.AsQueryable();
            var breedImagesMock = breedImagesQueryable.BuildMock();
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImagesMock);

            // Act
            var result = await _barnService.GetReleaseBarnDetail(barnId);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy thông tin thành công.", result.Message);
            Assert.NotNull(result.Data);
            
            // Verify barn data
            Assert.Equal(barnId, result.Data.Id);
            Assert.Equal("Test Barn", result.Data.BarnName);
            Assert.Equal("Test Address", result.Data.Address);
            Assert.Equal("test-image.jpg", result.Data.Image);
            
            // Verify livestock circle data
            Assert.NotNull(result.Data.LivestockCircle);
            Assert.Equal(livestockCircleId, result.Data.LivestockCircle.Id);
            Assert.Equal("Test Livestock Circle", result.Data.LivestockCircle.LivestockCircleName);
            Assert.Equal(StatusConstant.RELEASESTAT, result.Data.LivestockCircle.Status);
            Assert.Equal(100, result.Data.LivestockCircle.TotalUnit);
            Assert.Equal(5, result.Data.LivestockCircle.DeadUnit);
            Assert.Equal(90, result.Data.LivestockCircle.GoodUnitNumber);
            Assert.Equal(5, result.Data.LivestockCircle.BadUnitNumber);
            Assert.Equal(2.5f, result.Data.LivestockCircle.AverageWeight);
            Assert.NotNull(result.Data.LivestockCircle.StartDate);
            Assert.NotNull(result.Data.LivestockCircle.ReleaseDate);
            Assert.NotNull(result.Data.LivestockCircle.Images);
            Assert.Equal(2, result.Data.LivestockCircle.Images.Count);
            
            // Verify breed data
            Assert.NotNull(result.Data.Breed);
            Assert.Equal(breedId, result.Data.Breed.Id);
            Assert.Equal("Test Breed", result.Data.Breed.BreedName);
            Assert.NotNull(result.Data.Breed.BreedCategory);
            Assert.Equal(breedCategoryId, result.Data.Breed.BreedCategory.Id);
            Assert.Equal("Test Breed Category", result.Data.Breed.BreedCategory.Name);
            Assert.NotNull(result.Data.Breed.Thumbnail);
            Assert.NotNull(result.Data.Breed.ImageLinks);
            Assert.Equal(2, result.Data.Breed.ImageLinks.Count);
        }

        //[Fact]
        //public async Task GetReleaseBarnDetail_Success_WithNoImages_ReturnsDataWithoutImages()
        //{
        //    // Arrange
        //    var barnId = Guid.NewGuid();
        //    var breedId = Guid.NewGuid();
        //    var breedCategoryId = Guid.NewGuid();
        //    var livestockCircleId = Guid.NewGuid();

        //    // Create test data
        //    var barn = new Barn
        //    {
        //        Id = barnId,
        //        BarnName = "Test Barn",
        //        Address = "Test Address",
        //        Image = "test-image.jpg",
        //        IsActive = true
        //    };

        //    var breedCategory = new BreedCategory
        //    {
        //        Id = breedCategoryId,
        //        Name = "Test Breed Category",
        //        Description = "Test",
        //        IsActive = true
        //    };

        //    var breed = new Breed
        //    {
        //        Id = breedId,
        //        BreedName = "Test Breed",
        //        BreedCategoryId = breedCategoryId,
        //        BreedCategory = breedCategory,
        //        IsActive = true
        //    };

        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        BarnId = barnId,
        //        Barn = barn,
        //        BreedId = breedId,
        //        Breed = breed,
        //        LivestockCircleName = "Test Livestock Circle",
        //        Status = StatusConstant.RELEASESTAT,
        //        TotalUnit = 100,
        //        DeadUnit = 5,
        //        GoodUnitNumber = 90,
        //        BadUnitNumber = 5,
        //        AverageWeight = 2.5f,
        //        StartDate = DateTime.UtcNow.AddDays(-30),
        //        ReleaseDate = DateTime.UtcNow.AddDays(-5),
        //        IsActive = true
        //    };

        //    // Setup InMemory DbContext for LivestockCircle
        //    var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext2>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestLivestockCircleDbContext2(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.SaveChanges();

        //    // Mock repository to return DbSet from InMemory context
        //    _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
        //        .Returns(context.LivestockCircles);

        //    // Setup empty livestock circle images
        //    var circleImages = new List<ImageLivestockCircle>();
        //    var circleImagesQueryable = circleImages.AsQueryable();
        //    var circleImagesMock = circleImagesQueryable.BuildMock();
        //    _imageLiveStockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageLivestockCircle, bool>>>()))
        //        .Returns(circleImagesMock);

        //    // Setup empty breed images
        //    var breedImages = new List<ImageBreed>();
        //    var breedImagesQueryable = breedImages.AsQueryable();
        //    var breedImagesMock = breedImagesQueryable.BuildMock();
        //    _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
        //        .Returns(breedImagesMock);

        //    // Act
        //    var result = await _barnService.GetReleaseBarnDetail(barnId);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy thông tin thành công.", result.Message);
        //    Assert.NotNull(result.Data);
            
        //    // Verify barn data
        //    Assert.Equal(barnId, result.Data.Id);
        //    Assert.Equal("Test Barn", result.Data.BarnName);
        //    Assert.Equal("Test Address", result.Data.Address);
        //    Assert.Equal("test-image.jpg", result.Data.Image);
            
        //    // Verify livestock circle data
        //    Assert.NotNull(result.Data.LiveStockCircle);
        //    Assert.Equal(livestockCircleId, result.Data.LiveStockCircle.Id);
        //    Assert.Equal("Test Livestock Circle", result.Data.LiveStockCircle.LivestockCircleName);
        //    Assert.Equal(StatusConstant.RELEASESTAT, result.Data.LiveStockCircle.Status);
        //    Assert.Equal(100, result.Data.LiveStockCircle.TotalUnit);
        //    Assert.Equal(5, result.Data.LiveStockCircle.DeadUnit);
        //    Assert.Equal(90, result.Data.LiveStockCircle.GoodUnitNumber);
        //    Assert.Equal(5, result.Data.LiveStockCircle.BadUnitNumber);
        //    Assert.Equal(2.5f, result.Data.LiveStockCircle.AverageWeight);
        //    Assert.NotNull(result.Data.LiveStockCircle.StartDate);
        //    Assert.NotNull(result.Data.LiveStockCircle.ReleaseDate);
        //    Assert.NotNull(result.Data.LiveStockCircle.Images);
        //    Assert.Equal(0, result.Data.LiveStockCircle.Images.Count);
            
        //    // Verify breed data
        //    Assert.NotNull(result.Data.Breed);
        //    Assert.Equal(breedId, result.Data.Breed.Id);
        //    Assert.Equal("Test Breed", result.Data.Breed.BreedName);
        //    Assert.NotNull(result.Data.Breed.BreedCategory);
        //    Assert.Equal(breedCategoryId, result.Data.Breed.BreedCategory.Id);
        //    Assert.Equal("Test Breed Category", result.Data.Breed.BreedCategory.Name);
        //    Assert.Null(result.Data.Breed.Thumbnail);
        //    Assert.NotNull(result.Data.Breed.ImageLinks);
        //    Assert.Equal(0, result.Data.Breed.ImageLinks.Count);
        //}

        //[Fact]
        //public async Task GetReleaseBarnDetail_ExceptionOccurs_ReturnsError()
        //{
        //    // Arrange
        //    var barnId = Guid.NewGuid();
        //    _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
        //        .Throws(new Exception("Database connection error"));

        //    // Act
        //    var result = await _barnService.GetReleaseBarnDetail(barnId);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Không thế lấy thông tin chuồng nuôi", result.Message, StringComparison.OrdinalIgnoreCase);
        //    Assert.NotNull(result.Errors);
        //    Assert.Contains("Database connection error", result.Errors[0]);
        //}
    }
}

// Add minimal InMemory DbContext for LivestockCircle test
public class TestLivestockCircleDbContext2 : DbContext
{
    public TestLivestockCircleDbContext2(DbContextOptions<TestLivestockCircleDbContext2> options) : base(options) { }
    public DbSet<LivestockCircle> LivestockCircles { get; set; }
}
