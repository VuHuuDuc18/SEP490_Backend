using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;
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
    public class GetTodayDailyReportTest
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

        public GetTodayDailyReportTest()
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
        public async Task GetTodayDailyReport_ReportExistsWithRelatedData_ReturnsSuccess()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId,
                LivestockCircleName = "Test",
                Status = StatusConstant.GROWINGSTAT,
                IsActive = true };
            var today = DateTime.UtcNow.Date;
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircleId,
                IsActive = true,
                CreatedDate = today,
                Note = "Daily report",
                Status = "today",             
                DeadUnit = 0,
                GoodUnit = 100,
                BadUnit = 5,
                AgeInDays = 30
            };
            var foodReport = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = dailyReport.Id,
                FoodId = Guid.NewGuid(),
                Quantity = 50,
                IsActive = true
            };
            var medicineReport = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = dailyReport.Id,
                MedicineId = Guid.NewGuid(),
                Quantity = 10,
                IsActive = true
            };
            var imageDailyReport1 = new ImageDailyReport
            {
                DailyReportId = dailyReport.Id,
                ImageLink = "image1.jpg",
                Thumnail = "false",
                IsActive = true
            };
            var imageDailyReport2 = new ImageDailyReport
            {
                DailyReportId = dailyReport.Id,
                ImageLink = "thumbnail1.jpg",
                Thumnail = "true",
                IsActive = true
            };
            var food = new Food
            {
                Id = foodReport.FoodId,
                FoodName = "Food 1",
                IsActive = true
            };
            var medicine = new Medicine
            {
                Id = medicineReport.MedicineId,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var foodImage = new ImageFood
            {
                FoodId = foodReport.FoodId,
                ImageLink = "food_thumbnail.jpg",
                Thumnail = "true"
            };
            var medicineImage = new ImageMedicine
            {
                MedicineId = medicineReport.MedicineId,
                ImageLink = "medicine_thumbnail.jpg",
                Thumnail = "true"
            };

            var options = new DbContextOptionsBuilder<TestDbContext4>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext4(options);
            context.LivestockCircles.Add(livestockCircle);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.Add(foodReport);
            context.MedicineReports.Add(medicineReport);
            context.ImageDailyReports.AddRange(imageDailyReport1, imageDailyReport2);
            context.Foods.Add(food);
            context.Medicines.Add(medicine);
            context.ImageFoods.Add(foodImage);
            context.ImageMedicines.Add(medicineImage);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
                .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodReport.FoodId, null))
                .ReturnsAsync(food);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineReport.MedicineId, null))
                .ReturnsAsync(medicine);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy báo cáo hàng ngày hôm nay thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(dailyReport.Id, result.Data.Id);
            Assert.Equal(dailyReport.LivestockCircleId, result.Data.LivestockCircleId);
            Assert.Equal(dailyReport.Note, result.Data.Note);
            Assert.Equal(dailyReport.DeadUnit, result.Data.DeadUnit);
            Assert.Equal(dailyReport.GoodUnit, result.Data.GoodUnit);
            Assert.Equal(dailyReport.BadUnit, result.Data.BadUnit);
            Assert.Equal(dailyReport.AgeInDays, result.Data.AgeInDays);
            Assert.Equal(dailyReport.CreatedDate.Date, result.Data.CreatedDate.Date);
            Assert.Single(result.Data.ImageLinks);
            Assert.Equal("image1.jpg", result.Data.ImageLinks.First());
            Assert.Equal("thumbnail1.jpg", result.Data.Thumbnail);
            Assert.Single(result.Data.FoodReports);
            Assert.Equal(foodReport.Quantity, result.Data.FoodReports[0].Quantity);
            Assert.Equal(food.FoodName, result.Data.FoodReports[0].Food.FoodName);
            Assert.Equal(foodImage.ImageLink, result.Data.FoodReports[0].Food.Thumbnail);
            Assert.Single(result.Data.MedicineReports);
            Assert.Equal(medicineReport.Quantity, result.Data.MedicineReports[0].Quantity);
            Assert.Equal(medicine.MedicineName, result.Data.MedicineReports[0].Medicine.MedicineName);
            Assert.Equal(medicineImage.ImageLink, result.Data.MedicineReports[0].Medicine.Thumbnail);
        }

        //[Fact]
        //public async Task GetTodayDailyReport_ReportExistsNoRelatedData_ReturnsSuccess()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true };
        //    var today = DateTime.UtcNow.Date;
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircleId,
        //        IsActive = true,
        //        CreatedDate = today,
        //        Status = "today",
        //        Note = "Daily report"
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext4(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
        //        .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
        //        .ReturnsAsync((Food)null);
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null))
        //        .ReturnsAsync((Medicine)null);
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy báo cáo hàng ngày hôm nay thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(dailyReport.Id, result.Data.Id);
        //    Assert.Equal(dailyReport.LivestockCircleId, result.Data.LivestockCircleId);
        //    Assert.Equal(dailyReport.Note, result.Data.Note);
        //    Assert.Empty(result.Data.ImageLinks);
        //    Assert.Null(result.Data.Thumbnail);
        //    Assert.Empty(result.Data.FoodReports);
        //    Assert.Empty(result.Data.MedicineReports);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_MissingFoodAndMedicineDetails_ReturnsPartialData()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true };
        //    var today = DateTime.UtcNow.Date;
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircleId,
        //        IsActive = true,
        //        Status = "today",
        //        CreatedDate = today,
        //        Note = "Daily report"
        //    };
        //    var foodReport = new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        ReportId = dailyReport.Id,
        //        FoodId = Guid.NewGuid(),
        //        Quantity = 50,
        //        IsActive = true
        //    };
        //    var medicineReport = new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        ReportId = dailyReport.Id,
        //        MedicineId = Guid.NewGuid(),
        //        Quantity = 10,
        //        IsActive = true
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext4(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.FoodReports.Add(foodReport);
        //    context.MedicineReports.Add(medicineReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
        //        .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodReport.FoodId, null))
        //        .ReturnsAsync((Food)null);
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineReport.MedicineId, null))
        //        .ReturnsAsync((Medicine)null);
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy báo cáo hàng ngày hôm nay thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(dailyReport.Id, result.Data.Id);
        //    Assert.Single(result.Data.FoodReports);
        //    //Assert.Equal(foodReport.FoodId, result.Data.FoodReports[0].Food.Id);
        //    //Assert.Null(result.Data.FoodReports[0].Food.FoodName);
        //    //Assert.Null(result.Data.FoodReports[0].Food.Thumbnail);
        //    //Assert.Single(result.Data.MedicineReports);
        //    //Assert.Equal(medicineReport.MedicineId, result.Data.MedicineReports[0].Medicine.Id);
        //    //Assert.Null(result.Data.MedicineReports[0].Medicine.MedicineName);
        //    //Assert.Null(result.Data.MedicineReports[0].Medicine.Thumbnail);
        //}

        [Fact]
        public async Task GetTodayDailyReport_LivestockCircleNotFound_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Vòng chăn nuôi không tồn tại", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Vòng chăn nuôi không tồn tại", result.Errors.First());
            Assert.Null(result.Data);
        }

        //[Fact]
        //public async Task GetTodayDailyReport_InactiveLivestockCircle_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = false };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Vòng chăn nuôi không tồn tại", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Vòng chăn nuôi không tồn tại", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        [Fact]
        public async Task GetTodayDailyReport_NoReportToday_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId,
                LivestockCircleName = "Test",
                Status = StatusConstant.GROWINGSTAT,
                IsActive = true };
            var options = new DbContextOptionsBuilder<TestDbContext4>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext4(options);
            context.LivestockCircles.Add(livestockCircle);
            context.SaveChanges();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

            // Act
            var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Errors.First());
            Assert.Null(result.Data);
        }

        //[Fact]
        //public async Task GetTodayDailyReport_InactiveReportToday_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true };
        //    var today = DateTime.UtcNow.Date;
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        LivestockCircleId = livestockCircleId,
        //        IsActive = false,
        //        Status = "today",
        //        Note = "test",
        //        CreatedDate = today
        //    };

        //    var options = new DbContextOptionsBuilder<TestDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext4(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_ReportOnDifferentDate_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true };
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

        //    var options = new DbContextOptionsBuilder<TestDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext4(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_EmptyLivestockCircleId_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.Empty;
        //    var livestockCircle = new LivestockCircle { 
        //        Id = livestockCircleId,
        //        LivestockCircleName = "Test",
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };
        //    var options = new DbContextOptionsBuilder<TestDbContext4>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext4(options);
        //    context.LivestockCircles.Add(livestockCircle);
        //    context.SaveChanges();

        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày cho hôm nay", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_LivestockCircleRepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ThrowsAsync(new Exception("Database error"));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy báo cáo hàng ngày hôm nay", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database error", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_DailyReportRepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new Exception("Database query error"));

        //    // Act
        //    var result = await _dailyReportService.GetTodayDailyReport(livestockCircleId, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy báo cáo hàng ngày hôm nay", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database query error", result.Errors.First());
        //    Assert.Null(result.Data);
        //}

        //[Fact]
        //public async Task GetTodayDailyReport_CancellationRequested_ThrowsOperationCanceledException()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };
        //    var cts = new CancellationTokenSource();
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
        //        .ReturnsAsync(livestockCircle);
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new OperationCanceledException("Operation cancelled"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<OperationCanceledException>(() =>
        //        _dailyReportService.GetTodayDailyReport(livestockCircleId, cts.Token));
        //}

       
    }

    // InMemory DbContext for testing
    public class TestDbContext4 : DbContext
    {
        public TestDbContext4(DbContextOptions<TestDbContext4> options) : base(options) { }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<FoodReport> FoodReports { get; set; }
        public DbSet<MedicineReport> MedicineReports { get; set; }
        public DbSet<ImageDailyReport> ImageDailyReports { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<ImageFood> ImageFoods { get; set; }
        public DbSet<ImageMedicine> ImageMedicines { get; set; }
    }
} 