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
using Domain.Dto.Response.Food;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.FoodService
{
    public class GetFoodByCategoryTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetFoodByCategoryTest()
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

        //[Fact]
        //public async Task GetFoodByCategory_FilterByName_ReturnsFiltered()
        //{
        //    var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var Foods = new List<Food>
        //    {
        //        new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategory = category, FoodCategoryId = category.Id, IsActive = true },
        //        new Food { Id = Guid.NewGuid(), FoodName = "Other", FoodCategory = category, FoodCategoryId = category.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
        //    _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
        //        .Returns(new List<ImageFood>().AsQueryable().BuildMock());
        //    var result = await _FoodService.GetFoodByCategory("Food1", null, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Food1", result.Data[0].FoodName);
        //}

        //[Fact]
        //public async Task GetFoodByCategory_FilterByCategoryId_ReturnsFiltered()
        //{
        //    var category1 = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var category2 = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
        //    var Foods = new List<Food>
        //    {
        //        new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategory = category1, FoodCategoryId = category1.Id, IsActive = true },
        //        new Food { Id = Guid.NewGuid(), FoodName = "Food2", FoodCategory = category2, FoodCategoryId = category2.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
        //    _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
        //        .Returns(new List<ImageFood>().AsQueryable().BuildMock());
        //    var result = await _FoodService.GetFoodByCategory(null, category2.Id, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Food2", result.Data[0].FoodName);
        //}

        [Fact]
        public async Task GetFoodByCategory_FilterByNameAndCategoryId_ReturnsFiltered()
        {
            var category1 = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var category2 = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
            var Foods = new List<Food>
            {
                new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategory = category1, FoodCategoryId = category1.Id, IsActive = true },
                new Food { Id = Guid.NewGuid(), FoodName = "Food2", FoodCategory = category2, FoodCategoryId = category2.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns(new List<ImageFood>().AsQueryable().BuildMock());
            var result = await _FoodService.GetFoodByCategory("Food2", category2.Id, default);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            Assert.Equal("Food2", result.Data[0].FoodName);
        }

        [Fact]
        public async Task GetFoodByCategory_NoResult_ReturnsEmpty()
        {
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Foods = new List<Food>
            {
                new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategory = category, FoodCategoryId = category.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns(new List<ImageFood>().AsQueryable().BuildMock());
            var result = await _FoodService.GetFoodByCategory("NotExist", null, default);
            Assert.True(result.Succeeded);
            Assert.Empty(result.Data);
        }

        //[Fact]
        //public async Task GetFoodByCategory_Exception_ReturnsError()
        //{
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    var result = await _FoodService.GetFoodByCategory(null, null, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách thức ăn", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
} 