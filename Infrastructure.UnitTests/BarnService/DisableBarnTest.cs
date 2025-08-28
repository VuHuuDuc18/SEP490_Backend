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
    public class DisableBarnTest
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

        public DisableBarnTest()
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
        public async Task DisableBarn_BarnNotFoundOrInactive_ReturnsError()
        {
            var barnId = Guid.NewGuid();
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync((Barn)null);

            var result = await _barnService.DisableBarn(barnId);

            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DisableBarn_Success_Disable_ReturnsSuccess()
        {
            var barnId = Guid.NewGuid();
            var barn = new Barn { Id = barnId, IsActive = true };
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);
            _barnRepositoryMock.Setup(x => x.Update(barn)).Verifiable();

            var result = await _barnService.DisableBarn(barnId);

            Assert.True(result.Succeeded);
            Assert.Contains("xóa", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(barn.IsActive);
        }

        [Fact]
        public async Task DisableBarn_Success_Restore_ReturnsSuccess()
        {
            var barnId = Guid.NewGuid();
            var barn = new Barn { Id = barnId, IsActive = false };
            _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);
            _barnRepositoryMock.Setup(x => x.Update(barn)).Verifiable();

            var result = await _barnService.DisableBarn(barnId);

            Assert.True(result.Succeeded);
            Assert.Contains("khôi phục", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.True(barn.IsActive);
        }

        //[Fact]
        //public async Task DisableBarn_Exception_ReturnsError()
        //{
        //    var barnId = Guid.NewGuid();
        //    var barn = new Barn { Id = barnId, IsActive = true };
        //    _barnRepositoryMock.Setup(x => x.GetByIdAsync(barnId, default)).ReturnsAsync(barn);
        //    _barnRepositoryMock.Setup(x => x.Update(barn)).Throws(new Exception("db error"));

        //    var result = await _barnService.DisableBarn(barnId);

        //    Assert.False(result.Succeeded);
        //    Assert.Contains("lỗi", result.Message, StringComparison.OrdinalIgnoreCase);
        //}
    }
}
