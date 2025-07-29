using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Infrastructure.Services;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.BreedService
{
    public class DisableBreedTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableBreedTest()
        {
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedCategoryRepositoryMock = new Mock<IRepository<BreedCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("uid", _userId.ToString()) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _breedService = new Infrastructure.Services.Implements.BreedService(
                _breedRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _breedCategoryRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task DisableBreed_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync((Breed?)null);
            var result = await _breedService.DisableBreed(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Giống loài không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Giống loài không tồn tại hoặc đã bị xóa", result.Errors);
        }

        [Fact]
        public async Task DisableBreed_Success_Disable()
        {
            var id = Guid.NewGuid();
            var existing = new Breed { Id = id, BreedName = "Breed1", IsActive = true };
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _breedRepositoryMock.Setup(x => x.Update(It.IsAny<Breed>()));
            _breedRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _breedService.DisableBreed(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa giống thành công", result.Message);
            Assert.Contains("Giống đã được xóa thành công. ID:", result.Data);
            _breedRepositoryMock.Verify(x => x.Update(It.IsAny<Breed>()), Times.Once());
            _breedRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive);
        }

        [Fact]
        public async Task DisableBreed_Success_EnableAgain()
        {
            var id = Guid.NewGuid();
            var existing = new Breed { Id = id, BreedName = "Breed1", IsActive = false };
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _breedRepositoryMock.Setup(x => x.Update(It.IsAny<Breed>()));
            _breedRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _breedService.DisableBreed(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục giống thành công", result.Message);
            Assert.Contains("Giống đã được khôi phục thành công. ID:", result.Data);
            _breedRepositoryMock.Verify(x => x.Update(It.IsAny<Breed>()), Times.Once());
            _breedRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive);
        }

        [Fact]
        public async Task DisableBreed_Exception_ReturnsError()
        {
            var id = Guid.NewGuid();
            var existing = new Breed { Id = id, BreedName = "Breed1", IsActive = true };
            _breedRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _breedRepositoryMock.Setup(x => x.Update(It.IsAny<Breed>())).Throws(new Exception("DB error"));
            var result = await _breedService.DisableBreed(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi xóa giống loài", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
} 