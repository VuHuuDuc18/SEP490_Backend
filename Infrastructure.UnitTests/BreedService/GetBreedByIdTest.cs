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
using Domain.Dto.Response.Breed;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.BreedService
{
    public class GetBreedByIdTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetBreedByIdTest()
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
        public async Task GetBreedById_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            var breeds = new List<Breed>().AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            var result = await _breedService.GetBreedById(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Giống loài không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Giống loài không tồn tại hoặc đã bị xóa", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetBreedById_Success_ReturnsData()
        {
            var id = Guid.NewGuid();
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var breed = new Breed { Id = id, BreedName = "Breed1", BreedCategory = category, Stock = 10, IsActive = true };
            var breeds = new List<Breed> { breed }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            var images = new List<ImageBreed>
            {
                new ImageBreed { BreedId = id, ImageLink = "img1.jpg", Thumnail = "true" },
                new ImageBreed { BreedId = id, ImageLink = "img2.jpg", Thumnail = "false" }
            };
            var imageMock = images.AsQueryable().BuildMock();
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<ImageBreed, bool>> predicate) => imageMock.Where(predicate));
            var result = await _breedService.GetBreedById(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin giống loài thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            Assert.Equal("Breed1", result.Data.BreedName);
            Assert.Equal("Cat1", result.Data.BreedCategory.Name);
            Assert.Equal("img1.jpg", result.Data.Thumbnail);
            Assert.Contains("img2.jpg", result.Data.ImageLinks);
        }

        //[Fact]
        //public async Task GetBreedById_Success_NoImages()
        //{
        //    var id = Guid.NewGuid();
        //    var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var breed = new Breed { Id = id, BreedName = "Breed1", BreedCategory = category, Stock = 10, IsActive = true };
        //    var breeds = new List<Breed> { breed }.AsQueryable().BuildMock();
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
        //    var imageMock = new List<ImageBreed>().AsQueryable().BuildMock();
        //    _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<ImageBreed, bool>> predicate) => imageMock.Where(predicate));
        //    var result = await _breedService.GetBreedById(id, default);
        //    Assert.True(result.Succeeded);
        //    Assert.NotNull(result.Data);
        //    Assert.Null(result.Data.Thumbnail);
        //    Assert.Empty(result.Data.ImageLinks);
        //}

        //[Fact]
        //public async Task GetBreedById_Exception_ReturnsError()
        //{
        //    var id = Guid.NewGuid();
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    var result = await _breedService.GetBreedById(id, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy thông tin giống loài", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //    Assert.Null(result.Data);
        //}
    }
} 