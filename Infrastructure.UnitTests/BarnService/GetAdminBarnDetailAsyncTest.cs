using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.User;
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
    public class GetAdminBarnDetailAsyncTest
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
        private readonly Guid _userId = Guid.Parse("3c9ef2d9-4b1a-4e4e-8f5e-9b2c8d1e7f3a");

        public GetAdminBarnDetailAsyncTest()
        {
            _barnRepositoryMock = new Mock<IRepository<Barn>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Setup HttpContext with user claims
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

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
        public async Task GetAdminBarnDetailAsync_UserNotLoggedIn_ReturnsError()
        {
            // Arrange: No user in HttpContext
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var service = new Infrastructure.Services.Implements.BarnService(
                _barnRepositoryMock.Object,
                _userRepositoryMock.Object,
                _livestockCircleRepositoryMock.Object,
                _imageLiveStockCircleRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _cloudinaryCloudServiceMock.Object);
            var barnId = Guid.NewGuid();

            // Act
            var result = await service.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_BarnNotFound_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var barnsMock = new List<Barn>().AsQueryable().BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_BarnInactive_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var barn = new Barn { Id = barnId, IsActive = false };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_WorkerNotFound_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = null // Worker is null
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_WorkerInactive_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = false };
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_TechnicalStaffNotFound_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Setup active livestock circle with inactive technical staff
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                TechicalStaffId = Guid.NewGuid(),
                IsActive = true,
                Status = StatusConstant.GROWINGSTAT
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Technical staff not found
            _userRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircle.TechicalStaffId, default))
                .ReturnsAsync((User)null);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("nhân viên kỹ thuật", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_TechnicalStaffInactive_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Setup active livestock circle with inactive technical staff
            var technicalStaffId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                TechicalStaffId = technicalStaffId,
                IsActive = true,
                Status = StatusConstant.GROWINGSTAT
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Technical staff inactive
            var technicalStaff = new User { Id = technicalStaffId, IsActive = false };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(technicalStaffId, default))
                .ReturnsAsync(technicalStaff);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("nhân viên kỹ thuật", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_BreedNotFound_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Setup active livestock circle
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                TechicalStaffId = technicalStaffId,
                BreedId = breedId,
                IsActive = true,
                Status = StatusConstant.GROWINGSTAT
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Technical staff active
            var technicalStaff = new User { Id = technicalStaffId, IsActive = true, FullName = "Tech Staff", Email = "tech@email.com", PhoneNumber = "123456789" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(technicalStaffId, default))
                .ReturnsAsync(technicalStaff);

            // Breed not found
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, default))
                .ReturnsAsync((Breed)null);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("giống", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_BreedInactive_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Setup active livestock circle
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barnId,
                TechicalStaffId = technicalStaffId,
                BreedId = breedId,
                IsActive = true,
                Status = StatusConstant.GROWINGSTAT
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Technical staff active
            var technicalStaff = new User { Id = technicalStaffId, IsActive = true, FullName = "Tech Staff", Email = "tech@email.com", PhoneNumber = "123456789" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(technicalStaffId, default))
                .ReturnsAsync(technicalStaff);

            // Breed inactive
            var breed = new Breed { Id = breedId, IsActive = false, BreedName = "Test Breed" };
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, default))
                .ReturnsAsync(breed);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("giống", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_Success_WithoutActiveLivestockCircle()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                BarnName = "Test Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // No active livestock circle
            var livestockCircles = new List<LivestockCircle>().AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy chi tiết chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(barnId, result.Data.Id);
            Assert.Equal(barn.BarnName, result.Data.BarnName);
            Assert.Equal(barn.Address, result.Data.Address);
            Assert.Equal(barn.Image, result.Data.Image);
            Assert.True(result.Data.IsActive);
            Assert.NotNull(result.Data.Worker);
            Assert.Equal(worker.Id, result.Data.Worker.Id);
            Assert.Equal(worker.FullName, result.Data.Worker.FullName);
            Assert.Equal(worker.Email, result.Data.Worker.Email);
            Assert.Null(result.Data.ActiveLivestockCircle);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_Success_WithActiveLivestockCircle()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                BarnName = "Test Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                IsActive = true,
                Worker = worker
            };
            var barns = new List<Barn> { barn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnsMock);

            // Setup active livestock circle
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                BarnId = barnId,
                TechicalStaffId = technicalStaffId,
                BreedId = breedId,
                LivestockCircleName = "Test Circle",
                Status = StatusConstant.GROWINGSTAT,
                StartDate = DateTime.UtcNow.AddDays(-30),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 2.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                IsActive = true
            };
            var livestockCircles = new List<LivestockCircle> { livestockCircle }.AsQueryable();
            var livestockCirclesMock = livestockCircles.BuildMock();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCirclesMock);

            // Technical staff active
            var technicalStaff = new User { Id = technicalStaffId, IsActive = true, FullName = "Tech Staff", Email = "tech@email.com", PhoneNumber = "123456789" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(technicalStaffId, default))
                .ReturnsAsync(technicalStaff);

            // Breed active
            var breed = new Breed { Id = breedId, IsActive = true, BreedName = "Test Breed" };
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, default))
                .ReturnsAsync(breed);

            // Breed images
            var breedImages = new List<ImageBreed>
            {
                new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId, ImageLink = "thumbnail.jpg", Thumnail = "true", IsActive = true },
                new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId, ImageLink = "image1.jpg", Thumnail = "false", IsActive = true }
            };
            var breedImagesMock = breedImages.AsQueryable().BuildMock();
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImagesMock);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy chi tiết chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(barnId, result.Data.Id);
            Assert.Equal(barn.BarnName, result.Data.BarnName);
            Assert.Equal(barn.Address, result.Data.Address);
            Assert.Equal(barn.Image, result.Data.Image);
            Assert.True(result.Data.IsActive);
            Assert.NotNull(result.Data.Worker);
            Assert.Equal(worker.Id, result.Data.Worker.Id);
            Assert.Equal(worker.FullName, result.Data.Worker.FullName);
            Assert.Equal(worker.Email, result.Data.Worker.Email);
            
            // Check ActiveLivestockCircle
            Assert.NotNull(result.Data.ActiveLivestockCircle);
            Assert.Equal(livestockCircleId, result.Data.ActiveLivestockCircle.Id);
            Assert.Equal(livestockCircle.LivestockCircleName, result.Data.ActiveLivestockCircle.LivestockCircleName);
            Assert.Equal(livestockCircle.Status, result.Data.ActiveLivestockCircle.Status);
            Assert.Equal(livestockCircle.StartDate, result.Data.ActiveLivestockCircle.StartDate);
            Assert.Equal(livestockCircle.TotalUnit, result.Data.ActiveLivestockCircle.TotalUnit);
            Assert.Equal(livestockCircle.DeadUnit, result.Data.ActiveLivestockCircle.DeadUnit);
            Assert.Equal(livestockCircle.AverageWeight, result.Data.ActiveLivestockCircle.AverageWeight);
            Assert.Equal(livestockCircle.GoodUnitNumber, result.Data.ActiveLivestockCircle.GoodUnitNumber);
            Assert.Equal(livestockCircle.BadUnitNumber, result.Data.ActiveLivestockCircle.BadUnitNumber);
            
            // Check Breed
            Assert.NotNull(result.Data.ActiveLivestockCircle.Breed);
            Assert.Equal(breedId, result.Data.ActiveLivestockCircle.Breed.Id);
            Assert.Equal(breed.BreedName, result.Data.ActiveLivestockCircle.Breed.BreedName);
            Assert.Equal("thumbnail.jpg", result.Data.ActiveLivestockCircle.Breed.Thumbnail);
            
            // Check Technical Staff
            Assert.NotNull(result.Data.ActiveLivestockCircle.TechicalStaff);
            Assert.Equal(technicalStaffId, result.Data.ActiveLivestockCircle.TechicalStaff.Id);
            Assert.Equal(technicalStaff.Email, result.Data.ActiveLivestockCircle.TechicalStaff.Email);
            Assert.Equal(technicalStaff.FullName, result.Data.ActiveLivestockCircle.TechicalStaff.Fullname);
            Assert.Equal(technicalStaff.PhoneNumber, result.Data.ActiveLivestockCircle.TechicalStaff.PhoneNumber);
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_Success_WithCancelledLivestockCircle()
        {
           
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                BarnName = "Test Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                IsActive = true,
                Worker = worker
            };

            
            var barnOptions = new DbContextOptionsBuilder<TestBarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var barnContext = new TestBarnDbContext(barnOptions);
            barnContext.Barns.Add(barn);
            barnContext.SaveChanges();

            
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnContext.Barns);

            
            var livestockCircleOptions = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var livestockCircleContext = new TestLivestockCircleDbContext(livestockCircleOptions);
            
            
            livestockCircleContext.LivestockCircles.AddRange(new List<LivestockCircle>());
            livestockCircleContext.SaveChanges();

            
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCircleContext.LivestockCircles);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.True(result.Succeeded, $"Expected success but got: {result.Message}");
            Assert.Equal("Lấy chi tiết chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(barnId, result.Data.Id);
            Assert.NotNull(result.Data.Worker);
            Assert.Null(result.Data.ActiveLivestockCircle); 
        }

        [Fact]
        public async Task GetAdminBarnDetailAsync_Success_WithDoneLivestockCircle()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn 
            { 
                Id = barnId, 
                BarnName = "Test Barn",
                Address = "Test Address",
                Image = "test-image.jpg",
                IsActive = true,
                Worker = worker
            };

            var barnOptions = new DbContextOptionsBuilder<TestBarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var barnContext = new TestBarnDbContext(barnOptions);
            barnContext.Barns.Add(barn);
            barnContext.SaveChanges();

          
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barnContext.Barns);

           
            var livestockCircleOptions = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var livestockCircleContext = new TestLivestockCircleDbContext(livestockCircleOptions);
            
            
            livestockCircleContext.LivestockCircles.AddRange(new List<LivestockCircle>());
            livestockCircleContext.SaveChanges();

            
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns(livestockCircleContext.LivestockCircles);

            // Act
            var result = await _barnService.GetAdminBarnDetailAsync(barnId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy chi tiết chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(barnId, result.Data.Id);
            Assert.NotNull(result.Data.Worker);
            Assert.Null(result.Data.ActiveLivestockCircle); // Should be null because status is DONE
        }
    }
}

// Add minimal InMemory DbContext for LivestockCircle test
public class TestLivestockCircleDbContext : DbContext
{
    public TestLivestockCircleDbContext(DbContextOptions<TestLivestockCircleDbContext> options) : base(options) { }
    public DbSet<LivestockCircle> LivestockCircles { get; set; }
}
