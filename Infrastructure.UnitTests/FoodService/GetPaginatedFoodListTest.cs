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
using Domain.Dto.Request;
using Domain.Dto.Response.Food;
using Domain.Dto.Response;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.FoodService
{
    public class GetPaginatedFoodListTest
    {
        private readonly Mock<IRepository<Food>> _FoodRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepositoryMock;
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodService _FoodService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetPaginatedFoodListTest()
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
        public async Task GetPaginatedFoodList_RequestNull_ThrowsError()
        {
            var result = await _FoodService.GetPaginatedFoodList(null, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null", result.Message);
            Assert.Contains("Yêu cầu không được null", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodList_PageIndexInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodList_PageSizeInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodList_InvalidFilterField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } },
                Sort = new SearchObjectForCondition { Field = "FoodName", Value = "asc" }
            };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedFoodList_InvalidSortField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedFoodList_Success_ReturnsPaginatedData()
        {
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Foods = new List<Food>
            {
                new Food { Id = Guid.NewGuid(), FoodName = "Food1", FoodCategory = category, IsActive = true },
                new Food { Id = Guid.NewGuid(), FoodName = "Food2", FoodCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable()).Returns(Foods);
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns(new List<ImageFood>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "FoodName", Value = "asc" }
            };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
        }

        [Fact]
        public async Task GetPaginatedFoodList_Success_WithSearch()
        {
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Foods = new List<Food>
            {
                new Food { Id = Guid.NewGuid(), FoodName = "Searchable", FoodCategory = category, IsActive = true },
                new Food { Id = Guid.NewGuid(), FoodName = "Food2", FoodCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable()).Returns(Foods);
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns(new List<ImageFood>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "FoodName", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "FoodName", Value = "Searchable" } }
            };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.FoodName == "Searchable");
        }

        [Fact]
        public async Task GetPaginatedFoodList_Success_WithFilter()
        {
            var category = new FoodCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Foods = new List<Food>
            {
                new Food { Id = Guid.NewGuid(), FoodName = "Filterable", FoodCategory = category, IsActive = true },
                new Food { Id = Guid.NewGuid(), FoodName = "Food2", FoodCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _FoodRepositoryMock.Setup(x => x.GetQueryable()).Returns(Foods);
            _imageFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>() ))
                .Returns(new List<ImageFood>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "FoodName", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "FoodName", Value = "Filterable" } }
            };
            var result = await _FoodService.GetPaginatedFoodList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.FoodName == "Filterable");
        }

        //[Fact]
        //public async Task GetPaginatedFoodList_Exception_ReturnsError()
        //{
        //    _FoodRepositoryMock.Setup(x => x.GetQueryable()).Throws(new Exception("DB error"));
        //    var req = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "FoodName", Value = "asc" }
        //    };
        //    var result = await _FoodService.GetPaginatedFoodList(req, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách phân trang", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
} 