using Domain.Dto.Response;
using Domain.Dto.Response.Medicine;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.MedicineService
{
    public class GetAllMedicineTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;

        public GetAllMedicineTest()
        {
            _MedicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _imageMedicineRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _MedicineCategoryRepositoryMock = new Mock<IRepository<MedicineCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock HttpContext để lấy userId
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("uid", Guid.NewGuid().ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(user);

            // Mock IOptions<CloudinaryConfig>
            var cloudinaryConfigMock = new Mock<IOptions<Domain.Settings.CloudinaryConfig>>();
            cloudinaryConfigMock.Setup(x => x.Value).Returns(new Domain.Settings.CloudinaryConfig
            {
                CloudName = "dpgk5pqt9",
                ApiKey = "382542864398655",
                ApiSecret = "ct6gqlmsftVgmj2C3A8tYoiQk0M"
            });

            // Khởi tạo CloudinaryCloudService với mock config
            var cloudinaryService = new CloudinaryCloudService(cloudinaryConfigMock.Object);

            _MedicineService = new Infrastructure.Services.Implements.MedicineService(
                _MedicineRepositoryMock.Object,
                _MedicineCategoryRepositoryMock.Object,
                _imageMedicineRepositoryMock.Object,
                cloudinaryService,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllMedicine_ReturnsListOfMedicines_WhenMedicinesExist()
        {
            // Arrange
            var medicineCategoryId1 = Guid.NewGuid();
            var medicineCategoryId2 = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();

            var medicines = new List<Medicine>
    {
        new Medicine
        {
            Id = medicineId1,
            MedicineName = "Medicine1/VNR",
            MedicineCategoryId = medicineCategoryId1,
            Stock = 10,
            IsActive = true
        },
        new Medicine
        {
            Id = medicineId2,
            MedicineName = "Medicine2/VNR2",
            MedicineCategoryId = medicineCategoryId2,
            Stock = 20,
            IsActive = true
        }
    };

            var categories = new List<MedicineCategory>
    {
        new MedicineCategory { Id = medicineCategoryId1, Name = "Category1", Description = "Desc1", IsActive = true },
        new MedicineCategory { Id = medicineCategoryId2, Name = "Category2", Description = "Desc2", IsActive = true }
    };

            var images = new List<ImageMedicine>
    {
        new ImageMedicine { MedicineId = medicineId1, ImageLink = "image1.jpg", Thumnail = "true", IsActive = true },
        new ImageMedicine { MedicineId = medicineId1, ImageLink = "image2.jpg", Thumnail = "false", IsActive = true },
        new ImageMedicine { MedicineId = medicineId2, ImageLink = "image3.jpg", Thumnail = "true", IsActive = true }
    };

            var medicineMock = medicines.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> predicate) => medicineMock.Where(predicate));

            var imageMock = images.AsQueryable().BuildMock();
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> predicate) => imageMock.Where(predicate));

            _MedicineCategoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
                .ReturnsAsync((Guid id, CancellationToken ct) => categories.FirstOrDefault(c => c.Id == id));

            // Act
            var result = await _MedicineService.GetAllMedicine(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded, $"Test failed: {result.Message}. Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);

            var medicine1 = result.Data[0];
            Assert.Equal(medicineId1, medicine1.Id);
            Assert.Equal("Medicine1", medicine1.MedicineName);
            Assert.Equal("VNR", medicine1.MedicineCode);
            Assert.Equal("Category1", medicine1.MedicineCategory.Name);
            Assert.Equal("image1.jpg", medicine1.Thumbnail);
            Assert.Contains("image2.jpg", medicine1.ImageLinks);

            var medicine2 = result.Data[1];
            Assert.Equal(medicineId2, medicine2.Id);
            Assert.Equal("Medicine2", medicine2.MedicineName);
            Assert.Equal("VNR2", medicine2.MedicineCode);
            Assert.Equal("Category2", medicine2.MedicineCategory.Name);
            Assert.Equal("image3.jpg", medicine2.Thumbnail);
            Assert.Empty(medicine2.ImageLinks);
        }

        [Fact]
        public async Task GetAllMedicine_ReturnsEmptyList_WhenNoMedicinesExist()
        {
            // Arrange
            var medicines = new List<Medicine>();
            var medicineMock = medicines.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> predicate) => medicineMock.Where(predicate));

            var imageMock = Enumerable.Empty<ImageMedicine>().AsQueryable().BuildMock();
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> predicate) => imageMock.Where(predicate));

            _MedicineCategoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
                .ReturnsAsync((Guid id, CancellationToken ct) => null); 

            // Act
            var result = await _MedicineService.GetAllMedicine(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded, $"Test failed: {result.Message}. Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        //[Fact]
        //public async Task GetAllMedicine_ThrowsException_WhenRepositoryFails()
        //{
        //    // Arrange
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<Exception>(() => _MedicineService.GetAllMedicine(CancellationToken.None));
        //    Assert.Equal("Database error", exception.Message);
        //}
    }
}