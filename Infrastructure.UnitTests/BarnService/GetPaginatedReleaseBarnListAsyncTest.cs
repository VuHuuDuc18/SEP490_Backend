using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnService
{
    public class GetPaginatedReleaseBarnListAsyncTest
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

        public GetPaginatedReleaseBarnListAsyncTest()
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
        public async Task GetPaginatedReleaseBarnListAsync_RequestNull_ReturnsError()
        {
            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("không được null", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("PageSize", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> 
                { 
                    new SearchObjectForCondition { Field = "InvalidField", Value = "test" } 
                }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_InvalidSortField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_Success_ReturnsPaginatedData()
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
                Status = StatusConstant.GROWINGSTAT,
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
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            
            var item = result.Data.Items.First();
            Assert.Equal(barnId, item.Id);
            Assert.Equal("Test Barn", item.BarnName);
            Assert.Equal("Test Address", item.Address);
            Assert.Equal("test-image.jpg", item.Image);
            Assert.Equal(100, item.TotalUnit);
            Assert.Equal(5, item.DeadUnit);
            Assert.Equal(90, item.GoodUnitNumber);
            Assert.Equal(5, item.BadUnitNumber);
            Assert.Equal(2.5f, item.AverageWeight);
            Assert.Equal("Test Breed Category", item.BreedCategory);
            Assert.Equal("Test Breed", item.Breed);
            Assert.NotNull(item.StartDate);
            Assert.NotNull(item.ReleaseDate);
            Assert.True(item.Age >= 0); // Age should be calculated correctly
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_Success_WithSearchString()
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
                BarnName = "Searchable Barn",
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
                Status = StatusConstant.GROWINGSTAT,
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
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "BarnName",  Value = "Searchable" } }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal("Searchable Barn", result.Data.Items.First().BarnName);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_Success_WithFilter()
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
                BarnName = "Filterable Barn",
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
                Status = StatusConstant.GROWINGSTAT,
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
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "BarnName", Value = "Filterable Barn" } }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal("Filterable Barn", result.Data.Items.First().BarnName);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_Success_EmptyResult()
        {
            // Arrange
            // Setup InMemory DbContext for LivestockCircle with no data
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            // No livestock circles added
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.Data.Items.Count);
        }

        [Fact]
        public async Task GetPaginatedReleaseBarnListAsync_Success_WithMultipleItems()
        {
            // Arrange
            var barnId1 = Guid.NewGuid();
            var barnId2 = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            // Create test data
            var barn1 = new Barn
            {
                Id = barnId1,
                BarnName = "Barn 1",
                Address = "Address 1",
                Image = "image1.jpg",
                IsActive = true
            };

            var barn2 = new Barn
            {
                Id = barnId2,
                BarnName = "Barn 2",
                Address = "Address 2",
                Image = "image2.jpg",
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

            var livestockCircle1 = new LivestockCircle
            {
                Id = livestockCircleId1,
                BarnId = barnId1,
                Barn = barn1,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Livestock Circle 1",
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                ReleaseDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true
            };

            var livestockCircle2 = new LivestockCircle
            {
                Id = livestockCircleId2,
                BarnId = barnId2,
                Barn = barn2,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Livestock Circle 2",
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 150,
                DeadUnit = 10,
                GoodUnitNumber = 130,
                BadUnitNumber = 10,
                AverageWeight = 3.0f,
                StartDate = DateTime.UtcNow.AddDays(-45),
                ReleaseDate = DateTime.UtcNow.AddDays(-10),
                IsActive = true
            };

            // Setup InMemory DbContext for LivestockCircle
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetPaginatedReleaseBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            
            var items = result.Data.Items.OrderBy(x => x.BarnName).ToList();
            Assert.Equal("Barn 1", items[0].BarnName);
            Assert.Equal("Barn 2", items[1].BarnName);
            Assert.Equal(100, items[0].TotalUnit);
            Assert.Equal(150, items[1].TotalUnit);
        }
    }
}

// Add minimal InMemory DbContext for LivestockCircle test
public class TestLivestockCircleDbContext1 : DbContext
{
    public TestLivestockCircleDbContext1(DbContextOptions<TestLivestockCircleDbContext1> options) : base(options) { }
    public DbSet<LivestockCircle> LivestockCircles { get; set; }
}
