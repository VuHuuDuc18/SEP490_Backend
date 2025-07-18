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
using MockQueryable;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnService
{
    public class GetAssignedBarnTest
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

        public GetAssignedBarnTest()
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

        //[Fact]
        //public async Task GetAssignedBarn_RequestNull_ReturnsError()
        //{
        //    // Arrange
        //    var technicalStaffId = Guid.NewGuid();

        //    // Act
        //    var result = await _barnService.GetAssignedBarn(technicalStaffId, null);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("không được null", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        [Fact]
        public async Task GetAssignedBarn_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAssignedBarn_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("PageSize", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAssignedBarn_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
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
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAssignedBarn_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition>
                {
                    new SearchObjectForCondition { Field = "InvalidField", Value = "test" }
                }
            };

            // Act
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAssignedBarn_InvalidSortField_ReturnsError()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAssignedBarn_Success_ReturnsPaginatedData()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var barnId1 = Guid.NewGuid();
            var barnId2 = Guid.NewGuid();
            var workerId1 = Guid.NewGuid();
            var workerId2 = Guid.NewGuid();

            // Create test data
            var worker1 = new User
            {
                Id = workerId1,
                FullName = "Worker 1",
                Email = "worker1@email.com",
                IsActive = true
            };

            var worker2 = new User
            {
                Id = workerId2,
                FullName = "Worker 2",
                Email = "worker2@email.com",
                IsActive = true
            };

            var barn1 = new Barn
            {
                Id = barnId1,
                BarnName = "Barn 1",
                Address = "Address 1",
                Image = "image1.jpg",
                WorkerId = workerId1,
                Worker = worker1,
                IsActive = true
            };

            var barn2 = new Barn
            {
                Id = barnId2,
                BarnName = "Barn 2",
                Address = "Address 2",
                Image = "image2.jpg",
                WorkerId = workerId2,
                Worker = worker2,
                IsActive = true
            };

            var livestockCircle1 = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId1,
                LivestockCircleName = "Test1",
                Status = StatusConstant.PENDINGSTAT,
                Barn = barn1,
                TechicalStaffId = technicalStaffId,
                IsActive = true
            };

            var livestockCircle2 = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                LivestockCircleName = "Test2",
                Status = StatusConstant.PENDINGSTAT,
                BarnId = barnId2,
                Barn = barn2,
                TechicalStaffId = technicalStaffId,
                IsActive = true
            };

            // Setup InMemory DbContext for LivestockCircle
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext3(options);
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
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            
            var items = result.Data.Items.OrderBy(x => x.BarnName).ToList();
            Assert.Equal("Barn 1", items[0].BarnName);
            Assert.Equal("Barn 2", items[1].BarnName);
            Assert.Equal(barnId1, items[0].Id);
            Assert.Equal(barnId2, items[1].Id);
            Assert.Equal("Address 1", items[0].Address);
            Assert.Equal("Address 2", items[1].Address);
            Assert.Equal("image1.jpg", items[0].Image);
            Assert.Equal("image2.jpg", items[1].Image);
            Assert.True(items[0].IsActive);
            Assert.True(items[1].IsActive);
            
            // Verify worker data
            Assert.NotNull(items[0].Worker);
            Assert.Equal(workerId1, items[0].Worker.Id);
            Assert.Equal("Worker 1", items[0].Worker.FullName);
            Assert.Equal("worker1@email.com", items[0].Worker.Email);
            
            Assert.NotNull(items[1].Worker);
            Assert.Equal(workerId2, items[1].Worker.Id);
            Assert.Equal("Worker 2", items[1].Worker.FullName);
            Assert.Equal("worker2@email.com", items[1].Worker.Email);
        }

        [Fact]
        public async Task GetAssignedBarn_Success_WithSearchString()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var worker = new User
            {
                Id = workerId,
                FullName = "Worker 1",
                Email = "worker1@email.com",
                IsActive = true
            };

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Searchable Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                WorkerId = workerId,
                Worker = worker,
                IsActive = true
            };

            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                LivestockCircleName = "Test",
                Status = StatusConstant.PENDINGSTAT,
                Barn = barn,
                TechicalStaffId = technicalStaffId,
                IsActive = true
            };

            // Setup InMemory DbContext for LivestockCircle
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext3(options);
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
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "BarnName", Value = "Searchable" } }
            };

            // Act
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal("Searchable Barn", result.Data.Items.First().BarnName);
        }

        [Fact]
        public async Task GetAssignedBarn_Success_WithFilter()
        {
            // Arrange
            var technicalStaffId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var workerId = Guid.NewGuid();

            var worker = new User
            {
                Id = workerId,
                FullName = "Worker 1",
                Email = "worker1@email.com",
                IsActive = true
            };

            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Filterable Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                WorkerId = workerId,
                Worker = worker,
                IsActive = true
            };

            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                LivestockCircleName = "Test",
                Status = StatusConstant.PENDINGSTAT,
                Barn = barn,
                TechicalStaffId = technicalStaffId,
                IsActive = true
            };

            // Setup InMemory DbContext for LivestockCircle
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestLivestockCircleDbContext3(options);
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
            var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy dữ liệu thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal("Filterable Barn", result.Data.Items.First().BarnName);
        }

        //[Fact]
        //public async Task GetAssignedBarn_Success_EmptyResult()
        //{
        //    // Arrange
        //    var technicalStaffId = Guid.NewGuid();

        //    // Setup InMemory DbContext for LivestockCircle with no data
        //    var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext3>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestLivestockCircleDbContext3(options);
        //    // No livestock circles added
        //    context.SaveChanges();

        //    // Mock repository to return DbSet from InMemory context
        //    _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
        //        .Returns(context.LivestockCircles);

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
        //    };

        //    // Act
        //    var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy dữ liệu thành công.", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(0, result.Data.Items.Count);
        //}

        //[Fact]
        //public async Task GetAssignedBarn_Success_OnlyAssignedToTechnicalStaff()
        //{
        //    // Arrange
        //    var technicalStaffId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557");
        //    var otherTechnicalStaffId = Guid.Parse("E1D40884-FF30-407A-8934-0A580D2BD57A");
        //    var barnId1 = Guid.NewGuid();
        //    var barnId2 = Guid.NewGuid();
        //    var workerId1 = Guid.NewGuid();
        //    var workerId2 = Guid.NewGuid();

        //    var worker1 = new User
        //    {
        //        Id = workerId1,
        //        FullName = "Worker 1",
        //        Email = "worker1@email.com",
        //        IsActive = true
        //    };

        //    var worker2 = new User
        //    {
        //        Id = workerId2,
        //        FullName = "Worker 2",
        //        Email = "worker2@email.com",
        //        IsActive = true
        //    };

        //    var barn1 = new Barn
        //    {
        //        Id = barnId1,
        //        BarnName = "Test Barn 1",
        //        Address = "Test Address 1",
        //        Image = "test-image1.jpg",
        //        WorkerId = workerId1,
        //        Worker = worker1,
        //        IsActive = true
        //    };

        //    var barn2 = new Barn
        //    {
        //        Id = barnId2,
        //        BarnName = "Test Barn 2",
        //        Address = "Test Address 2",
        //        Image = "test-image2.jpg",
        //        WorkerId = workerId2,
        //        Worker = worker2,
        //        IsActive = true
        //    };

        //    // Create livestock circle assigned to the specific technical staff
        //    var assignedLivestockCircle = new LivestockCircle
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleName = "Test1",
        //        Status = StatusConstant.PENDINGSTAT,
        //        BarnId = barnId1,
        //        Barn = barn1,
        //        TechicalStaffId = technicalStaffId,
        //        IsActive = true
        //    };

        //    // Create livestock circle assigned to different technical staff (should not be returned)
        //    var unassignedLivestockCircle = new LivestockCircle
        //    {
        //        Id = Guid.NewGuid(),
        //        BarnId = barnId2,
        //        LivestockCircleName = "Test2",
        //        Status = StatusConstant.PENDINGSTAT,
        //        Barn = barn2,
        //        TechicalStaffId = otherTechnicalStaffId,
        //        IsActive = true
        //    };

        //    // Setup InMemory DbContext for LivestockCircle
        //    var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext3>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestLivestockCircleDbContext3(options);
        //    context.LivestockCircles.AddRange(assignedLivestockCircle, unassignedLivestockCircle);
        //    context.SaveChanges();

        //    // Mock repository to return DbSet from InMemory context
        //    _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
        //        .Returns(context.LivestockCircles);

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
        //    };

        //    // Act
        //    var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy dữ liệu thành công.", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(1, result.Data.Items.Count);
        //    Assert.Equal("Test Barn 1", result.Data.Items.First().BarnName);
        //    Assert.Equal(barnId1, result.Data.Items.First().Id);
        //    Assert.Equal(workerId1, result.Data.Items.First().Worker.Id);
        //}

        //[Fact]
        //public async Task GetAssignedBarn_ExceptionOccurs_ReturnsError()
        //{
        //    // Arrange
        //    var technicalStaffId = Guid.NewGuid();
        //    var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
        //        .Throws(new Exception("Database connection error"));

        //    // Act
        //    var result = await _barnService.GetAssignedBarn(technicalStaffId, request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi khi lấy danh sách phân trang", result.Message, StringComparison.OrdinalIgnoreCase);
        //    Assert.Contains("Database connection error", result.Message);
        //}
    }
}

// Add minimal InMemory DbContext for LivestockCircle test
public class TestLivestockCircleDbContext3 : DbContext
{
    public TestLivestockCircleDbContext3(DbContextOptions<TestLivestockCircleDbContext3> options) : base(options) { }
    public DbSet<LivestockCircle> LivestockCircles { get; set; }
}
