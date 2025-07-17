using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable; // Thêm namespace này

namespace Infrastructure.UnitTests.BreedCategoryService
{
    public class GetAllBreedCategoryTest
    {
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedCategoryService _breedCategoryService;

        public GetAllBreedCategoryTest()
        {
            _breedCategoryRepoMock = new Mock<IRepository<BreedCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim> { new Claim("uid", userId) };
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
        public async Task GetAllCategory_ShouldReturnActiveCategories()
        {
            // Arrange
            var categories = new List<BreedCategory>
            {
                new BreedCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1", IsActive = true },
                new BreedCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "Desc 2", IsActive = true },
                new BreedCategory { Id = Guid.NewGuid(), Name = "Inactive Category", Description = "Desc 3", IsActive = false }
            };

            // Sử dụng MockQueryable.Moq để tạo IQueryable hỗ trợ bất đồng bộ
            var mockQueryable = categories.AsQueryable().BuildMock();
            _breedCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BreedCategory, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<BreedCategory, bool>> predicate) =>
                    mockQueryable.Where(predicate));

            // Act
            var result = await _breedCategoryService.GetAllCategory();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.DoesNotContain("Inactive", c.Name));
        }
    }
}