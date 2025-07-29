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

namespace Infrastructure.UnitTests.FoodService
{
    public class DisableFoodTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableFoodTest()
        {
            _FoodRepositoryMock = new Mock<IRepository<Food>>();
            _imageFoodRepositoryMock = new Mock<IRepository<ImageFood>>();
            _FoodCategoryRepositoryMock = new Mock<IRepository<FoodCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("uid", _userId.ToString()) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _FoodService = new Infrastructure.Services.Implements.FoodService(
                _FoodRepositoryMock.Object,
                _imageFoodRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _FoodCategoryRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task DisableFood_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            _FoodRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync((Food?)null);
            var result = await _FoodService.DisableFood(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Thức ăn không tồn tại", result.Message);
            Assert.Contains("Thức ăn không tồn tại", result.Errors);
        }

        [Fact]
        public async Task DisableFood_Success_Disable()
        {
            var id = Guid.NewGuid();
            var existing = new Food { Id = id, FoodName = "Food1", IsActive = true };
            _FoodRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _FoodRepositoryMock.Setup(x => x.Update(It.IsAny<Food>()));
            _FoodRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _FoodService.DisableFood(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa thức ăn thành công", result.Message);
            Assert.Contains("Thức ăn đã được xóa thành công. ID:", result.Data);
            _FoodRepositoryMock.Verify(x => x.Update(It.IsAny<Food>()), Times.Once());
            _FoodRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive);
        }

        [Fact]
        public async Task DisableFood_Success_EnableAgain()
        {
            var id = Guid.NewGuid();
            var existing = new Food { Id = id, FoodName = "Food1", IsActive = false };
            _FoodRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _FoodRepositoryMock.Setup(x => x.Update(It.IsAny<Food>()));
            _FoodRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _FoodService.DisableFood(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục thức ăn thành công", result.Message);
            Assert.Contains("Thức ăn đã được khôi phục thành công. ID:", result.Data);
            _FoodRepositoryMock.Verify(x => x.Update(It.IsAny<Food>()), Times.Once());
            _FoodRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive);
        }

        [Fact]
        public async Task DisableFood_Exception_ReturnsError()
        {
            var id = Guid.NewGuid();
            var existing = new Food { Id = id, FoodName = "Food1", IsActive = true };
            _FoodRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _FoodRepositoryMock.Setup(x => x.Update(It.IsAny<Food>())).Throws(new Exception("DB error"));
            var result = await _FoodService.DisableFood(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi xóa thức ăn", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
} 