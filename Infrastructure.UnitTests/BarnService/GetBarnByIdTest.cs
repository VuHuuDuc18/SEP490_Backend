using System;
using System.Security.Claims;
using System.Threading.Tasks;
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

namespace Infrastructure.UnitTests.BarnService
{
    public class GetBarnByIdTest
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

        public GetBarnByIdTest()
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
            var claims = new[] { new Claim("uid", _userId.ToString()) };
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
        public async Task GetBarnById_BarnNotFound_ReturnsError()
        {
            var barnId = Guid.NewGuid();
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync((Barn)null);

            var result = await _barnService.GetBarnById(barnId);

            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        //[Fact]
        //public async Task GetBarnById_BarnInactive_ReturnsError()
        //{
        //    var barnId = Guid.NewGuid();
        //    var barn = new Barn { Id = barnId, IsActive = false };
        //    _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);

        //    var result = await _barnService.GetBarnById(barnId);

        //    Assert.False(result.Succeeded);
        //    Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        [Fact]
        public async Task GetBarnById_WorkerNotFoundOrInactive_ReturnsError()
        {
            var barnId = Guid.NewGuid();
            var barn = new Barn { Id = barnId, IsActive = true, Worker = null };
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);

            var result = await _barnService.GetBarnById(barnId);

            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnById_WorkerInactive_ReturnsError()
        {
            var barnId = Guid.NewGuid();
            var worker = new User { Id = Guid.NewGuid(), IsActive = false };
            var barn = new Barn { Id = barnId, IsActive = true, Worker = worker };
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);

            var result = await _barnService.GetBarnById(barnId);

            Assert.False(result.Succeeded);
            Assert.Contains("người gia công", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetBarnById_Success_ReturnsBarnResponse()
        {
            var barnId = Guid.Parse("e1d40884-ff30-407a-8934-0a580d2bd57a");
            var worker = new User { Id = Guid.NewGuid(), IsActive = true, FullName = "Worker 1", Email = "worker1@email.com" };
            var barn = new Barn
            {
                Id = barnId,
                BarnName = "Chuồng 1",
                Address = "Địa chỉ 1",
                Image = "image.jpg",
                Worker = worker,
                IsActive = true
            };
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);

            var result = await _barnService.GetBarnById(barnId);

            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin chuồng trại thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(barnId, result.Data.Id);
            Assert.Equal("Chuồng 1", result.Data.BarnName);
            Assert.Equal("Địa chỉ 1", result.Data.Address);
            Assert.Equal("image.jpg", result.Data.Image);
            Assert.Equal(worker.Id, result.Data.Worker.Id);
            Assert.Equal(worker.FullName, result.Data.Worker.FullName);
            Assert.Equal(worker.Email, result.Data.Worker.Email);
        }
    }
}
