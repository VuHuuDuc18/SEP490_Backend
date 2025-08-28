using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Domain.Helper.Constants;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.DailyReportService
{
    public class HasDailyReportTodayTest
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

        public HasDailyReportTodayTest()
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
        public async Task HasDailyReportToday_ReportExistsToday_ReturnsTrue()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                LivestockCircleName = "Test",
                Status = StatusConstant.GROWINGSTAT,
                IsActive = true
            };
            var today = DateTime.UtcNow.Date;
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircleId,
                Status = "today",
                Note = "test",
                IsActive = true,
                CreatedDate = today
            };

            var options = new DbContextOptionsBuilder<TestDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext3(options);
            context.LivestockCircles.Add(livestockCircle);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

            // Act
            var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Kiểm tra báo cáo thành công", result.Message);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task HasDailyReportToday_NoReportToday_ReturnsFalse()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                LivestockCircleName = "Test",
                Status = StatusConstant.GROWINGSTAT,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestDbContext3>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext3(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

            // Act
            var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Kiểm tra báo cáo thành công", result.Message);
            Assert.False(result.Data);
        }

        //[Fact]
        //public async Task HasDailyReportToday_InactiveReportToday_ReturnsFalse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    var today = DateTime.UtcNow.Date;
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircleId,
        //        Status = "today",
        //        Note = "test",
        //        IsActive = false, // Inactive report
        //        CreatedDate = today
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext3>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext3(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Kiểm tra báo cáo thành công", result.Message);
        //    Assert.False(result.Data);
        //}

        //[Fact]
        //public async Task HasDailyReportToday_ReportOnDifferentDate_ReturnsFalse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircleId,
        //        Status = "today",
        //        Note = "test",
        //        IsActive = true,
        //        CreatedDate = yesterday
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext3>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext3(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Kiểm tra báo cáo thành công", result.Message);
        //    Assert.False(result.Data);
        //}

        [Fact]
        public async Task HasDailyReportToday_LivestockCircleNotFound_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Vòng chăn nuôi không tồn tại", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Vòng chăn nuôi không tồn tại", result.Errors.First());
        }

        //[Fact]
        //public async Task HasDailyReportToday_InactiveLivestockCircle_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = false
        //    };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Vòng chăn nuôi không tồn tại", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Vòng chăn nuôi không tồn tại", result.Errors.First());
        //}

        //[Fact]
        //public async Task HasDailyReportToday_EmptyLivestockCircleId_ReturnsFalse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.Empty;
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    var options = new DbContextOptionsBuilder<TestDbContext3>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext3(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Kiểm tra báo cáo thành công", result.Message);
        //    Assert.False(result.Data);
        //}

        //[Fact]
        //public async Task HasDailyReportToday_LivestockCircleRepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ThrowsAsync(new Exception("Database error"));

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi kiểm tra báo cáo hàng ngày", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database error", result.Errors.First());
        //}

        //[Fact]
        //public async Task HasDailyReportToday_DailyReportRepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new Exception("Database query error"));

        //    // Act
        //    var result = await _dailyReportService.HasDailyReportToday(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi kiểm tra báo cáo hàng ngày", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database query error", result.Errors.First());
        //}

        //[Fact]
        //public async Task HasDailyReportToday_CancellationRequested_ThrowsOperationCanceledException()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    var cts = new CancellationTokenSource();
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new OperationCanceledException("Operation cancelled"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        //        _dailyReportService.HasDailyReportToday(livestockCircleId, cts.Token));
        //}

     
    }

    // InMemory DbContext for testing
    public class TestDbContext3 : DbContext
    {
        public TestDbContext3(DbContextOptions<TestDbContext3> options) : base(options) { }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
    }
}