using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request.Food;
using Domain.Dto.Response.Food;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.FoodService
{
    public class CreateFoodTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public CreateFoodTest()
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
        public async Task CreateFood_RequestNull_ReturnsError()
        {
            var result = await _FoodService.CreateFood(null, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu thức ăn không được null", result.Message);
            Assert.Contains("Dữ liệu thức ăn không được null", result.Errors);
        }

        [Fact]
        public async Task CreateFood_UserIdEmpty_ReturnsError()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var service = new Infrastructure.Services.Implements.FoodService(
                _FoodRepositoryMock.Object,
                _imageFoodRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _FoodCategoryRepositoryMock.Object,
                httpContextAccessorMock.Object
            );
            var request = new CreateFoodRequest { FoodName = "Food1", FoodCategoryId = Guid.NewGuid(), Stock = 10 };
            var result = await service.CreateFood(request, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Hãy đăng nhập và thử lại", result.Message);
            Assert.Contains("Hãy đăng nhập và thử lại", result.Errors);
        }

        [Fact]
        public async Task CreateFood_ValidationError_ReturnsError()
        {
            var request = new CreateFoodRequest { FoodName = "", FoodCategoryId = Guid.NewGuid(), Stock = 10 };
            var result = await _FoodService.CreateFood(request, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Assert.Contains(result.Errors, e => e.Contains("Tên thức ăn là bắt buộc"));
        }

        [Fact]
        public async Task CreateFood_FoodNameExists_ReturnsError()
        {
            // Arrange
            var request = new CreateFoodRequest { FoodName = "Food1", FoodCategoryId = Guid.NewGuid(), Stock = 10, WeighPerUnit = 40 };
            var Foods = new List<Food> { new Food { FoodName = "Food1", FoodCategoryId = request.FoodCategoryId, IsActive = true } };
            var mockQueryable = Foods.AsQueryable().BuildMock();

            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _FoodService.CreateFood(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đã tồn tại", result.Message);
            Assert.Contains("đã tồn tại", result.Errors[0]);
        }

        [Fact]
        public async Task CreateFood_FoodCategoryNotFound_ReturnsError()
        {
            // Arrange
            var request = new CreateFoodRequest { FoodName = "Food1", FoodCategoryId = Guid.NewGuid(), Stock = 10 , WeighPerUnit = 40 };
            var Foods = new List<Food>().AsQueryable().BuildMock();

            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));

            _FoodCategoryRepositoryMock.Setup(x => x.GetByIdAsync(request.FoodCategoryId, null))
                .ReturnsAsync((FoodCategory?)null);

            // Act
            var result = await _FoodService.CreateFood(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục thức ăn không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Danh mục thức ăn không tồn tại hoặc đã bị xóa", result.Errors);
        }

        [Fact]
        public async Task CreateFood_Success_WithImageAndThumbnail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var request = new CreateFoodRequest
            {
                FoodName = "Food1",
                FoodCategoryId = categoryId,
                Stock = 10,
                WeighPerUnit = 40,
            };
            var Foods = new List<Food>().AsQueryable().BuildMock();

            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));

            _FoodCategoryRepositoryMock
                .Setup(x => x.GetByIdAsync(request.FoodCategoryId, null))
                .ReturnsAsync(new FoodCategory { Id = request.FoodCategoryId, IsActive = true });

            _FoodRepositoryMock.Setup(x => x.Insert(It.IsAny<Food>()));
            _FoodRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _imageFoodRepositoryMock.Setup(x => x.Insert(It.IsAny<ImageFood>()));
            _imageFoodRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _cloudinaryCloudServiceMock.Setup(x => x.UploadImage(It.IsAny<string>(), "Food", It.IsAny<CancellationToken>())).ReturnsAsync("https://cloudinary.com/Food-image.jpg");

            // Act
            var result = await _FoodService.CreateFood(request, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Tạo thức ăn thành công", result.Message);
            Assert.Contains("Thức ăn đã được tạo thành công. ID:", result.Data);
            _FoodRepositoryMock.Verify(x => x.Insert(It.IsAny<Food>()), Times.Once());
            _FoodRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            //_imageFoodRepositoryMock.Verify(x => x.Insert(It.IsAny<ImageFood>()), Times.Once());
            //_imageFoodRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        //[Fact]
        //public async Task CreateFood_Exception_ReturnsError()
        //{
        //    var request = new CreateFoodRequest { FoodName = "Food1", FoodCategoryId = Guid.NewGuid(), Stock = 10 };
        //    var Foods = new List<Food>().AsQueryable().BuildMock();
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>()).AnyAsync(default))
        //        .ThrowsAsync(new Exception("DB error"));
        //    var result = await _FoodService.CreateFood(request, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi tạo thức ăn loài", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
