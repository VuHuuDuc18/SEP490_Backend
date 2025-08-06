using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class GetReleasedLivetockCircleListTest
    {
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _livestockCircleImageRepoMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly LivestockCircleService _livestockCircleService;

        public GetReleasedLivetockCircleListTest()
        {
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _livestockCircleImageRepoMock = new Mock<IRepository<ImageLivestockCircle>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _foodImageRepositoryMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());

            _livestockCircleService = new LivestockCircleService(
                _livestockCircleRepositoryMock.Object,
                _livestockCircleImageRepoMock.Object,
                _userRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _livestockCircleFoodRepositoryMock.Object,
                _livestockCircleMedicineRepositoryMock.Object,
                _foodRepositoryMock.Object,
                _medicineRepositoryMock.Object,
                _foodImageRepositoryMock.Object,
                _medicineImageRepositoryMock.Object,
                _imageLiveStockCircleRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object
            );
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_NullRequest_ReturnsError()
        {
            // Arrange
            ListingRequest request = null;

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null.", result.Message);
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0.", result.Message);
            //Assert.NotNull(result.Errors);
            //Assert.Single(result.Errors);
            //Assert.Equal("PageIndex và PageSize phải lớn hơn 0.", result.Errors[0]);
            //Assert.Null(result.Data);
            //_livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0.", result.Message);
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "Test" } }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Message);
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "Test" } }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Message);
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_InvalidSortField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ", result.Message);
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_ReturnsPaginatedData()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test",
                Image = "Test",
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
                LivestockCircleName = "Test Cycle",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Null(result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);

            var item = result.Data.Items.First();
            Assert.Equal(barnId, item.Id);
            Assert.Equal(livestockCircleId, item.LivestockCircleId);
            Assert.Equal("Test Barn", item.BarnName);
            Assert.Equal("Test Breed Category", item.BreedCategoryName);
            Assert.Equal("Test Breed", item.BreedName);
            Assert.Equal(100, item.TotalUnit);
            Assert.Equal(1, result.Data.PageIndex);
           // Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_EmptyResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            // No data added

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_WithSearchString()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Searchable Barn",
                Address = "Test",
                Image = "Test",
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
                LivestockCircleName = "Test Cycle",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "TotalUnit", Value = "100" } }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_WithFilter()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Filterable Barn",
                Address = "Test",
                Image = "Test",
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
                LivestockCircleName = "Test Cycle",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "TotalUnit", Value = "100" } }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_WithSorting()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test",
                Image = "Test",
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
                BarnId = barnId,
                Barn = barn,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Cycle 1",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
              //  StartDate = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            };

            var livestockCircle2 = new LivestockCircle
            {
                Id = livestockCircleId2,
                BarnId = barnId,
                Barn = barn,
                BreedId = breedId, // Fixed from barnId
                Breed = breed,
                LivestockCircleName = "Cycle 2",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 150,
              //  StartDate = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Id", Value = "desc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
           
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_WithMultipleItems()
        {
            // Arrange
            var barnId1 = Guid.NewGuid();
            var barnId2 = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            var barn1 = new Barn
            {
                Id = barnId1,
                BarnName = "Barn 1",
                Address = "Test",
                Image = "Test",
                IsActive = true
            };

            var barn2 = new Barn
            {
                Id = barnId2,
                BarnName = "Barn 2",
                Address = "Test",
                Image = "Test",
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
                LivestockCircleName = "Cycle 1",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                IsActive = true
            };

            var livestockCircle2 = new LivestockCircle
            {
                Id = livestockCircleId2,
                BarnId = barnId2,
                Barn = barn2,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Cycle 2",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 150,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
           
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_Success_PaginationAccuracy()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test",
                Image = "Test",
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
                BarnId = barnId,
                Barn = barn,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Cycle 1",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 100,
                IsActive = true
            };

            var livestockCircle2 = new LivestockCircle
            {
                Id = livestockCircleId2,
                BarnId = barnId,
                Barn = barn,
                BreedId = breedId,
                Breed = breed,
                LivestockCircleName = "Cycle 2",
                Status = StatusConstant.RELEASESTAT,
                TotalUnit = 150,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext1(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 2,
                PageSize = 1,
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetReleasedLivestockCircleList(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            
        }

        [Fact]
        public async Task GetReleasedLivestockCircleList_QueryException_ThrowsException()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "TotalUnit", Value = "asc" }
            };

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _livestockCircleService.GetReleasedLivestockCircleList(request));
            Assert.Equal("Lỗi khi lấy danh sách phân trang: Database error", exception.Message);
            _livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Once());
        }
    }

    // In-memory DbContext for testing
    public class TestLivestockCircleDbContext1 : DbContext
    {
        public TestLivestockCircleDbContext1(DbContextOptions<TestLivestockCircleDbContext1> options) : base(options) { }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<Barn> Barns { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<BreedCategory> BreedCategories { get; set; }
    }
}