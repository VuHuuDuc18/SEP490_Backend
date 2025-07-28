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
using Domain.Dto.Request;
using Domain.Dto.Response.Medicine;
using Domain.Dto.Response;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.UnitTests.MedicineService
{
    public class GetPaginatedMedicineListTest
    {
        private readonly Mock<IRepository<Medicine>> _MedicineRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepositoryMock;
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineService _MedicineService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetPaginatedMedicineListTest()
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
        public async Task GetPaginatedMedicineList_RequestNull_ThrowsError()
        {
            var result = await _MedicineService.GetPaginatedMedicineList(null, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null", result.Message);
            Assert.Contains("Yêu cầu không được null", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_PageIndexInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_PageSizeInvalid_ThrowsError()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", result.Errors);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_InvalidFilterField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } },
                Sort = new SearchObjectForCondition { Field = "MedicineName", Value = "asc" }
            };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_InvalidSortField_ThrowsError()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_Success_ReturnsPaginatedData()
        {
            var category = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine1/VNR1", MedicineCategory = category, IsActive = true },
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine2/VNR2", MedicineCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable()).Returns(Medicines);
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
                .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "MedicineName", Value = "asc" }
            };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy danh sách phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_Success_WithSearch()
        {
            var category = new MedicineCategory { Id = Guid.NewGuid(), Name = "Cat1", Description = "Desc1" };
            var Medicines = new List<Medicine>
            {
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Searchable/VNR1", MedicineCategory = category, IsActive = true },
                new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine2//VNR2", MedicineCategory = category, IsActive = true }
            }.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable()).Returns(Medicines);
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>() ))
                .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "MedicineName", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "MedicineName", Value = "Searchable" } }
            };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.True(result.Succeeded);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.MedicineName == "Searchable");
        }

        [Fact]
        public async Task GetPaginatedMedicineList_Success_WithFilter()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var category = new MedicineCategory { Id = categoryId, Name = "Cat1", Description = "Desc1", IsActive = true };
            var medicines = new List<Medicine>
    {
        new Medicine { Id = medicineId, MedicineName = "Filterable/VNR1", MedicineCategoryId = categoryId, IsActive = true },
        new Medicine { Id = Guid.NewGuid(), MedicineName = "Medicine2/VNR2", MedicineCategoryId = categoryId, IsActive = true }
    };
            var medicineMock = medicines.AsQueryable().BuildMock();
            _MedicineRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(() => medicines.Where(m => m.MedicineName.Contains("Filterable")).AsQueryable().BuildMock());
            _MedicineCategoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
                .ReturnsAsync((Guid id, CancellationToken ct) => category);
            var imageMock = new List<ImageMedicine>().AsQueryable().BuildMock();
            _imageMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<ImageMedicine, bool>> predicate) => imageMock.Where(predicate));

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "MedicineName", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "MedicineName", Value = "Filterable/VNR1" } }
            };

            // Act
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded, $"Test failed: {result.Message}. Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.MedicineName == "Filterable");
            Assert.Equal("VNR1", result.Data.Items.First().MedicineCode);
            Assert.Equal("Cat1", result.Data.Items.First().MedicineCategory.Name);
        }

        [Fact]
        public async Task GetPaginatedMedicineList_Exception_ReturnsError()
        {
            _MedicineRepositoryMock.Setup(x => x.GetQueryable()).Throws(new Exception("DB error"));
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "MedicineName", Value = "asc" }
            };
            var result = await _MedicineService.GetPaginatedMedicineList(req, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy danh sách phân trang", result.Message);
            Assert.Contains("DB error", result.Errors[0]);
        }
    }
} 