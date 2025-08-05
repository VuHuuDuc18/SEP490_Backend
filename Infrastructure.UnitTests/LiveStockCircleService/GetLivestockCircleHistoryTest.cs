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
using static System.Runtime.InteropServices.JavaScript.JSType;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class GetLivestockCircleHistoryTest
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

        public GetLivestockCircleHistoryTest()
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
        public async Task GetLivestockCircleHistory_NullRequest_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            ListingRequest request = null;

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null.", result.Message);
            //Assert.NotNull(result.Errors);
            //Assert.Single(result.Errors);
            //Assert.Equal("Yêu cầu không được null.", result.Errors[0]);
            //Assert.Null(result.Data);
            //_livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetLivestockCircleHistory_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

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
        public async Task GetLivestockCircleHistory_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

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
        public async Task GetLivestockCircleHistory_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField",Value = "Test" } }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Message);
            //Assert.NotNull(result.Errors);
            //Assert.Single(result.Errors);
            //Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Errors[0]);
            //Assert.Null(result.Data);
            //_livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetLivestockCircleHistory_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "Test" } }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Message);
            //Assert.NotNull(result.Errors);
            //Assert.Single(result.Errors);
            //Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Errors[0]);
            //Assert.Null(result.Data);
            //_livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetLivestockCircleHistory_InvalidSortField_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ", result.Message);
            //Assert.NotNull(result.Errors);
            //Assert.Single(result.Errors);
            //Assert.Equal("Trường lọc không hợp lệ", result.Errors[0]);
            //Assert.Null(result.Data);
            //_livestockCircleRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetLivestockCircleHistory_Success_ReturnsPaginatedData()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address ="Test",
                Image = "Tets",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
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
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "StartDate", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
           // Assert.Equal("Lấy dữ liệu thành công.", result.Message);
           // Assert.NotNull(result.Data);
           // Assert.Equal(1, result.Data.Items.Count);

           // var item = result.Data.Items.First();
           // Assert.Equal(livestockCircleId, item.Id);
           // Assert.Equal("Test Cycle", item.LivestockCircleName);
           // Assert.Equal(StatusConstant.GROWINGSTAT, item.Status);
           // Assert.Equal(100, item.TotalUnit);
           // Assert.Equal(5, item.DeadUnit);
           // Assert.Equal(2.5f, item.AverageWeight);
           // Assert.Equal("Test Breed", item.BreedName);
           // Assert.Equal(breedId, item.BreedId);
           // Assert.Equal(1, result.Data.PageIndex);
           //// Assert.Equal(10, result.Data.PageSize);
           // Assert.Equal(1, result.Data.TotalCount);
           // Assert.Equal(1, result.Data.TotalPages);
        }
        
        [Fact]
        public async Task GetLivestockCircleHistory_Success_WithSearchString()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Searchable Barn",
                Address = "Test",
                Image = "Tets",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
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
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "StartDate", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "LivestockCircleName", Value = "Test Cycle" } }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            //Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            //Assert.NotNull(result.Data);
            //Assert.Equal(1, result.Data.Items.Count);
            //Assert.Equal("Test Cycle", result.Data.Items.First().LivestockCircleName);
          //  Assert.Equal("Searchable Barn", result.Data.Items.First().BarnName);
        }

        [Fact]
        public async Task GetLivestockCircleHistory_Success_WithFilter()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Filterable Barn",
                Address = "Test",
                Image = "Tets",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
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
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "StartDate", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "LivestockCircleName", Value = "Test Cycle" } }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            //Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            //Assert.NotNull(result.Data);
            //Assert.Equal(1, result.Data.Items.Count);
            //Assert.Equal("Test Cycle", result.Data.Items.First().LivestockCircleName);
           // Assert.Equal("Filterable Barn", result.Data.Items.First().BarnName);
        }

        [Fact]
        public async Task GetLivestockCircleHistory_Success_WithSorting()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test",
                Image = "Tets",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
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
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
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
                DeadUnit = 10,
                AverageWeight = 3.0f,
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "StartDate", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            //Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            //Assert.NotNull(result.Data);
            //Assert.Equal(2, result.Data.Items.Count);
            //Assert.Equal("Cycle 2", result.Data.Items.First().LivestockCircleName); // Latest StartDate first
            //Assert.Equal("Cycle 1", result.Data.Items.Last().LivestockCircleName);
        }

        

        [Fact]
        public async Task GetLivestockCircleHistory_Success_PaginationAccuracy()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Test Barn",
                Address = "Test",
                Image = "Tets",
                IsActive = true
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
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
                Status = StatusConstant.GROWINGSTAT,
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
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
                DeadUnit = 10,
                AverageWeight = 3.0f,
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext(options);
            context.LivestockCircles.AddRange(livestockCircle1, livestockCircle2);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(context.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 2,
                PageSize = 1,
                Sort = new SearchObjectForCondition { Field = "StartDate", Value = "asc" }
            };

            // Act
            var result = await _livestockCircleService.GetLivestockCircleHistory(barnId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
          //  Assert.Equal("Lấy dữ liệu thành công.", result.Message);
          //  Assert.NotNull(result.Data);
          //  Assert.Equal(1, result.Data.Items.Count);
          //  Assert.Equal("Cycle 2", result.Data.Items.First().LivestockCircleName); // Second item in ascending StartDate order
          //  Assert.Equal(2, result.Data.PageIndex);
          ////  Assert.Equal(1, result.Data.PageSize);
          //  Assert.Equal(2, result.Data.TotalCount);
          //  Assert.Equal(2, result.Data.TotalPages);
        }

       
    }

    // In-memory DbContext for testing
    public class TestLivestockCircleDbContext : DbContext
    {
        public TestLivestockCircleDbContext(DbContextOptions<TestLivestockCircleDbContext> options) : base(options) { }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<Barn> Barns { get; set; }
        public DbSet<Breed> Breeds { get; set; }
    }
}