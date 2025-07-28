using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Food;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.FoodService
{
    public class ExcelDataHandleTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public ExcelDataHandleTest()
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
        public async Task ExcelDataHandle_AddNewFoodAndCategory_Success()
        {
            var cell = new CellFoodItem { Ten = "Food1", Phan_Loai = "Cat1", So_luong = 5 };
            var Foods = new List<Food>().AsQueryable().BuildMock();
            var categories = new List<FoodCategory>().AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            _FoodCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => categories.Where(predicate));
            _FoodCategoryRepositoryMock.Setup(x => x.Insert(It.IsAny<FoodCategory>()));
            _FoodCategoryRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
 
            _FoodRepositoryMock.Setup(x => x.Insert(It.IsAny<Food>()));
            _FoodRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _FoodService.ExcelDataHandle(new List<CellFoodItem> { cell });
            Assert.True(result);
            _FoodCategoryRepositoryMock.Verify(x => x.Insert(It.IsAny<FoodCategory>()), Times.Once());
            _FoodRepositoryMock.Verify(x => x.Insert(It.IsAny<Food>()), Times.Once());
        }

        [Fact]
        public async Task ExcelDataHandle_IncrementStock_Success()
        {
            var cell = new CellFoodItem { Ten = "Food1", Phan_Loai = "Cat1", So_luong = 3 };
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Cat1" };
            var Food = new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategoryId = category.Id, Stock = 2, IsActive = true };
            var Foods = new List<Food> { Food }.AsQueryable().BuildMock();
            var categories = new List<FoodCategory> { category }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Food, bool>> predicate) => Foods.Where(predicate));
            _FoodCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<FoodCategory, bool>> predicate) => categories.Where(predicate));
            _FoodRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _FoodService.ExcelDataHandle(new List<CellFoodItem> { cell });
            Assert.True(result);
            Assert.Equal(5, Food.Stock);
        }

        [Fact]
        public async Task ExcelDataHandle_Exception_ReturnsError()
        {
            _FoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Food, bool>>>() ))
                .Throws(new Exception("DB error"));
            await Assert.ThrowsAsync<Exception>(() => _FoodService.ExcelDataHandle(new List<CellFoodItem> { new CellFoodItem { Ten = "Food1", Phan_Loai = "Cat1", So_luong = 1 } }));
        }
    }
} 