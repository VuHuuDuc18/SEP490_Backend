using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Medicine;
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

namespace Infrastructure.UnitTests.MedicineService
{
    public class ExcelDataHandleTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;
        private readonly Guid _userId = Guid.NewGuid();

        public ExcelDataHandleTest()
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
        public async Task ExcelDataHandle_AddNewMedicineAndCategory_Success()
        {
            var cell = new CellMedicineItem { Ten_Thuoc = "Medicine1",Ma_dang_ky = "VNR" , Phan_Loai_Thuoc = "Cat1", So_luong = 5 };
            var Medicines = new List<Medicine>().AsQueryable().BuildMock();
            var categories = new List<MedicineCategory>().AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
            _MedicineCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) => categories.Where(predicate));
            _MedicineCategoryRepositoryMock.Setup(x => x.Insert(It.IsAny<MedicineCategory>()));
            _MedicineCategoryRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
 
            _MedicineRepositoryMock.Setup(x => x.Insert(It.IsAny<Medicine>()));
            _MedicineRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
            var result = await _MedicineService.ExcelDataHandle(new List<CellMedicineItem> { cell });
         //   Assert.True(result);
            _MedicineCategoryRepositoryMock.Verify(x => x.Insert(It.IsAny<MedicineCategory>()), Times.Once());
            _MedicineRepositoryMock.Verify(x => x.Insert(It.IsAny<Medicine>()), Times.Once());
        }

        //[Fact]
        //public async Task ExcelDataHandle_IncrementStock_Success()
        //{
        //    var cell = new CellMedicineItem { Ten_Thuoc = "Medicine1", Ma_dang_ky = "VNR", Phan_Loai_Thuoc = "Cat1", So_luong = 3 };
        //    var category = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Cat1" };
        //    var Medicine = new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1/VNR", MedicineCategoryId = category.Id, Stock = 2, IsActive = true };
        //    var Medicines = new List<Medicine> { Medicine }.AsQueryable().BuildMock();
        //    var categories = new List<MedicineCategory> { category }.AsQueryable().BuildMock();
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<Medicine, bool>> predicate) => Medicines.Where(predicate));
        //    _MedicineCategoryRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) => categories.Where(predicate));
        //    _MedicineRepositoryMock.Setup(x => x.CommitAsync(default)).ReturnsAsync(1);
        //    var result = await _MedicineService.ExcelDataHandle(new List<CellMedicineItem> { cell });
        //  //  Assert.True(result);
        //    Assert.Equal(5, Medicine.Stock);
        //}

        //[Fact]
        //public async Task ExcelDataHandle_Exception_ReturnsError()
        //{
        //    _MedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>() ))
        //        .Throws(new Exception("DB error"));
        //    await Assert.ThrowsAsync<Exception>(() => _MedicineService.ExcelDataHandle(new List<CellMedicineItem> { new CellMedicineItem { Ten_Thuoc = "Medicine1", Ma_dang_ky = "VNR", Phan_Loai_Thuoc = "Cat1", So_luong = 5 } }));
        //}
    }
} 