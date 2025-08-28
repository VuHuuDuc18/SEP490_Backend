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
using Domain.Dto.Response.Medicine;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.MedicineService
{
    public class GetMedicineByIdTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetMedicineByIdTest()
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
        public async Task GetMedicineById_NotFound_ReturnsError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null))
                .ReturnsAsync((Medicine)null);

            // Act
            var result = await _MedicineService.GetMedicineById(id, default);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Thuốc không tồn tại hoặc đã bị xóa", result.Message);
            Assert.Contains("Thuốc không tồn tại hoặc đã bị xóa", result.Errors);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetMedicineById_Success_ReturnsData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var medicine = new Medicine { Id = id, MedicineName = "Medicine1/VNR", MedicineCategoryId = categoryId, Stock = 10, IsActive = true };
            var category = new MedicineCategory { Id = categoryId, Name = "Cat1", Description = "Desc1", IsActive = true };
            var images = new List<ImageMedicine>
            {
                new ImageMedicine { MedicineId = id, ImageLink = "img1.jpg", Thumnail = "true", IsActive = true },
                new ImageMedicine { MedicineId = id, ImageLink = "img2.jpg", Thumnail = "false", IsActive = true }
            };

            _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null))
                .ReturnsAsync(medicine);
            _MedicineCategoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, null))
                .ReturnsAsync(category);
            var imageMock = images.AsQueryable().BuildMock();
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<ImageMedicine, bool>> predicate) => imageMock.Where(predicate));

            // Act
            var result = await _MedicineService.GetMedicineById(id, default);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded, $"Test failed: {result.Message}. Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy thông tin thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(id, result.Data.Id);
            Assert.Equal("Medicine1", result.Data.MedicineName);
            Assert.Equal("VNR", result.Data.MedicineCode);
            Assert.Equal("Cat1", result.Data.MedicineCategory.Name);
            Assert.Equal("img1.jpg", result.Data.Thumbnail);
            Assert.Contains("img2.jpg", result.Data.ImageLinks);
        }

        //[Fact]
        //public async Task GetMedicineById_Success_NoImages()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    var categoryId = Guid.NewGuid();
        //    var medicine = new Medicine { Id = id, MedicineName = "Medicine1/VNR", MedicineCategoryId = categoryId, Stock = 10, IsActive = true };
        //    var category = new MedicineCategory { Id = categoryId, Name = "Cat1", Description = "Desc1", IsActive = true };

        //    _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null))
        //        .ReturnsAsync(medicine);
        //    _MedicineCategoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, null))
        //        .ReturnsAsync(category);
        //    var imageMock = new List<ImageMedicine>().AsQueryable().BuildMock();
        //    _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((System.Linq.Expressions.Expression<Func<ImageMedicine, bool>> predicate) => imageMock.Where(predicate));

        //    // Act
        //    var result = await _MedicineService.GetMedicineById(id, default);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.Succeeded, $"Test failed: {result.Message}. Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
        //    Assert.Equal("Lấy thông tin thuốc thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(id, result.Data.Id);
        //    Assert.Equal("Medicine1", result.Data.MedicineName);
        //    Assert.Equal("VNR", result.Data.MedicineCode);
        //    Assert.Equal("Cat1", result.Data.MedicineCategory.Name);
        //    Assert.Null(result.Data.Thumbnail);
        //    Assert.Empty(result.Data.ImageLinks);
        //}

        //[Fact]
        //public async Task GetMedicineById_Exception_ReturnsError()
        //{
        //    // Arrange
        //    var id = Guid.NewGuid();
        //    _MedicineRepositoryMock.Setup(x => x.GetByIdAsync(id, null))
        //        .ThrowsAsync(new Exception("DB error"));

        //    // Act
        //    var result = await _MedicineService.GetMedicineById(id, default);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy thông tin thuốc", result.Message);
        //    Assert.Contains("DB error", result.Errors);
        //    Assert.Null(result.Data);
        //}
    }
}