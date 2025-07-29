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
using Domain.Dto.Request.Category;
using Application.Wrappers;

namespace Infrastructure.UnitTests.MedicineCategoryService
{
    public class DisableMedicineCategoryTest
    {
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineCategoryService _MedicineCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableMedicineCategoryTest()
        {
            _MedicineCategoryRepoMock = new Mock<IRepository<MedicineCategory>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock user context
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
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
        public async Task DisableMedicineCategory_NotFound_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _MedicineCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((MedicineCategory)null);

            // Act
            var result = await _MedicineCategoryService.DisableMedicineCategory(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Danh mục thuốc không tồn tại", result.Message);
            Assert.Contains("Danh mục thuốc không tồn tại", result.Errors);
        }

        [Fact]
        public async Task DisableMedicineCategory_Success_Disable()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new MedicineCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
            _MedicineCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _MedicineCategoryRepoMock.Setup(x => x.Update(It.IsAny<MedicineCategory>()));
            _MedicineCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _MedicineCategoryService.DisableMedicineCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa danh mục thuốc thành công", result.Message);
            Assert.Contains("Danh mục thuốc đã được xóa thành công. ID:", result.Data);
            _MedicineCategoryRepoMock.Verify(x => x.Update(It.IsAny<MedicineCategory>()), Times.Once());
            _MedicineCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive); // Đã bị disable
        }

        [Fact]
        public async Task DisableMedicineCategory_Success_EnableAgain()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new MedicineCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = false };
            _MedicineCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _MedicineCategoryRepoMock.Setup(x => x.Update(It.IsAny<MedicineCategory>()));
            _MedicineCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _MedicineCategoryService.DisableMedicineCategory(id, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục danh mục thuốc thành công", result.Message);
            Assert.Contains("Danh mục thuốc đã được khôi phục thành công. ID:", result.Data);
            _MedicineCategoryRepoMock.Verify(x => x.Update(It.IsAny<MedicineCategory>()), Times.Once());
            _MedicineCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive); // Đã được enable lại
        }

        [Fact]
        public async Task DisableMedicineCategory_Exception_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new MedicineCategory { Id = id, Name = "Category 1", Description = "desc", IsActive = true };
            _MedicineCategoryRepoMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync(existing);
            _MedicineCategoryRepoMock.Setup(x => x.Update(It.IsAny<MedicineCategory>())).Throws(new Exception("DB error"));

            // Act
            var result = await _MedicineCategoryService.DisableMedicineCategory(id, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi xóa danh mục thuốc", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
}
