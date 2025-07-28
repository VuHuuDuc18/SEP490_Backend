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

namespace Infrastructure.UnitTests.MedicineCategoryService
{
    public class GetAllMedicineCategoryTest
    {
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineCategoryService _MedicineCategoryService;

        public GetAllMedicineCategoryTest()
        {
            _MedicineCategoryRepoMock = new Mock<IRepository<MedicineCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim> { new Claim("uid", userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _MedicineCategoryService = new Infrastructure.Services.Implements.MedicineCategoryService(
                _MedicineCategoryRepoMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllCategory_ShouldReturnActiveCategories()
        {
            // Arrange
            var categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1", IsActive = true },
                new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "Desc 2", IsActive = true },
                new MedicineCategory { Id = Guid.NewGuid(), Name = "Inactive Category", Description = "Desc 3", IsActive = false }
            };

            // Sử dụng MockQueryable.Moq để tạo IQueryable hỗ trợ bất đồng bộ
            var mockQueryable = categories.AsQueryable().BuildMock();
            _MedicineCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) =>
                    mockQueryable.Where(predicate));

            // Act
            var result = await _MedicineCategoryService.GetAllMedicineCategory();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.DoesNotContain("Inactive", c.Name));
        }
    }
}