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
using Domain.Dto.Response.Breed;
using Application.Wrappers;

namespace Infrastructure.UnitTests.BreedCategoryService
{
    public class GetFoodCategoryByIdTest
    {
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedCategoryService _breedCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetFoodCategoryByIdTest()
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
        public async Task GetBreedCategoryById_NotFoundOrInactive_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((BreedCategory)null);

            // Act
            var result = await _breedCategoryService.GetBreedCategoryById(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục giống không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Danh mục giống không tồn tại hoặc đã bị xóa", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetBreedCategoryById_Inactive_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new BreedCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = false };
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);

            // Act
            var result = await _breedCategoryService.GetBreedCategoryById(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục giống không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Danh mục giống không tồn tại hoặc đã bị xóa", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetBreedCategoryById_Success_ReturnsData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new BreedCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);

            // Act
            var result = await _breedCategoryService.GetBreedCategoryById(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin danh mục giống thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            Assert.Equal("Category 1", result.Data.Name);
            Assert.Equal("desc", result.Data.Description);
        }

        [Fact]
        public async Task GetBreedCategoryById_Exception_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _breedCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _breedCategoryService.GetBreedCategoryById(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy thông tin danh mục giống", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
            Assert.Null(result.Data);
        }
    }
}
