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
    public class GetBreedByCategoryTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetBreedByCategoryTest()
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
        public async Task GetBreedByCategory_FilterByName_ReturnsFiltered()
        {
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var breeds = new List<Breed>
            {
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategory = category, BreedCategoryId = category.Id, IsActive = true },
                new Breed { Id = Guid.NewGuid(), BreedName = "Other", BreedCategory = category, BreedCategoryId = category.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
            var result = await _breedService.GetBreedByCategory("Breed1", null, default);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            Assert.Equal("Breed1", result.Data[0].BreedName);
        }

        [Fact]
        public async Task GetBreedByCategory_FilterByCategoryId_ReturnsFiltered()
        {
            var category1 = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var category2 = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
            var breeds = new List<Breed>
            {
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategory = category1, BreedCategoryId = category1.Id, IsActive = true },
                new Breed { Id = Guid.NewGuid(), BreedName = "Breed2", BreedCategory = category2, BreedCategoryId = category2.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
                .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
            var result = await _breedService.GetBreedByCategory(null, category2.Id, default);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            Assert.Equal("Breed2", result.Data[0].BreedName);
        }

        //[Fact]
        //public async Task GetBreedByCategory_FilterByNameAndCategoryId_ReturnsFiltered()
        //{
        //    var category1 = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var category2 = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
        //    var breeds = new List<Breed>
        //    {
        //        new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategory = category1, BreedCategoryId = category1.Id, IsActive = true },
        //        new Breed { Id = Guid.NewGuid(), BreedName = "Breed2", BreedCategory = category2, BreedCategoryId = category2.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
        //    _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
        //        .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
        //    var result = await _breedService.GetBreedByCategory("Breed2", category2.Id, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Breed2", result.Data[0].BreedName);
        //}

        //[Fact]
        //public async Task GetBreedByCategory_NoResult_ReturnsEmpty()
        //{
        //    var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var breeds = new List<Breed>
        //    {
        //        new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategory = category, BreedCategoryId = category.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
        //    _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>() ))
        //        .Returns(new List<ImageBreed>().AsQueryable().BuildMock());
        //    var result = await _breedService.GetBreedByCategory("NotExist", null, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Empty(result.Data);
        //}

        //[Fact]
        //public async Task GetBreedByCategory_Exception_ReturnsError()
        //{
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    var result = await _breedService.GetBreedByCategory(null, null, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách giống loài", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
} 