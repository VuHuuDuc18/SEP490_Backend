﻿using Entities.EntityModel;
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

namespace Infrastructure.UnitTests.FoodCategoryService
{
    public class DisableFoodCategoryTest
    {
        private readonly Mock<IRepository<FoodCategory>> _foodCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodCategoryService _foodCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableFoodCategoryTest()
        {
            _foodCategoryRepoMock = new Mock<IRepository<FoodCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _foodCategoryService = new Infrastructure.Services.Implements.FoodCategoryService(
                _foodCategoryRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task DisableFoodCategory_NotFound_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _foodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((FoodCategory)null);

            // Act
            var result = await _foodCategoryService.DisableFoodCategory(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục thức ăn không tồn tại", result.Message);
            Assert.Contains("Danh mục thức ăn không tồn tại", result.Errors);
        }

        [Fact]
        public async Task DisableFoodCategory_Success_Disable()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new FoodCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
            _foodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _foodCategoryRepoMock.Setup(x => x.Update(It.IsAny<FoodCategory>()));
            _foodCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _foodCategoryService.DisableFoodCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa danh mục thức ăn thành công", result.Message);
            Assert.Contains("Danh mục thức ăn đã được xóa thành công. ID:", result.Data);
            _foodCategoryRepoMock.Verify(x => x.Update(It.IsAny<FoodCategory>()), Times.Once());
            _foodCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive); // Đã bị disable
        }

        [Fact]
        public async Task DisableFoodCategory_Success_EnableAgain()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new FoodCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = false };
            _foodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _foodCategoryRepoMock.Setup(x => x.Update(It.IsAny<FoodCategory>()));
            _foodCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _foodCategoryService.DisableFoodCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục danh mục thức ăn thành công", result.Message);
            Assert.Contains("Danh mục thức ăn đã được khôi phục thành công. ID:", result.Data);
            _foodCategoryRepoMock.Verify(x => x.Update(It.IsAny<FoodCategory>()), Times.Once());
            _foodCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive); // Đã được enable lại
        }

        //[Fact]
        //public async Task DisableFoodCategory_Exception_ReturnsError()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    var existing = new FoodCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
        //    _foodCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
        //    _foodCategoryRepoMock.Setup(x => x.Update(It.IsAny<FoodCategory>())).Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _foodCategoryService.DisableFoodCategory(id, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi xóa danh mục thức ăn", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
