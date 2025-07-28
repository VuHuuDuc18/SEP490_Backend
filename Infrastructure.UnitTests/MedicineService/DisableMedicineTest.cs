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
using Infrastructure.Services;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.MedicineService
{
    public class DisableMedicineTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;
        private readonly Guid _userId = Guid.NewGuid();

        public DisableMedicineTest()
        {
            _MedicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _imageMedicineRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _MedicineCategoryRepositoryMock = new Mock<IRepository<MedicineCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("uid", _userId.ToString()) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _MedicineService = new Infrastructure.Services.Implements.MedicineService(
                _MedicineRepositoryMock.Object,
                _MedicineCategoryRepositoryMock.Object,
                _imageMedicineRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task DisableMedicine_NotFound_ReturnsError()
        {
            var id = Guid.NewGuid();
            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync((Medicine?)null);
            var result = await _MedicineService.DisableMedicine(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Thuốc không tồn tại", result.Message);
            Assert.Contains("Thuốc không tồn tại", result.Errors);
        }

        [Fact]
        public async Task DisableMedicine_Success_Disable()
        {
            var id = Guid.NewGuid();
            var existing = new Medicine { Id = id, MedicineName = "Medicine1", IsActive = true };
            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _MedicineRepositoryMock.Setup(x => x.Update(It.IsAny<Medicine>()));
            _MedicineRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _MedicineService.DisableMedicine(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Xóa thuốc thành công", result.Message);
            Assert.Contains("Thuốc đã được xóa thành công. ID:", result.Data);
            _MedicineRepositoryMock.Verify(x => x.Update(It.IsAny<Medicine>()), Times.Once());
            _MedicineRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.False(existing.IsActive);
        }

        [Fact]
        public async Task DisableMedicine_Success_EnableAgain()
        {
            var id = Guid.NewGuid();
            var existing = new Medicine { Id = id, MedicineName = "Medicine1", IsActive = false };
            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _MedicineRepositoryMock.Setup(x => x.Update(It.IsAny<Medicine>()));
            _MedicineRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var result = await _MedicineService.DisableMedicine(id, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Khôi phục thuốc thành công", result.Message);
            Assert.Contains("Thuốc đã được khôi phục thành công. ID:", result.Data);
            _MedicineRepositoryMock.Verify(x => x.Update(It.IsAny<Medicine>()), Times.Once());
            _MedicineRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.True(existing.IsActive);
        }

        [Fact]
        public async Task DisableMedicine_Exception_ReturnsError()
        {
            var id = Guid.NewGuid();
            var existing = new Medicine { Id = id, MedicineName = "Medicine1", IsActive = true };
            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null)).ReturnsAsync(existing);
            _MedicineRepositoryMock.Setup(x => x.Update(It.IsAny<Medicine>())).Throws(new Exception("DB error"));
            var result = await _MedicineService.DisableMedicine(id, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi xóa thuốc", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
} 