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
    public class GetMedicineByCategoryTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetMedicineByCategoryTest()
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
        public async Task GetMedicineByCategory_FilterByName_ReturnsFiltered()
        {
            var category = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1/VNR", MedicineCategory = category, MedicineCategoryId = category.Id, IsActive = true },
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Other/VNR1", MedicineCategory = category, MedicineCategoryId = category.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
                .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
            var result = await _MedicineService.GetMedicineByCategory("Medicine1/VNR", null, default);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            Assert.Equal("Medicine1", result.Data[0].MedicineName);
        }

        [Fact]
        public async Task GetMedicineByCategory_FilterByCategoryId_ReturnsFiltered()
        {
            var category1 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var category2 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
            var Medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1/VNR", MedicineCategory = category1, MedicineCategoryId = category1.Id, IsActive = true },
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine2/VNR1", MedicineCategory = category2, MedicineCategoryId = category2.Id, IsActive = true }
            }.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
                .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
            var result = await _MedicineService.GetMedicineByCategory(null, category2.Id, default);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data);
            Assert.Equal("Medicine2", result.Data[0].MedicineName);
        }

        //[Fact]
        //public async Task GetMedicineByCategory_FilterByNameAndCategoryId_ReturnsFiltered()
        //{
        //    var category1 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var category2 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat2", Description = "Desc2" };
        //    var Medicines = new List<Medicine>
        //    {
        //        new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1/VNR", MedicineCategory = category1, MedicineCategoryId = category1.Id, IsActive = true },
        //        new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine2/VNR1", MedicineCategory = category2, MedicineCategoryId = category2.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
        //    _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
        //        .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
        //    var result = await _MedicineService.GetMedicineByCategory("Medicine2", category2.Id, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data);
        //    Assert.Equal("Medicine2", result.Data[0].MedicineName);
        //}

        //[Fact]
        //public async Task GetMedicineByCategory_NoResult_ReturnsEmpty()
        //{
        //    var category = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
        //    var Medicines = new List<Medicine>
        //    {
        //        new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1", MedicineCategory = category, MedicineCategoryId = category.Id, IsActive = true }
        //    }.AsQueryable().BuildMock();
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
        //    _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
        //        .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
        //    var result = await _MedicineService.GetMedicineByCategory("NotExist", null, default);
        //    Assert.True(result.Succeeded);
        //    Assert.Empty(result.Data);
        //}

        //[Fact]
        //public async Task GetMedicineByCategory_Exception_ReturnsError()
        //{
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    var result = await _MedicineService.GetMedicineByCategory(null, null, default);
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách thuốc", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
} 