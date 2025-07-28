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
using Domain.Dto.Response.Breed;
using Domain.Dto.Response;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.BreedService
{
    public class GetPaginatedBreedListTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetPaginatedBreedListTest()
        {
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedCategoryRepositoryMock = new Mock<IRepository<BreedCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("uid", _userId.ToString()) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _breedService = new Infrastructure.Services.Implements.BreedService(
                _breedRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _breedCategoryRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetPaginatedBreedList_RequestNull_ThrowsError()
        {
            var result = await _breedService.GetPaginatedBreedList(null, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null", result.Message);
            Assert.Contains("Yêu cầu không được null", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedBreedList_PageIndexInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedBreedList_PageSizeInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedBreedList_InvalidFilterField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } },
                Sort = new SearchObjectForCondition { Field = "BreedName", Value = "asc" }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedBreedList_InvalidSortField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedBreedList_Success_ReturnsPaginatedData()
        {
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var breeds = new List<Breed>
            {
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategory = category, IsActive = true },
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed2", BreedCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Returns(breeds);
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BreedName", Value = "asc" }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
        }

        [Fact]
        public async Task GetPaginatedBreedList_Success_WithSearch()
        {
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var breeds = new List<Breed>
            {
                new Breed { Id = Guid.NewGuid(), BreedName = "Searchable", BreedCategory = category, IsActive = true },
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed2", BreedCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Returns(breeds);
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BreedName", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "BreedName", Value = "Searchable" } }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.BreedName == "Searchable");
        }

        [Fact]
        public async Task GetPaginatedBreedList_Success_WithFilter()
        {
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var breeds = new List<Breed>
            {
                new Breed { Id = Guid.NewGuid(), BreedName = "Filterable", BreedCategory = category, IsActive = true },
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed2", BreedCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Returns(breeds);
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BreedName", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "BreedName", Value = "Filterable" } }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.BreedName == "Filterable");
        }

        [Fact]
        public async Task GetPaginatedBreedList_Exception_ReturnsError()
        {
            _breedRepositoryMock.Setup(x => x.GetQueryable()).Throws(new Exception("DB error"));
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "BreedName", Value = "asc" }
            };
            var result = await _breedService.GetPaginatedBreedList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy danh sách phân trang", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
} 