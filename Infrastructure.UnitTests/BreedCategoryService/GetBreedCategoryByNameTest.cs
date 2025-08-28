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
using Domain.Dto.Response.Breed;
using Application.Wrappers;
using MockQueryable;

namespace Infrastructure.UnitTests.BreedCategoryService
{
    public class GetBreedCategoryByNameTest
    {
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedCategoryService _breedCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetBreedCategoryByNameTest()
        {
            _breedCategoryRepoMock = new Mock<IRepository<BreedCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _breedCategoryService = new Infrastructure.Services.Implements.BreedCategoryService(
                _breedCategoryRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetBreedCategoryByName_AllActive_ReturnsAll()
        {
            // Arrange
            var categories = new List<BreedCategory>
            {
                new BreedCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true },
                new BreedCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true },
                new BreedCategory { Id = Guid.NewGuid(), Name = "Inactive", Description = "desc 3", IsActive = false }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _breedCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _breedCategoryService.GetBreedCategoryByName(null, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách danh mục giống thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, c => Assert.True(c.Name.StartsWith("Category")));
        }

        //[Fact]
        //public async Task GetBreedCategoryByName_FilterByName_ReturnsFiltered()
        //{
        //    // Arrange
        //    var categories = new List<BreedCategory>
        //    {
        //        new BreedCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true },
        //        new BreedCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "desc 2", IsActive = true },
        //        new BreedCategory { Id = Guid.NewGuid(), Name = "Inactive", Description = "desc 3", IsActive = false }
        //    };
        //    var mockQueryable = categories.AsQueryable().BuildMock();
        //    _breedCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) => mockQueryable.Where(predicate));

        //    // Act
        //    var result = await _breedCategoryService.GetBreedCategoryByName("2", default);

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Equal("Lấy danh sách danh mục giống thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Category 2", result.Data[0].Name);
        //}

        [Fact]
        public async Task GetBreedCategoryByName_NoResult_ReturnsEmpty()
        {
            // Arrange
            var categories = new List<BreedCategory>
            {
                new BreedCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc 1", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _breedCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) => mockQueryable.Where(predicate));

            // Act
            var result = await _breedCategoryService.GetBreedCategoryByName("NotExist", default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách danh mục giống thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        //[Fact]
        //public async Task GetBreedCategoryByName_Exception_ReturnsError()
        //{
        //    // Arrange
        //    _breedCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>() ))
        //        .Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _breedCategoryService.GetBreedCategoryByName(null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách danh mục giống", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
