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
using Domain.Dto.Request;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response;
using Application.Wrappers;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.FoodCategoryService
{
    public class GetPaginatedFoodCategoryListTest
    {
        private readonly Mock<IRepository<FoodCategory>> _FoodCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.FoodCategoryService _FoodCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetPaginatedFoodCategoryListTest()
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
        public async Task GetPaginatedFoodCategoryList_RequestNull_ThrowsError()
        {
            // Act
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(null, default);
            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null", result.Message);
            Assert.Contains("Yêu cầu không được null", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_PageIndexInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_PageSizeInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_InvalidFilterField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } },
                Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" }
            };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_InvalidSortField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_Success_ReturnsPaginatedData()
        {
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true },
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable()).Returns(mockQueryable);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" }
            };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_Success_WithSearch()
        {
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Searchable", Description = "desc 1", IsActive = true },
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable()).Returns(mockQueryable);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Searchable" } }
            };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.Name == "Searchable");
        }

        [Fact]
        public async Task GetPaginatedFoodCategoryList_Success_WithFilter()
        {
            var categories = new List<FoodCategory>
            {
                new FoodCategory { Id = Guid.NewGuid(), Name = "Filterable", Description = "desc 1", IsActive = true },
                new FoodCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _FoodCategoryRepoMock.Setup(x => x.GetQueryable()).Returns(mockQueryable);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Filterable" } }
            };
            var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.Name == "Filterable");
        }

        //[Fact]
        //public async Task GetPaginatedFoodCategoryList_Exception_ReturnsError()
        //{
        //    _FoodCategoryRepoMock.Setup(x => x.GetQueryable()).Throws(new Exception("DB error"));
        //    var req = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" }
        //    };
        //    var result = await _FoodCategoryService.GetPaginatedFoodCategoryList(req, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách phân trang", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
