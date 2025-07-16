using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnService
{
    public class GetPaginatedAdminBarnListAsyncTest
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

        public GetPaginatedAdminBarnListAsyncTest()
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
        public async Task GetPaginatedAdminBarnListAsync_UserNotLoggedIn_ReturnsError()
        {
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
            var result = await service.GetPaginatedAdminBarnListAsync(request);
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedAdminBarnListAsync_RequestNull_ReturnsError()
        {
            var result = await _barnService.GetPaginatedAdminBarnListAsync(null);
            Assert.False(result.Succeeded);
            Assert.Contains("không được null", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedAdminBarnListAsync_InvalidPageIndexOrSize_ReturnsError()
        {
            var request = new ListingRequest { PageIndex = 0, PageSize = 0 };
            var result = await _barnService.GetPaginatedAdminBarnListAsync(request);
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedAdminBarnListAsync_InvalidFilterField_ReturnsError()
        {
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };
            var result = await _barnService.GetPaginatedAdminBarnListAsync(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedAdminBarnListAsync_InvalidSortField_ReturnsError()
        {
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await _barnService.GetPaginatedAdminBarnListAsync(request);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedAdminBarnListAsync_Success_ReturnsPaginatedAdminBarns()
        {
            // Arrange
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn1 = new Barn
            {
                Id = Guid.NewGuid(),
                BarnName = "Barn 1",
                Address = "Address 1",
                Image = "image1.jpg",
                WorkerId = worker.Id,
                Worker = worker,
                IsActive = true
            };
            var barn2 = new Barn
            {
                Id = Guid.NewGuid(),
                BarnName = "Barn 2",
                Address = "Address 2",
                Image = "image2.jpg",
                WorkerId = worker.Id,
                Worker = worker,
                IsActive = true
            };
            var barnsList = new List<Barn> { barn1, barn2 };

            // Setup InMemory DbContext for barns
            var options = new DbContextOptionsBuilder<TestBarnDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestBarnDbContext(options);
            context.Barns.AddRange(barnsList);
            context.SaveChanges();
            _barnRepositoryMock.Setup(x => x.GetQueryable()).Returns(context.Barns);

            // Setup active livestock circles (only barn1 has active livestock circle)
            var activeLivestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barn1.Id,
                LivestockCircleName = "Active LSC",
                StartDate = DateTime.UtcNow,
                EndDate = null,
                TotalUnit = 100,
                DeadUnit = 0,
                AverageWeight = 50.5f,
                GoodUnitNumber = 95,
                BadUnitNumber = 5,
                ReleaseDate = null,
                BreedId = Guid.NewGuid(),
                TechicalStaffId = Guid.NewGuid(),
                Status = Domain.Helper.Constants.StatusConstant.GROWINGSTAT,
                IsActive = true
            };
            var inactiveLivestockCircle = new LivestockCircle
            {
                Id = Guid.NewGuid(),
                BarnId = barn2.Id,
                LivestockCircleName = "Inactive LSC",
                StartDate = DateTime.UtcNow,
                EndDate = null,
                TotalUnit = 100,
                DeadUnit = 0,
                AverageWeight = 50.5f,
                GoodUnitNumber = 95,
                BadUnitNumber = 5,
                ReleaseDate = null,
                BreedId = Guid.NewGuid(),
                TechicalStaffId = Guid.NewGuid(),
                Status = Domain.Helper.Constants.StatusConstant.DONESTAT,
                IsActive = true
            };
            var livestockCircles = new List<LivestockCircle> { activeLivestockCircle, inactiveLivestockCircle };

            var lscOptions = new DbContextOptionsBuilder<TestLivestockCircleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var lscContext = new TestLivestockCircleDbContext(lscOptions);
            lscContext.LivestockCircles.AddRange(livestockCircles);
            lscContext.SaveChanges();
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircle, bool>>>()))
                .Returns((Expression<Func<LivestockCircle, bool>> expr) => lscContext.LivestockCircles.Where(expr));
            _livestockCircleRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(lscContext.LivestockCircles);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BarnName", Value = "asc" }
            };

            // Act
            var result = await _barnService.GetPaginatedAdminBarnListAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách phân trang chuồng trại cho admin thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            var adminBarn1 = result.Data.Items.FirstOrDefault(x => x.Id == barn1.Id);
            var adminBarn2 = result.Data.Items.FirstOrDefault(x => x.Id == barn2.Id);
            Assert.NotNull(adminBarn1);
            Assert.NotNull(adminBarn2);
            Assert.True(adminBarn1.HasActiveLivestockCircle);
            Assert.False(adminBarn2.HasActiveLivestockCircle);
        }
    }

    // Minimal InMemory DbContext for test
    public class TestBarnDbContext : DbContext
    {
        public TestBarnDbContext(DbContextOptions<TestBarnDbContext> options) : base(options) { }
        public DbSet<Barn> Barns { get; set; }
    }
    public class TestLivestockCircleDbContext : DbContext
    {
        public TestLivestockCircleDbContext(DbContextOptions<TestLivestockCircleDbContext> options) : base(options) { }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
    }
}
