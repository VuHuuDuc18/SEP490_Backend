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
using MockQueryable;

namespace Infrastructure.UnitTests.FoodCategoryService
{
    public class GetFoodCategoryByNameTest
    {
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodCategoryService _FoodCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetFoodCategoryByNameTest()
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
        public async Task GetFoodCategoryByName_AllActive_ReturnsAll()
        {
            // Arrange
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true },
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true },
                new FoodCategory { Id = Guid.NewGuid(), Name = "Inactive", Description = "desc 3", IsActive = false }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _FoodCategoryService.GetFoodCategoryByName(null, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách danh mục thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.True(c.Name.StartsWith("Category")));
        }

        //[Fact]
        //public async Task GetFoodCategoryByName_FilterByName_ReturnsFiltered()
        //{
        //    // Arrange
        //    var categories = new List<FoodCategory>
        //    {
        //        new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true },
        //        new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true },
        //        new FoodCategory { Id = Guid.NewGuid(), Name = "Inactive", Description = "desc 3", IsActive = false }
        //    };
        //    var mockQueryable = categories.AsQueryable().BuildMock();
        //    _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));

        //    // Act
        //    var result = await _FoodCategoryService.GetFoodCategoryByName("2", default);

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Equal("Lấy danh sách danh mục thức ăn thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Category 2", result.Data[0].Name);
        //}

        [Fact]
        public async Task GetFoodCategoryByName_NoResult_ReturnsEmpty()
        {
            // Arrange
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _FoodCategoryService.GetFoodCategoryByName("NotExist", default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách danh mục thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        //[Fact]
        //public async Task GetFoodCategoryByName_Exception_ReturnsError()
        //{
        //    // Arrange
        //    _FoodCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
        //        .Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _FoodCategoryService.GetFoodCategoryByName(null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách danh mục thức ăn", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
