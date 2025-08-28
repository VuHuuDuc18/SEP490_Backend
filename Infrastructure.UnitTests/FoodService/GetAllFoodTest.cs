using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.FoodService
{
    public class GetAllFoodTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;

        public GetAllFoodTest()
        {
            _FoodRepositoryMock = new Mock<IRepository<Food>>();
            _imageFoodRepositoryMock = new Mock<IRepository<ImageFood>>();
            _FoodCategoryRepositoryMock = new Mock<IRepository<FoodCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock HttpContext để lấy userId
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("uid", Guid.NewGuid().ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(user);

            // Mock IOptions<CloudinaryConfig>
            var cloudinaryConfigMock = new Mock<IOptions<Domain.Settings.CloudinaryConfig>>();
            cloudinaryConfigMock.Setup(x => x.Value).Returns(new Domain.Settings.CloudinaryConfig
            {
                CloudName = "dpgk5pqt9",
                ApiKey = "382542864398655",
                ApiSecret = "ct6gqlmsftVgmj2C3A8tYoiQk0M"
            });

            // Khởi tạo CloudinaryCloudService với mock config
            var cloudinaryService = new CloudinaryCloudService(cloudinaryConfigMock.Object);

            _FoodService = new Infrastructure.Services.Implements.FoodService(
                _FoodRepositoryMock.Object,
                _imageFoodRepositoryMock.Object,
                cloudinaryService,
                _FoodCategoryRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllFood_ReturnsListOfFoods_WhenFoodsExist()
        {
            // Arrange
            var Foods = new List<Food>
            {
                new Food
                {
                    Id = Guid.NewGuid(),
                    FoodName = "Food1",
                    FoodCategoryId = Guid.NewGuid(),
                    Stock = 10,
                    IsActive = true,
                    FoodCategory = new FoodCategory { Id = Guid.NewGuid(), Name = "Category1", Description = "Desc1" }
                },
                new Food
                {
                    Id = Guid.NewGuid(),
                    FoodName = "Food2",
                    FoodCategoryId = Guid.NewGuid(),
                    Stock = 20,
                    IsActive = true,
                    FoodCategory = new FoodCategory { Id = Guid.NewGuid(), Name = "Category2", Description = "Desc2" }
                }
            };
            var images = new List<ImageFood>
            {
                new ImageFood { FoodId = Foods[0].Id, ImageLink = "image1.jpg", Thumnail = "true" },
                new ImageFood { FoodId = Foods[0].Id, ImageLink = "image2.jpg", Thumnail = "false" },
                new ImageFood { FoodId = Foods[1].Id, ImageLink = "image3.jpg", Thumnail = "true" }
            };

            var FoodMock = Foods.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> predicate) => FoodMock.Where(predicate));
            _FoodRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(() => FoodMock); // Loại bỏ mock Include, trả về IQueryable đã chứa dữ liệu
            var imageMock = images.AsQueryable().BuildMock();
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns(() => imageMock);
            //_FoodCategoryRepositoryMock.Setup(x => x.GetQueryable()).Returns(categories.AsQueryable());

            // Act
            var result = await _FoodService.GetAllFood(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Food1", result[0].FoodName);
            Assert.Equal("Category1", result[0].FoodCategory.Name);
            Assert.Equal("image1.jpg", result[0].Thumbnail);
            Assert.Contains("image2.jpg", result[0].ImageLinks);
            Assert.Equal("Food2", result[1].FoodName);
            Assert.Equal("Category2", result[1].FoodCategory.Name);
            Assert.Equal("image3.jpg", result[1].Thumbnail);
        }

        [Fact]
        public async Task GetAllFood_ReturnsEmptyList_WhenNoFoodsExist()
        {
            // Arrange
            var Foods = new List<Food>();
            var FoodMock = Foods.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> predicate) => FoodMock.Where(predicate));
            var imageMock = Enumerable.Empty<ImageFood>().AsQueryable().BuildMock();
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns(() => imageMock);

            // Act
            var result = await _FoodService.GetAllFood(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //[Fact]
        //public async Task GetAllFood_ThrowsException_WhenRepositoryFails()
        //{
        //    // Arrange
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<Exception>(() => _FoodService.GetAllFood(CancellationToken.None));
        //    Assert.Equal("Database error", exception.Message);
        //}
    }
}