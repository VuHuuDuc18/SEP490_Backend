using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Breed;
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

namespace Infrastructure.UnitTests.BreedService
{
    public class ExcelDataHandleTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;
        private readonly Guid _userId = Guid.NewGuid();

        public ExcelDataHandleTest()
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
        public async Task ExcelDataHandle_AddNewBreedAndCategory_Success()
        {
            var cell = new CellBreedItem { Ten = "Breed1", Phan_Loai = "Cat1", So_luong = 5 };
            var breeds = new List<Breed>().AsQueryable().BuildMock();
            var categories = new List<BreedCategory>().AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            _breedCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) => categories.Where(predicate));
            _breedCategoryRepositoryMock.Setup(x => x.Insert(It.IsAny<BreedCategory>()));
            _breedCategoryRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
 
            _breedRepositoryMock.Setup(x => x.Insert(It.IsAny<Breed>()));
            _breedRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _breedService.ExcelDataHandle(new List<CellBreedItem> { cell });
           // Assert.True(result);
            _breedCategoryRepositoryMock.Verify(x => x.Insert(It.IsAny<BreedCategory>()), Times.Once());
            _breedRepositoryMock.Verify(x => x.Insert(It.IsAny<Breed>()), Times.Once());
        }

        [Fact]
        public async Task ExcelDataHandle_IncrementStock_Success()
        {
            var cell = new CellBreedItem { Ten = "Breed1", Phan_Loai = "Cat1", So_luong = 3 };
            var category = new BreedCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Cat1" };
            var breed = new Breed { Id = Guid.NewGuid(), BreedName = "Breed1", BreedCategoryId = category.Id, Stock = 2, IsActive = true };
            var breeds = new List<Breed> { breed }.AsQueryable().BuildMock();
            var categories = new List<BreedCategory> { category }.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Breed, bool>> predicate) => breeds.Where(predicate));
            _breedCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) => categories.Where(predicate));
            _breedRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _breedService.ExcelDataHandle(new List<CellBreedItem> { cell });
           // Assert.True(result);
            Assert.Equal(5, breed.Stock);
        }

        //[Fact]
        //public async Task ExcelDataHandle_Exception_ReturnsError()
        //{
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Breed, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    await Assert.ThrowsAsync<Exception>(() => _breedService.ExcelDataHandle(new List<CellBreedItem> { new CellBreedItem { Ten = "Breed1", Phan_Loai = "Cat1", So_luong = 1 } }));
        //}
    }
} 