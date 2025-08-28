using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response.DailyReport;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using MockQueryable.Moq;
using Infrastructure.Services;
using MockQueryable;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.DailyReportService
{
    public class GetDailyReportByIdTest
    {
        private readonly Mock<IRepository<DailyReport>> _dailyReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<FoodReport>> _foodReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<MedicineReport>> _medicineReportRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<ImageDailyReport>> _imageDailyReportRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.DailyReportService _dailyReportService;
        private readonly Guid _userId = Guid.NewGuid();

        public GetDailyReportByIdTest()
        {
            _dailyReportRepositoryMock = new Mock<IRepository<DailyReport>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _foodReportRepositoryMock = new Mock<IRepository<FoodReport>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _medicineReportRepositoryMock = new Mock<IRepository<MedicineReport>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _imageDailyReportRepositoryMock = new Mock<IRepository<ImageDailyReport>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _foodImageRepositoryMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(null);
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _dailyReportService = new Infrastructure.Services.Implements.DailyReportService(
                _dailyReportRepositoryMock.Object,
                _livestockCircleRepositoryMock.Object,
                _foodReportRepositoryMock.Object,
                _livestockCircleFoodRepositoryMock.Object,
                _medicineReportRepositoryMock.Object,
                _livestockCircleMedicineRepositoryMock.Object,
                _imageDailyReportRepositoryMock.Object,
                _foodRepositoryMock.Object,
                _medicineRepositoryMock.Object,
                _foodImageRepositoryMock.Object,
                _medicineImageRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        //[Fact]
        //public async Task GetDailyReportById_EmptyId_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.Empty;

        //    // Act
        //    var result = await _dailyReportService.GetDailyReportById(reportId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("ID báo cáo không hợp lệ", result.Message);
        //    Assert.Contains("ID báo cáo không hợp lệ", result.Errors);
        //}

        [Fact]
        public async Task GetDailyReportById_NonExistentReport_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync((DailyReport)null);

            // Act
            var result = await _dailyReportService.GetDailyReportById(reportId, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
            Assert.Contains("Không tìm thấy báo cáo hàng ngày", result.Errors);
        }

        //[Fact]
        //public async Task GetDailyReportById_InactiveReport_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var report = new DailyReport
        //    {
        //        Id = reportId,
        //        CreatedBy = _userId,
        //        IsActive = false,
        //        LivestockCircleId = Guid.NewGuid(),
        //        DeadUnit = 1,
        //        GoodUnit = 8,
        //        BadUnit = 1,
        //        Note = "Test note",
        //        AgeInDays = 5,
        //        Status = "Normal",
        //        //CreatedAt = DateTime.UtcNow
        //    };
        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);

        //    // Act
        //    var result = await _dailyReportService.GetDailyReportById(reportId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
        //    Assert.Contains("Không tìm thấy báo cáo hàng ngày", result.Errors);
        //}

        //[Fact]
        //public async Task GetDailyReportById_UnauthorizedUser_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var differentUserId = Guid.NewGuid();
        //    var report = new DailyReport
        //    {
        //        Id = reportId,
        //        CreatedBy = differentUserId,
        //        IsActive = true,
        //        LivestockCircleId = Guid.NewGuid(),
        //        DeadUnit = 1,
        //        GoodUnit = 8,
        //        BadUnit = 1,
        //        Note = "Test note",
        //        AgeInDays = 5,
        //        Status = "Normal",
        //        //CreatedAt = DateTime.UtcNow
        //    };
        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);

        //    // Act
        //    var result = await _dailyReportService.GetDailyReportById(reportId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Bạn không có quyền truy cập báo cáo này", result.Message);
        //    Assert.Contains("Bạn không có quyền truy cập báo cáo này", result.Errors);
        //}

        [Fact]
        public async Task GetDailyReportById_Success_WithRelatedData()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow;
            var report = new DailyReport
            {
                Id = reportId,
                CreatedBy = _userId,
                IsActive = true,
                LivestockCircleId = livestockCircleId,
                DeadUnit = 1,
                GoodUnit = 8,
                BadUnit = 1,
                Note = "Test note",
                AgeInDays = 5,
                Status = "Normal",
                CreatedDate = createdDate
            };
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-5),
                GoodUnitNumber = 10
            };
            var foodReports = new List<FoodReport>
            {
                new FoodReport
                {
                    Id = Guid.NewGuid(),
                    FoodId = foodId,
                    ReportId = reportId,
                    Quantity = 2,
                    IsActive = true,
                    Food = new Food { Id = foodId, FoodName = "Food1", IsActive = true }
                }
            }.AsQueryable();
            var medicineReports = new List<MedicineReport>
            {
                new MedicineReport
                {
                    Id = Guid.NewGuid(),
                    MedicineId = medicineId,
                    ReportId = reportId,
                    Quantity = 3,
                    IsActive = true,
                    Medicine = new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
                }
            }.AsQueryable();
            var imageDailyReports = new List<ImageDailyReport>
            {
                new ImageDailyReport
                {
                    Id = Guid.NewGuid(),
                    DailyReportId = reportId,
                    Thumnail = "https://cloudinary.com/thumbnail.jpg",
                    ImageLink = "https://cloudinary.com/image.jpg",
                    IsActive = true
                }
            }.AsQueryable();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(livestockCircle);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
                .Returns(foodReports.Where(fr => fr.ReportId == reportId && fr.IsActive).BuildMock());
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
                .Returns(medicineReports.Where(mr => mr.ReportId == reportId && mr.IsActive).BuildMock());
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
                .Returns(imageDailyReports.Where(idr => idr.DailyReportId == reportId && idr.IsActive).BuildMock());

            // Act
            var result = await _dailyReportService.GetDailyReportById(reportId, default);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin báo cáo hàng ngày thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(reportId, result.Data.Id);
            Assert.Equal(livestockCircleId, result.Data.LivestockCircleId);
            Assert.Equal(1, result.Data.DeadUnit);
            Assert.Equal(8, result.Data.GoodUnit);
            Assert.Equal(1, result.Data.BadUnit);
            Assert.Equal("Test note", result.Data.Note);
            Assert.True(result.Data.IsActive);
            Assert.Equal(5, result.Data.AgeInDays);
            Assert.Equal(createdDate, result.Data.CreatedDate);
            Assert.Single(result.Data.FoodReports);
            Assert.Single(result.Data.MedicineReports);
            //Assert.Single(result.Data.ImageLinks);
            //Assert.Equal("https://cloudinary.com/thumbnail.jpg", result.Data.Thumbnail);
            //Assert.Equal("https://cloudinary.com/image.jpg", result.Data.ImageLinks.First());
            //Assert.Equal(foodId, result.Data.FoodReports.First().Food.Id);
            //Assert.Equal(2, result.Data.FoodReports.First().Quantity);
            //Assert.True(result.Data.FoodReports.First().IsActive);
            //Assert.Equal("Food1", result.Data.FoodReports.First().Food.FoodName);
            //Assert.Equal(medicineId, result.Data.MedicineReports.First().Medicine.Id);
            //Assert.Equal(3, result.Data.MedicineReports.First().Quantity);
            //Assert.True(result.Data.MedicineReports.First().IsActive);
            //Assert.Equal("Medicine1", result.Data.MedicineReports.First().Medicine.MedicineName);
        }

        //[Fact]
        //public async Task GetDailyReportById_Success_NoRelatedData()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var livestockCircleId = Guid.NewGuid();
        //    var report = new DailyReport
        //    {
        //        Id = reportId,
        //        CreatedBy = _userId,
        //        IsActive = true,
        //        LivestockCircleId = livestockCircleId,
        //        DeadUnit = 1,
        //        GoodUnit = 8,
        //        BadUnit = 1,
        //        Note = "Test note",
        //        AgeInDays = 5,
        //        Status = "Normal",
        //        //CreatedAt = DateTime.UtcNow
        //    };
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        IsActive = true,
        //        StartDate = DateTime.UtcNow.AddDays(-5),
        //        GoodUnitNumber = 10
        //    };
        //    var foodReports = new List<FoodReport>().AsQueryable().BuildMock();
        //    var medicineReports = new List<MedicineReport>().AsQueryable().BuildMock();
        //    var imageDailyReports = new List<ImageDailyReport>().AsQueryable().BuildMock();

        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null)).ReturnsAsync(livestockCircle);
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
        //        .Returns(foodReports);
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
        //        .Returns(medicineReports);
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns(imageDailyReports);

        //    // Act
        //    var result = await _dailyReportService.GetDailyReportById(reportId, default);

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Equal("Lấy thông tin báo cáo hàng ngày thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(reportId, result.Data.Id);
        //    Assert.Equal(livestockCircleId, result.Data.LivestockCircleId);
        //    Assert.Equal(1, result.Data.DeadUnit);
        //    Assert.Equal(8, result.Data.GoodUnit);
        //    Assert.Equal(1, result.Data.BadUnit);
        //    Assert.Equal("Test note", result.Data.Note);
        //    Assert.True(result.Data.IsActive);
        //    Assert.Equal(5, result.Data.AgeInDays);
        //    //Assert.Equal(report.CreatedAt, result.Data.CreatedDate);
        //    Assert.Empty(result.Data.FoodReports);
        //    Assert.Empty(result.Data.MedicineReports);
        //    Assert.Empty(result.Data.ImageLinks);
        //    Assert.Null(result.Data.Thumbnail);
        //}

        //[Fact]
        //public async Task GetDailyReportById_RepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
        //        .ThrowsAsync(new Exception("Database error"));

        //    // Act
        //    var result = await _dailyReportService.GetDailyReportById(reportId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy báo cáo hàng ngày", result.Message);
        //    Assert.Contains("Database error", result.Errors);
        //}
    }
}