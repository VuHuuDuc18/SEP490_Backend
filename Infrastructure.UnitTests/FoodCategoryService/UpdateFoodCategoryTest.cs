using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Dto.Request.Category;
using Application.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.FoodCategoryService
{
    public class UpdateFoodCategoryTest
    {
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodCategoryService _FoodCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateFoodCategoryTest()
        {
            _FoodCategoryRepoMock = new Mock<IRepository<FoodCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _FoodCategoryService = new Infrastructure.Services.Implements.FoodCategoryService(
                _FoodCategoryRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task UpdateFoodCategory_RequestNull_ReturnsError()
        {
            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu danh mục thức ăn không được null", result.Message);
            Assert.Contains("Dữ liệu danh mục thức ăn không được null", result.Errors);
        }

        [Fact]
        public async Task UpdateFoodCategory_FoodCategoryNotFoundOrInactive_ReturnsError()
        {
            // Arrange
            var request = new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc" };
            _FoodCategoryRepoMock.Setup(x => x.GetByIdAsync(request.Id, default)).ReturnsAsync((FoodCategory)null);

            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục thức ăn không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Danh mục thức ăn không tồn tại hoặc đã bị xóa", result.Errors);
        }

        [Fact]
        public async Task UpdateFoodCategory_NameBlank_ReturnsValidationError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new FoodCategory { Id = id, IsActive = true };
            _FoodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            var request = new UpdateCategoryRequest { Id = id, Name = "", Description = "desc" };

            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Assert.Contains("Tên danh mục là bắt buộc.", result.Errors);
        }

        [Fact]
        public async Task UpdateFoodCategory_NameDuplicate_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateCategoryRequest { Id = id, Name = "Category 1", Description = "desc" };
            var existing = new FoodCategory { Id = id, Name = "Old Name", Description = "desc", IsActive = true };
            _FoodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đã tồn tại", result.Message);
            Assert.Contains("đã tồn tại", result.Errors[0]);
        }

        [Fact]
        public async Task UpdateFoodCategory_Success_ReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateCategoryRequest { Id = id, Name = "Category 2", Description = "desc" };
            var existing = new FoodCategory { Id = id, Name = "Old Name", Description = "desc", IsActive = true };
            _FoodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            var categories = new List<FoodCategory>().AsQueryable();
            var mockQueryable = categories.BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));
            _FoodCategoryRepoMock.Setup(x => x.Update(It.IsAny<FoodCategory>()));
            _FoodCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(request, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Cập nhật danh mục thức ăn thành công", result.Message);
            Assert.Contains("Danh mục thức ăn đã được cập nhật thành công. ID:", result.Data);
            _FoodCategoryRepoMock.Verify(x => x.Update(It.IsAny<FoodCategory>()), Times.Once());
            _FoodCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateFoodCategory_Exception_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateCategoryRequest { Id = id, Name = "Category 3", Description = "desc" };
            var existing = new FoodCategory { Id = id, Name = "Old Name", Description = "desc", IsActive = true };
            _FoodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            var categories = new List<FoodCategory>().AsQueryable();
            var mockQueryable = categories.BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));
            _FoodCategoryRepoMock.Setup(x => x.Update(It.IsAny<FoodCategory>())).Throws(new Exception("DB error"));

            // Act
            var result = await _FoodCategoryService.UpdateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi cập nhật danh mục thức ăn", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
}
