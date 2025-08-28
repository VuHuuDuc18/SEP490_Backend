using Entities.EntityModel;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;
using Domain.Dto.Request.Category;
using Application.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.MedicineCategoryService
{
    public class CreateMedicineCategoryTest
    {
        private readonly Mock<IRepository<MedicineCategory>> _MedicineCategoryRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.MedicineCategoryService _MedicineCategoryService;
        private readonly Guid _userId = Guid.NewGuid();

        public CreateMedicineCategoryTest()
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

        //[Fact]
        //public async Task CreateMedicineCategory_RequestNull_ReturnsError()
        //{
        //    // Act
        //    var result = await _MedicineCategoryService.CreateMedicineCategory(null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Dữ liệu danh mục thuốc không được null", result.Message);
        //    Assert.Contains("Dữ liệu danh mục thuốc không được null", result.Errors);
        //}

        [Fact]
        public async Task CreateMedicineCategory_NameBlank_ReturnsValidationError()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "", Description = "desc" };

            // Act
            var result = await _MedicineCategoryService.CreateMedicineCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Assert.Contains("Tên danh mục là bắt buộc.", result.Errors);
        }

        [Fact]
        public async Task CreateMedicineCategory_NameDuplicate_ReturnsError()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Category 1", Description = "desc" };
            var categories = new List<MedicineCategory>
            {
                new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "desc", IsActive = true }
            };
            var mockQueryable = categories.AsQueryable().BuildMock();
            _MedicineCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) => mockQueryable.Where(predicate));
            // XÓA setup AnyAsync

            // Act
            var result = await _MedicineCategoryService.CreateMedicineCategory(request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("đã tồn tại", result.Message);
            Assert.Contains("đã tồn tại", result.Errors[0]);
        }

        [Fact]
        public async Task CreateMedicineCategory_Success_ReturnsOk()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Category 2", Description = "desc" };
            var categories = new List<MedicineCategory>().AsQueryable();
            var mockQueryable = categories.BuildMock();
            _MedicineCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>() ))
                .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) => mockQueryable.Where(predicate));
            // XÓA setup AnyAsync
            _MedicineCategoryRepoMock.Setup(x => x.Insert(It.IsAny<MedicineCategory>()));
            _MedicineCategoryRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _MedicineCategoryService.CreateMedicineCategory(request, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Tạo danh mục thuốc thành công", result.Message);
            Assert.Contains("Danh mục thuốc đã được tạo thành công. ID:", result.Data);
            _MedicineCategoryRepoMock.Verify(x => x.Insert(It.IsAny<MedicineCategory>()), Times.Once());
            _MedicineCategoryRepoMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        //[Fact]
        //public async Task CreateMedicineCategory_Exception_ReturnsError()
        //{
        //    // Arrange
        //    var request = new CreateCategoryRequest { Name = "Category 3", Description = "desc" };
        //    var categories = new List<MedicineCategory>().AsQueryable();
        //    var mockQueryable = categories.BuildMock();
        //    _MedicineCategoryRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineCategory, bool>>>() ))
        //        .Returns((System.Linq.Expressions.Expression<Func<MedicineCategory, bool>> predicate) => mockQueryable.Where(predicate));
        //    // XÓA setup AnyAsync
        //    _MedicineCategoryRepoMock.Setup(x => x.Insert(It.IsAny<MedicineCategory>())).Throws(new Exception("DB error"));

        //    // Act
        //    var result = await _MedicineCategoryService.CreateMedicineCategory(request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi tạo danh mục thuốc", result.Message);
        //    Assert.Contains("DB error", result.Errors[0]);
        //}
    }
}
