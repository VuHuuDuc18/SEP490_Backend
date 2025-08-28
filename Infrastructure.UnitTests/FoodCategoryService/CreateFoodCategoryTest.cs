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
    public class CreateFoodCategoryTest
    {
        private readonly Mock<IRepository<FoodCategory>> _foodCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodCategoryService _foodCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public CreateFoodCategoryTest()
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

        //[Fact]
        //public async Task CreateFoodCategory_RequestNull_ReturnsError()
        //{
        //    // Act
        //    var result = await _foodCategoryService.CreateFoodCategory(null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Dữ liệu danh mục thức ăn không được null", result.Message);
        //    Assert.Contains("Dữ liệu danh mục thức ăn không được null", result.Errors);
        //}

        [Fact]
        public async Task CreateFoodCategory_NameBlank_ReturnsValidationError()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "", Description = "desc" };

            // Act
            var result = await _foodCategoryService.CreateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Assert.Contains("Tên danh mục là bắt buộc.", result.Errors);
        }

        [Fact]
        public async Task CreateFoodCategory_NameDuplicate_ReturnsError()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Category 1", Description = "desc" };
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _foodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));
            // XÓA setup AnyAsync

            // Act
            var result = await _foodCategoryService.CreateFoodCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đã tồn tại", result.Message);
            Assert.Contains("đã tồn tại", result.Errors[0]);
        }

        [Fact]
        public async Task CreateFoodCategory_Success_ReturnsOk()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Category 2", Description = "desc" };
            var categories = new List<FoodCategory>().AsQueryable();
            var mockQueryable = categories.BuildMock();
            _foodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));
            // XÓA setup AnyAsync
            _foodCategoryRepoMock.Setup(x => x.Insert(It.IsAny<FoodCategory>()));
            _foodCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _foodCategoryService.CreateFoodCategory(request, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Tạo danh mục thức ăn thành công", result.Message);
            Assert.Contains("Danh mục thức ăn đã được tạo thành công. ID:", result.Data);
            _foodCategoryRepoMock.Verify(x => x.Insert(It.IsAny<FoodCategory>()), Times.Once());
            _foodCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        //[Fact]
        //public async Task CreateFoodCategory_Exception_ReturnsError()
        //{
        //    // Arrange
        //    var request = new CreateCategoryRequest { Name = "Category 3", Description = "desc" };
        //    var categories = new List<FoodCategory>().AsQueryable();
        //    var mockQueryable = categories.BuildMock();
        //    _foodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));
        //    // XÓA setup AnyAsync
        //    _foodCategoryRepoMock.Setup(x => x.Insert(It.IsAny<FoodCategory>())).Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _foodCategoryService.CreateFoodCategory(request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi tạo danh mục thức ăn", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
