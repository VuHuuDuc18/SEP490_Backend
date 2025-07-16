using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.Moq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.BarnService
{
    public class GetBarnByWorkerTest
    {
        private readonly Mock<IRepository<Barn>> _barnRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<Infrastructure.Services.CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BarnService _barnService;
        private readonly Guid _userId = Guid.Parse("3c9ef2d9-4b1a-4e4e-8f5e-9b2c8d1e7f3a");

        public GetBarnByWorkerTest()
        {
            _barnRepositoryMock = new Mock<IRepository<Barn>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _cloudinaryCloudServiceMock = new Mock<Infrastructure.Services.CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
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
        public async Task GetBarnByWorker_UserNotLoggedIn_ReturnsError()
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
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };

            // Act
            var result = await service.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_WorkerNotFoundOrInactive_ReturnsError()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync((User)null);
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_WorkerInactive_ReturnsError()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = false };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_RequestNull_ReturnsError()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = true };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);

            // Act
            var result = await _barnService.GetBarnByWorker(null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("không được null", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_InvalidPageIndexOrSize_ReturnsError()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = true };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);
            var request = new ListingRequest { PageIndex = 0, PageSize = 0 };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = true };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_InvalidSortField_ReturnsError()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = true };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnByWorker_Success_ReturnsPaginatedBarns()
        {
            // Arrange
            var worker = new User { Id = _userId, IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(_userId, default)).ReturnsAsync(worker);
            var barnsList = new List<Barn>
            {
                new Barn
                {
                    Id = Guid.NewGuid(),
                    BarnName = "Barn 1",
                    Address = "Address 1",
                    Image = "image1.jpg",
                    WorkerId = _userId,
                    Worker = worker,
                    IsActive = true
                },
                new Barn
                {
                    Id = Guid.NewGuid(),
                    BarnName = "Barn 2",
                    Address = "Address 2",
                    Image = "image2.jpg",
                    WorkerId = _userId,
                    Worker = worker,
                    IsActive = true
                }
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestBarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestBarnDbContext(options);
            context.Barns.AddRange(barnsList);
            context.SaveChanges();

            // Mock repository to return DbSet from InMemory context
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>())).Returns(context.Barns);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetBarnByWorker(request);

            Console.WriteLine("Service message: " + result.Message); // Debug

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách phân trang chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.All(result.Data.Items, item =>
            {
                Assert.NotEqual(Guid.Empty, item.Id);
                Assert.False(string.IsNullOrEmpty(item.BarnName));
                Assert.False(string.IsNullOrEmpty(item.Address));
                Assert.False(string.IsNullOrEmpty(item.Image));
                Assert.NotNull(item.Worker);
                Assert.Equal(worker.Id, item.Worker.Id);
                Assert.Equal(worker.FullName, item.Worker.FullName);
                Assert.Equal(worker.Email, item.Worker.Email);
                Assert.True(item.IsActive);
            });
        }
    }
}

public static class PaginationTestExtension
{
    public static Task<PaginationSet<T>> Pagination<T>(this IQueryable<T> query, int pageIndex, int pageSize, object sort)
    {
        var items = query.ToList();
        return Task.FromResult(new PaginationSet<T>
        {
            PageIndex = pageIndex,
            Count = items.Count,
            TotalCount = items.Count,
            TotalPages = 1,
            Items = items
        });
    }
}

// Add minimal InMemory DbContext for test
public class TestBarnDbContext : DbContext
{
    public TestBarnDbContext(DbContextOptions<TestBarnDbContext> options) : base(options) { }
    public DbSet<Barn> Barns { get; set; }
}
