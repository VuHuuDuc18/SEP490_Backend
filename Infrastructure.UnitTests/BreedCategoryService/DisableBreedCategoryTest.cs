using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using Domain.Dto.Request.Category;
using Application.Wrappers;

namespace Infrastructure.UnitTests.BreedCategoryService
{
    public class DisableBreedCategoryTest
    {
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedCategoryService _breedCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableBreedCategoryTest()
        {
            _breedCategoryRepoMock = new Mock<IRepository<BreedCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _breedCategoryService = new Infrastructure.Services.Implements.BreedCategoryService(
                _breedCategoryRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task DisableBreedCategory_NotFound_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((BreedCategory)null);

            // Act
            var result = await _breedCategoryService.DisableBreedCategory(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục giống không tồn tại", result.Message);
            Assert.Contains("Danh mục giống không tồn tại", result.Errors);
        }

        [Fact]
        public async Task DisableBreedCategory_Success_Disable()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new BreedCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _breedCategoryRepoMock.Setup(x => x.Update(It.IsAny<BreedCategory>()));
            _breedCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _breedCategoryService.DisableBreedCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa danh mục giống thành công", result.Message);
            Assert.Contains("Danh mục giống đã được xóa thành công. ID:", result.Data);
            _breedCategoryRepoMock.Verify(x => x.Update(It.IsAny<BreedCategory>()), Times.Once());
            _breedCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive); // Đã bị disable
        }

        [Fact]
        public async Task DisableBreedCategory_Success_EnableAgain()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new BreedCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = false };
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _breedCategoryRepoMock.Setup(x => x.Update(It.IsAny<BreedCategory>()));
            _breedCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _breedCategoryService.DisableBreedCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục danh mục giống thành công", result.Message);
            Assert.Contains("Danh mục giống đã được khôi phục thành công. ID:", result.Data);
            _breedCategoryRepoMock.Verify(x => x.Update(It.IsAny<BreedCategory>()), Times.Once());
            _breedCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive); // Đã được enable lại
        }

        //[Fact]
        //public async Task DisableBreedCategory_Exception_ReturnsError()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    var existing = new BreedCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
        //    _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
        //    _breedCategoryRepoMock.Setup(x => x.Update(It.IsAny<BreedCategory>())).Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _breedCategoryService.DisableBreedCategory(id, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi xóa danh mục giống", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
