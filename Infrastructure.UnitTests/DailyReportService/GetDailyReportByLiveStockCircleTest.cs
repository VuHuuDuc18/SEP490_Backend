using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response.DailyReport;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;
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
    public class GetDailyReportByLiveStockCircleTest
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
        public GetDailyReportByLiveStockCircleTest()
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
        public async Task GetDailyReportByLiveStockCircle_NullLivestockCircleId_ReturnsAllActiveReports()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var livestockCircleId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow;
            var dailyReports = new List<DailyReport>
        {
            new DailyReport
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
            }
        }.AsQueryable();
            var foodReports = new List<FoodReport>
        {
            new FoodReport
            {
                Id = Guid.NewGuid(),
                FoodId = foodId,
                ReportId = reportId,
                Quantity = 2,
                IsActive = true
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
                IsActive = true
            }
        }.AsQueryable();
            var imageDailyReports = new List<ImageDailyReport>
        {
            new ImageDailyReport
            {
                Id = Guid.NewGuid(),
                DailyReportId = reportId,
                Thumnail = "true",
                ImageLink = "https://cloudinary.com/thumbnail.jpg",
                IsActive = true
            },
            new ImageDailyReport
            {
                Id = Guid.NewGuid(),
                DailyReportId = reportId,
                Thumnail = "false",
                ImageLink = "https://cloudinary.com/image.jpg",
                IsActive = true
            }
        }.AsQueryable();
            var foods = new List<Food>
        {
            new Food { Id = foodId, FoodName = "Food1", IsActive = true }
        }.AsQueryable();
            var medicines = new List<Medicine>
        {
            new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
        }.AsQueryable();
            var foodImages = new List<ImageFood>
        {
            new ImageFood { FoodId = foodId, ImageLink = "https://cloudinary.com/food_thumbnail.jpg", Thumnail = "true" }
        }.AsQueryable();
            var medicineImages = new List<ImageMedicine>
        {
            new ImageMedicine { MedicineId = medicineId, ImageLink = "https://cloudinary.com/medicine_thumbnail.jpg", Thumnail = "true" }
        }.AsQueryable();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
                .Returns(dailyReports.BuildMock());
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
                .Returns(foodReports.BuildMock());
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
                .Returns(medicineReports.BuildMock());
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
                .Returns(imageDailyReports.BuildMock());
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId, null))
                .ReturnsAsync(foods.First());
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId, null))
                .ReturnsAsync(medicines.First());
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
                .Returns(foodImages.BuildMock());
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns(medicineImages.BuildMock());

            // Act
            var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(null, default);

            // Assert
            Assert.Null(errorMessage);
            Assert.NotNull(dailyReportsResult);
            Assert.Single(dailyReportsResult);
            var report = dailyReportsResult.First();
            Assert.Equal(reportId, report.Id);
            Assert.Equal(livestockCircleId, report.LivestockCircleId);
            Assert.Equal(1, report.DeadUnit);
            Assert.Equal(8, report.GoodUnit);
            Assert.Equal(1, report.BadUnit);
            Assert.Equal("Test note", report.Note);
            Assert.True(report.IsActive);
            Assert.Equal(5, report.AgeInDays);
            Assert.Equal(createdDate, report.CreatedDate);
            Assert.Single(report.FoodReports);
            Assert.Single(report.MedicineReports);
            Assert.Single(report.ImageLinks);
            Assert.Equal("https://cloudinary.com/thumbnail.jpg", report.Thumbnail);
            Assert.Equal("https://cloudinary.com/image.jpg", report.ImageLinks.First());
            Assert.Equal(foodId, report.FoodReports.First().Food.Id);
            Assert.Equal(2, report.FoodReports.First().Quantity);
            Assert.True(report.FoodReports.First().IsActive);
            Assert.Equal("Food1", report.FoodReports.First().Food.FoodName);
            Assert.Equal("https://cloudinary.com/food_thumbnail.jpg", report.FoodReports.First().Food.Thumbnail);
            Assert.Equal(medicineId, report.MedicineReports.First().Medicine.Id);
            Assert.Equal(3, report.MedicineReports.First().Quantity);
            Assert.True(report.MedicineReports.First().IsActive);
            Assert.Equal("Medicine1", report.MedicineReports.First().Medicine.MedicineName);
            Assert.Equal("https://cloudinary.com/medicine_thumbnail.jpg", report.MedicineReports.First().Medicine.Thumbnail);
        }

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_EmptyLivestockCircleId_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var dailyReports = new List<DailyReport>().AsQueryable().BuildMock();
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports);

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(Guid.Empty, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Empty(dailyReportsResult);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_NonExistentLivestockCircleId_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var dailyReports = new List<DailyReport>().AsQueryable().BuildMock();
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports);

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Empty(dailyReportsResult);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_NoActiveReports_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var dailyReports = new List<DailyReport>
        //{
        //    new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        CreatedBy = _userId,
        //        IsActive = false,
        //        LivestockCircleId = livestockCircleId,
        //        DeadUnit = 1,
        //        GoodUnit = 8,
        //        BadUnit = 1,
        //        Note = "Test note",
        //        AgeInDays = 5,
        //        Status = "Normal",
        //        CreatedDate = DateTime.UtcNow
        //    }
        //}.AsQueryable();
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports.BuildMock());

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Empty(dailyReportsResult);
        //}

        [Fact]
        public async Task GetDailyReportByLiveStockCircle_Success_WithRelatedData()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var reportId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var createdDate = DateTime.UtcNow;
            var dailyReports = new List<DailyReport>
        {
            new DailyReport
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
            }
        }.AsQueryable();
            var foodReports = new List<FoodReport>
        {
            new FoodReport
            {
                Id = Guid.NewGuid(),
                FoodId = foodId,
                ReportId = reportId,
                Quantity = 2,
                IsActive = true
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
                IsActive = true
            }
        }.AsQueryable();
            var imageDailyReports = new List<ImageDailyReport>
        {
            new ImageDailyReport
            {
                Id = Guid.NewGuid(),
                DailyReportId = reportId,
                Thumnail = "true",
                ImageLink = "https://cloudinary.com/thumbnail.jpg",
                IsActive = true
            },
            new ImageDailyReport
            {
                Id = Guid.NewGuid(),
                DailyReportId = reportId,
                Thumnail = "false",
                ImageLink = "https://cloudinary.com/image.jpg",
                IsActive = true
            }
        }.AsQueryable();
            var foods = new List<Food>
        {
            new Food { Id = foodId, FoodName = "Food1", IsActive = true }
        }.AsQueryable();
            var medicines = new List<Medicine>
        {
            new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
        }.AsQueryable();
            var foodImages = new List<ImageFood>
        {
            new ImageFood { FoodId = foodId, ImageLink = "https://cloudinary.com/food_thumbnail.jpg", Thumnail = "true" }
        }.AsQueryable();
            var medicineImages = new List<ImageMedicine>
        {
            new ImageMedicine { MedicineId = medicineId, ImageLink = "https://cloudinary.com/medicine_thumbnail.jpg", Thumnail = "true" }
        }.AsQueryable();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
                .Returns(dailyReports.BuildMock());
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
                .Returns(foodReports.BuildMock());
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
                .Returns(medicineReports.BuildMock());
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
                .Returns(imageDailyReports.BuildMock());
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId,null))
                .ReturnsAsync(foods.First());
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId,null))
                .ReturnsAsync(medicines.First());
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
                .Returns(foodImages.BuildMock());
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns(medicineImages.BuildMock());

            // Act
            var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

            // Assert
            Assert.Null(errorMessage);
            Assert.NotNull(dailyReportsResult);
            Assert.Single(dailyReportsResult);
            var report = dailyReportsResult.First();
            Assert.Equal(reportId, report.Id);
            Assert.Equal(livestockCircleId, report.LivestockCircleId);
            Assert.Equal(1, report.DeadUnit);
            Assert.Equal(8, report.GoodUnit);
            Assert.Equal(1, report.BadUnit);
            Assert.Equal("Test note", report.Note);
            Assert.True(report.IsActive);
            Assert.Equal(5, report.AgeInDays);
            Assert.Equal(createdDate, report.CreatedDate);
            Assert.Single(report.FoodReports);
            Assert.Single(report.MedicineReports);
            Assert.Single(report.ImageLinks);
            Assert.Equal("https://cloudinary.com/thumbnail.jpg", report.Thumbnail);
            Assert.Equal("https://cloudinary.com/image.jpg", report.ImageLinks.First());
            Assert.Equal(foodId, report.FoodReports.First().Food.Id);
            Assert.Equal(2, report.FoodReports.First().Quantity);
            Assert.True(report.FoodReports.First().IsActive);
            Assert.Equal("Food1", report.FoodReports.First().Food.FoodName);
            Assert.Equal("https://cloudinary.com/food_thumbnail.jpg", report.FoodReports.First().Food.Thumbnail);
            Assert.Equal(medicineId, report.MedicineReports.First().Medicine.Id);
            Assert.Equal(3, report.MedicineReports.First().Quantity);
            Assert.True(report.MedicineReports.First().IsActive);
            Assert.Equal("Medicine1", report.MedicineReports.First().Medicine.MedicineName);
            Assert.Equal("https://cloudinary.com/medicine_thumbnail.jpg", report.MedicineReports.First().Medicine.Thumbnail);
        }

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_InactiveRelatedData_ExcludesFromResponse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var reportId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var createdDate = DateTime.UtcNow;
        //    var dailyReports = new List<DailyReport>
        //{
        //    new DailyReport
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
        //        CreatedDate = createdDate
        //    }
        //}.AsQueryable();
        //    var foodReports = new List<FoodReport>
        //{
        //    new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodId = foodId,
        //        ReportId = reportId,
        //        Quantity = 2,
        //        IsActive = false
        //    }
        //}.AsQueryable();
        //    var medicineReports = new List<MedicineReport>
        //{
        //    new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        MedicineId = medicineId,
        //        ReportId = reportId,
        //        Quantity = 3,
        //        IsActive = false
        //    }
        //}.AsQueryable();
        //    var imageDailyReports = new List<ImageDailyReport>
        //{
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId,
        //        Thumnail = "true",
        //        ImageLink = "https://cloudinary.com/thumbnail.jpg",
        //        IsActive = false
        //    }
        //}.AsQueryable();
        //    var foods = new List<Food>
        //{
        //    new Food { Id = foodId, FoodName = "Food1", IsActive = true }
        //}.AsQueryable();
        //    var medicines = new List<Medicine>
        //{
        //    new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
        //}.AsQueryable();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports.BuildMock());
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
        //        .Returns(foodReports.BuildMock());
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
        //        .Returns(medicineReports.BuildMock());
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns(imageDailyReports.BuildMock());
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId,null))
        //        .ReturnsAsync(foods.First());
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId,null))
        //        .ReturnsAsync(medicines.First());
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(new List<ImageFood>().AsQueryable().BuildMock());
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns(new List<ImageMedicine>().AsQueryable().BuildMock());

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Single(dailyReportsResult);
        //    var report = dailyReportsResult.First();
        //    Assert.Equal(reportId, report.Id);
        //    Assert.Equal(livestockCircleId, report.LivestockCircleId);
        //    Assert.Equal(1, report.DeadUnit);
        //    Assert.Equal(8, report.GoodUnit);
        //    Assert.Equal(1, report.BadUnit);
        //    Assert.Equal("Test note", report.Note);
        //    Assert.True(report.IsActive);
        //    Assert.Equal(5, report.AgeInDays);
        //    Assert.Equal(createdDate, report.CreatedDate);
        //    //Assert.Empty(report.FoodReports);
        //    //Assert.Empty(report.MedicineReports);
        //    //Assert.Empty(report.ImageLinks);
        //    //Assert.Null(report.Thumbnail);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_NullImageLinkOrThumbnail_ExcludesFromResponse()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var reportId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var createdDate = DateTime.UtcNow;
        //    var dailyReports = new List<DailyReport>
        //{
        //    new DailyReport
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
        //        CreatedDate = createdDate
        //    }
        //}.AsQueryable();
        //    var foodReports = new List<FoodReport>
        //{
        //    new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodId = foodId,
        //        ReportId = reportId,
        //        Quantity = 2,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var medicineReports = new List<MedicineReport>
        //{
        //    new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        MedicineId = medicineId,
        //        ReportId = reportId,
        //        Quantity = 3,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var imageDailyReports = new List<ImageDailyReport>
        //{
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId,
        //        Thumnail = "true",
        //        ImageLink = null,
        //        IsActive = true
        //    },
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId,
        //        Thumnail = "false",
        //        ImageLink = null,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var foods = new List<Food>
        //{
        //    new Food { Id = foodId, FoodName = "Food1", IsActive = true }
        //}.AsQueryable();
        //    var medicines = new List<Medicine>
        //{
        //    new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
        //}.AsQueryable();
        //    var foodImages = new List<ImageFood>
        //{
        //    new ImageFood { FoodId = foodId, ImageLink = "https://cloudinary.com/food_thumbnail.jpg", Thumnail = "true" }
        //}.AsQueryable();
        //    var medicineImages = new List<ImageMedicine>
        //{
        //    new ImageMedicine { MedicineId = medicineId, ImageLink = "https://cloudinary.com/medicine_thumbnail.jpg", Thumnail = "true" }
        //}.AsQueryable();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports.BuildMock());
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
        //        .Returns(foodReports.BuildMock());
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
        //        .Returns(medicineReports.BuildMock());
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns(imageDailyReports.BuildMock());
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId,null))
        //        .ReturnsAsync(foods.First());
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId,null))
        //        .ReturnsAsync(medicines.First());
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(foodImages.BuildMock());
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns(medicineImages.BuildMock());

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Single(dailyReportsResult);
        //    var report = dailyReportsResult.First();
        //    Assert.Equal(reportId, report.Id);
        //    Assert.Equal(livestockCircleId, report.LivestockCircleId);
        //    Assert.Equal(1, report.DeadUnit);
        //    Assert.Equal(8, report.GoodUnit);
        //    Assert.Equal(1, report.BadUnit);
        //    Assert.Equal("Test note", report.Note);
        //    Assert.True(report.IsActive);
        //    Assert.Equal(5, report.AgeInDays);
        //    Assert.Equal(createdDate, report.CreatedDate);
        //    Assert.Single(report.FoodReports);
        //    Assert.Single(report.MedicineReports);
        //    //Assert.Empty(report.ImageLinks);
        //    //Assert.Null(report.Thumbnail);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_InactiveFoodOrMedicine_IncludesEmptyDetails()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var reportId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var createdDate = DateTime.UtcNow;
        //    var dailyReports = new List<DailyReport>
        //{
        //    new DailyReport
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
        //        CreatedDate = createdDate
        //    }
        //}.AsQueryable();
        //    var foodReports = new List<FoodReport>
        //{
        //    new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodId = foodId,
        //        ReportId = reportId,
        //        Quantity = 2,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var medicineReports = new List<MedicineReport>
        //{
        //    new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        MedicineId = medicineId,
        //        ReportId = reportId,
        //        Quantity = 3,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var imageDailyReports = new List<ImageDailyReport>
        //{
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId,
        //        Thumnail = "true",
        //        ImageLink = "https://cloudinary.com/thumbnail.jpg",
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var foods = new List<Food>
        //{
        //    new Food { Id = foodId, FoodName = "Food1", IsActive = false }
        //}.AsQueryable();
        //    var medicines = new List<Medicine>
        //{
        //    new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = false }
        //}.AsQueryable();
        //    var foodImages = new List<ImageFood>().AsQueryable();
        //    var medicineImages = new List<ImageMedicine>().AsQueryable();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports.BuildMock());
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
        //        .Returns(foodReports.BuildMock());
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
        //        .Returns(medicineReports.BuildMock());
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns(imageDailyReports.BuildMock());
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId,null))
        //        .ReturnsAsync((Food)null);
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId,null))
        //        .ReturnsAsync((Medicine)null);
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(foodImages.BuildMock());
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns(medicineImages.BuildMock());

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Single(dailyReportsResult);
        //    var report = dailyReportsResult.First();
        //    Assert.Equal(reportId, report.Id);
        //    Assert.Equal(livestockCircleId, report.LivestockCircleId);
        //    Assert.Equal(1, report.DeadUnit);
        //    Assert.Equal(8, report.GoodUnit);
        //    Assert.Equal(1, report.BadUnit);
        //    Assert.Equal("Test note", report.Note);
        //    Assert.True(report.IsActive);
        //    Assert.Equal(5, report.AgeInDays);
        //    Assert.Equal(createdDate, report.CreatedDate);
        //    Assert.Single(report.FoodReports);
        //    Assert.Single(report.MedicineReports);
        //    Assert.Empty(report.ImageLinks);
        //    Assert.Equal("https://cloudinary.com/thumbnail.jpg", report.Thumbnail);
        //    Assert.Equal(Guid.Empty, report.FoodReports.First().Food.Id);
        //    Assert.Null(report.FoodReports.First().Food.FoodName);
        //    Assert.Null(report.FoodReports.First().Food.Thumbnail);
        //    Assert.Equal(Guid.Empty, report.MedicineReports.First().Medicine.Id);
        //    Assert.Null(report.MedicineReports.First().Medicine.MedicineName);
        //    Assert.Null(report.MedicineReports.First().Medicine.Thumbnail);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_RepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.NotNull(errorMessage);
        //    Assert.Contains("Database error", errorMessage);
        //    Assert.Null(dailyReportsResult);
        //}

        //[Fact]
        //public async Task GetDailyReportByLiveStockCircle_MultipleReports_ReturnsAllReports()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var reportId1 = Guid.NewGuid();
        //    var reportId2 = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var createdDate = DateTime.UtcNow;
        //    var dailyReports = new List<DailyReport>
        //{
        //    new DailyReport
        //    {
        //        Id = reportId1,
        //        CreatedBy = _userId,
        //        IsActive = true,
        //        LivestockCircleId = livestockCircleId,
        //        DeadUnit = 1,
        //        GoodUnit = 8,
        //        BadUnit = 1,
        //        Note = "Test note 1",
        //        AgeInDays = 5,
        //        Status = "Normal",
        //        CreatedDate = createdDate
        //    },
        //    new DailyReport
        //    {
        //        Id = reportId2,
        //        CreatedBy = _userId,
        //        IsActive = true,
        //        LivestockCircleId = livestockCircleId,
        //        DeadUnit = 2,
        //        GoodUnit = 7,
        //        BadUnit = 2,
        //        Note = "Test note 2",
        //        AgeInDays = 6,
        //        Status = "Normal",
        //        CreatedDate = createdDate.AddDays(1)
        //    }
        //}.AsQueryable();
        //    var foodReports = new List<FoodReport>
        //{
        //    new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodId = foodId,
        //        ReportId = reportId1,
        //        Quantity = 2,
        //        IsActive = true
        //    },
        //    new FoodReport
        //    {
        //        Id = Guid.NewGuid(),
        //        FoodId = foodId,
        //        ReportId = reportId2,
        //        Quantity = 3,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var medicineReports = new List<MedicineReport>
        //{
        //    new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        MedicineId = medicineId,
        //        ReportId = reportId1,
        //        Quantity = 3,
        //        IsActive = true
        //    },
        //    new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        MedicineId = medicineId,
        //        ReportId = reportId2,
        //        Quantity = 4,
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var imageDailyReports = new List<ImageDailyReport>
        //{
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId1,
        //        Thumnail = "true",
        //        ImageLink = "https://cloudinary.com/thumbnail1.jpg",
        //        IsActive = true
        //    },
        //    new ImageDailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        DailyReportId = reportId2,
        //        Thumnail = "true",
        //        ImageLink = "https://cloudinary.com/thumbnail2.jpg",
        //        IsActive = true
        //    }
        //}.AsQueryable();
        //    var foods = new List<Food>
        //{
        //    new Food { Id = foodId, FoodName = "Food1", IsActive = true }
        //}.AsQueryable();
        //    var medicines = new List<Medicine>
        //{
        //    new Medicine { Id = medicineId, MedicineName = "Medicine1", IsActive = true }
        //}.AsQueryable();
        //    var foodImages = new List<ImageFood>
        //{
        //    new ImageFood { FoodId = foodId, ImageLink = "https://cloudinary.com/food_thumbnail.jpg", Thumnail = "true" }
        //}.AsQueryable();
        //    var medicineImages = new List<ImageMedicine>
        //{
        //    new ImageMedicine { MedicineId = medicineId, ImageLink = "https://cloudinary.com/medicine_thumbnail.jpg", Thumnail = "true" }
        //}.AsQueryable();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<DailyReport, bool>>>()))
        //        .Returns(dailyReports.BuildMock());
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<FoodReport, bool>>>()))
        //        .Returns(foodReports.BuildMock());
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineReport, bool>>>()))
        //        .Returns(medicineReports.BuildMock());
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns(imageDailyReports.BuildMock());
        //    _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId,null))
        //        .ReturnsAsync(foods.First());
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId,null))
        //        .ReturnsAsync(medicines.First());
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(foodImages.BuildMock());
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns(medicineImages.BuildMock());

        //    // Act
        //    var (dailyReportsResult, errorMessage) = await _dailyReportService.GetDailyReportByLiveStockCircle(livestockCircleId, default);

        //    // Assert
        //    Assert.Null(errorMessage);
        //    Assert.NotNull(dailyReportsResult);
        //    Assert.Equal(2, dailyReportsResult.Count);
        //    var report1 = dailyReportsResult.First(r => r.Id == reportId1);
        //    Assert.Equal("Test note 1", report1.Note);
        //    //Assert.Single(report1.FoodReports);
        //    //Assert.Single(report1.MedicineReports);
        //    //Assert.Empty(report1.ImageLinks);
        //    //Assert.Equal("https://cloudinary.com/thumbnail1.jpg", report1.Thumbnail);
        //    //Assert.Equal(2, report1.FoodReports.First().Quantity);
        //    //Assert.Equal(3, report1.MedicineReports.First().Quantity);
        //    //var report2 = dailyReportsResult.First(r => r.Id == reportId2);
        //    //Assert.Equal("Test note 2", report2.Note);
        //    //Assert.Single(report2.FoodReports);
        //    //Assert.Single(report2.MedicineReports);
        //    //Assert.Empty(report2.ImageLinks);
        //    //Assert.Equal("https://cloudinary.com/thumbnail2.jpg", report2.Thumbnail);
        //    //Assert.Equal(3, report2.FoodReports.First().Quantity);
        //    //Assert.Equal(4, report2.MedicineReports.First().Quantity);
        //}
    }

}