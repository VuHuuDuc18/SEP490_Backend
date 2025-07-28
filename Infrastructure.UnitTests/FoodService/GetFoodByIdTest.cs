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
    public class GetFoodByIdTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetFoodByIdTest()
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
        public async Task GetFoodById_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            var Foods = new List<Food>().AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            var result = await _FoodService.GetFoodById(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Thức ăn không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Thức ăn không tồn tại hoặc đã bị xóa", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetFoodById_Success_ReturnsData()
        {
            var id = Guid.NewGuid();
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Food = new Food { Id = id, FoodName = "Food1", FoodCategory = category, Stock = 10, IsActive = true };
            var Foods = new List<Food> { Food }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            var images = new List<ImageFood>
            {
                new ImageFood { FoodId = id, ImageLink = "img1.jpg", Thumnail = "true" },
                new ImageFood { FoodId = id, ImageLink = "img2.jpg", Thumnail = "false" }
            };
            var imageMock = images.AsQueryable().BuildMock();
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<ImageFood, bool>> predicate) => imageMock.Where(predicate));
            var result = await _FoodService.GetFoodById(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            Assert.Equal("Food1", result.Data.FoodName);
            Assert.Equal("Cat1", result.Data.FoodCategory.Name);
            Assert.Equal("img1.jpg", result.Data.Thumbnail);
            Assert.Contains("img2.jpg", result.Data.ImageLinks);
        }

        [Fact]
        public async Task GetFoodById_Success_NoImages()
        {
            var id = Guid.NewGuid();
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Food = new Food { Id = id, FoodName = "Food1", FoodCategory = category, Stock = 10, IsActive = true };
            var Foods = new List<Food> { Food }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            var imageMock = new List<ImageFood>().AsQueryable().BuildMock();
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<ImageFood, bool>> predicate) => imageMock.Where(predicate));
            var result = await _FoodService.GetFoodById(id, default);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Thumbnail);
            Assert.Empty(result.Data.ImageLinks);
        }

        [Fact]
        public async Task GetFoodById_Exception_ReturnsError()
        {
            var id = Guid.NewGuid();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Throws(new Exception("DB error"));
            var result = await _FoodService.GetFoodById(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy thông tin thức ăn", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
            Assert.Null(result.Data);
        }
    }
} 