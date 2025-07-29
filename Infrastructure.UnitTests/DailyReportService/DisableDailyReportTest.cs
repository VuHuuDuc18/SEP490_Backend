using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using MockQueryable.Moq;
using Assert = Xunit.Assert;
using MockQueryable;

namespace Infrastructure.UnitTests.DailyReportService
{
    public class DisableDailyReportTest
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

        public DisableDailyReportTest()
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

        [Fact]
        public async Task DisableDailyReport_ReportNotFound_ReturnsError()
        {
            var reportId = Guid.NewGuid();
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync((DailyReport)null);
            var result = await _dailyReportService.DisableDailyReport(reportId, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Báo cáo hàng ngày không tồn tại hoặc đã bị xóa", result.Message);
        }

        [Fact]
        public async Task DisableDailyReport_ReportInactive_ReturnsError()
        {
            var reportId = Guid.NewGuid();
            var report = new DailyReport { Id = reportId, IsActive = false };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
            var result = await _dailyReportService.DisableDailyReport(reportId, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Báo cáo hàng ngày không tồn tại hoặc đã bị xóa", result.Message);
        }

        [Fact]
        public async Task DisableDailyReport_LivestockCircleNotFound_ReturnsError()
        {
            var reportId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var report = new DailyReport { Id = reportId, IsActive = true, LivestockCircleId = circleId };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(circleId, null)).ReturnsAsync((LivestockCircle)null);
            var result = await _dailyReportService.DisableDailyReport(reportId, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Vòng chăn nuôi không tồn tại hoặc đã bị xóa", result.Message);
        }

        [Fact]
        public async Task DisableDailyReport_LivestockCircleInactive_ReturnsError()
        {
            var reportId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var report = new DailyReport { Id = reportId, IsActive = true, LivestockCircleId = circleId };
            var circle = new LivestockCircle { Id = circleId, IsActive = false };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(circleId, null)).ReturnsAsync(circle);
            var result = await _dailyReportService.DisableDailyReport(reportId, default);
            Assert.False(result.Succeeded);
            Assert.Equal("Vòng chăn nuôi không tồn tại hoặc đã bị xóa", result.Message);
        }

        [Fact]
        public async Task DisableDailyReport_Success()
        {
            var reportId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var medId = Guid.NewGuid();
            var imageId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var report = new DailyReport { Id = reportId, IsActive = true, LivestockCircleId = circleId, GoodUnit = 10, BadUnit = 2, DeadUnit = 3, CreatedBy = _userId, CreatedDate = now };
            var circle = new LivestockCircle { Id = circleId, IsActive = true, GoodUnitNumber = 5, BadUnitNumber = 1, DeadUnit = 1, UpdatedBy = _userId, UpdatedDate = now };
            var foodReport = new FoodReport { Id = Guid.NewGuid(), ReportId = reportId, FoodId = foodId, Quantity = 5, IsActive = true, CreatedBy = _userId, CreatedDate = now };
            var medicineReport = new MedicineReport { Id = Guid.NewGuid(), ReportId = reportId, MedicineId = medId, Quantity = 2, IsActive = true, CreatedBy = _userId, CreatedDate = now };
            var imageReport = new ImageDailyReport { Id = imageId, DailyReportId = reportId, ImageLink = "http://image", IsActive = true, CreatedBy = _userId, CreatedDate = now };
            var livestockCircleFood = new LivestockCircleFood { Id = Guid.NewGuid(), LivestockCircleId = circleId, FoodId = foodId, Remaining = 10 };
            var livestockCircleMedicine = new LivestockCircleMedicine { Id = Guid.NewGuid(), LivestockCircleId = circleId, MedicineId = medId, Remaining = 5 };

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null)).ReturnsAsync(report);
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(circleId, null)).ReturnsAsync(circle);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
                .Returns(new List<FoodReport> { foodReport }.AsQueryable().BuildMock());
            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns(new List<LivestockCircleFood> { livestockCircleFood }.AsQueryable().BuildMock());
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
                .Returns(new List<MedicineReport> { medicineReport }.AsQueryable().BuildMock());
            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns(new List<LivestockCircleMedicine> { livestockCircleMedicine }.AsQueryable().BuildMock());
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
                .Returns(new List<ImageDailyReport> { imageReport }.AsQueryable().BuildMock());
            _cloudinaryCloudServiceMock.Setup(x => x.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("Đã xóa thành công!");

            var result = await _dailyReportService.DisableDailyReport(reportId, default);

            Assert.True(result.Succeeded);
            Assert.Equal("Vô hiệu hóa báo cáo hàng ngày thành công", result.Message);
            Assert.Contains("Báo cáo hàng ngày đã được vô hiệu hóa", result.Data);
            _dailyReportRepositoryMock.Verify(x => x.Update(It.IsAny<DailyReport>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Once());
            _foodReportRepositoryMock.Verify(x => x.Update(It.IsAny<FoodReport>()), Times.AtLeastOnce());
            _medicineReportRepositoryMock.Verify(x => x.Update(It.IsAny<MedicineReport>()), Times.AtLeastOnce());
            _imageDailyReportRepositoryMock.Verify(x => x.Update(It.IsAny<ImageDailyReport>()), Times.AtLeastOnce());
            _cloudinaryCloudServiceMock.Verify(x => x.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
            _dailyReportRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _foodReportRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _livestockCircleFoodRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _medicineReportRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _livestockCircleMedicineRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
            _imageDailyReportRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
