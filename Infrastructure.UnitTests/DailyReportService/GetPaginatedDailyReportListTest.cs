using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using MockQueryable.Moq;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.DailyReportService
{
    public class GetPaginatedDailyReportListTest
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

        public GetPaginatedDailyReportListTest()
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
        //public async Task GetPaginatedDailyReportList_NullRequest_ReturnsError()
        //{
        //    // Act
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(null, null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Yêu cầu không được null", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Yêu cầu không được null", result.Errors.First());
        //}

        [Fact]
        public async Task GetPaginatedDailyReportList_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_InvalidSortField_ReturnsError()
        {
            // Arrange
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "desc" }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.StartsWith("Trường sắp xếp không hợp lệ: InvalidField", result.Message);
            Assert.Single(result.Errors);
            Assert.Contains("Trường hợp lệ:", result.Errors.First());
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_Success_ReturnsPaginatedDailyReports()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var dailyReport1 = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId,
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId,
                DeadUnit = 0,
                GoodUnit = 100,
                BadUnit = 5,
                AgeInDays = 30
            };
            var dailyReport2 = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId,
                Note = "Report 2",
                Status = "yesterday",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                CreatedBy = _userId,
                DeadUnit = 1,
                GoodUnit = 99,
                BadUnit = 4,
                AgeInDays = 29
            };
            var foodReport = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = dailyReport1.Id,
                FoodId = Guid.NewGuid(),
                Quantity = 50,
                IsActive = true
            };
            var medicineReport = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = dailyReport1.Id,
                MedicineId = Guid.NewGuid(),
                Quantity = 10,
                IsActive = true
            };
            var imageDailyReport1 = new ImageDailyReport
            {
                DailyReportId = dailyReport1.Id,
                ImageLink = "image1.jpg",
                Thumnail = "false",
                IsActive = true
            };
            var imageDailyReport2 = new ImageDailyReport
            {
                DailyReportId = dailyReport1.Id,
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

            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.AddRange(dailyReport1, dailyReport2);
            context.FoodReports.Add(foodReport);
            context.MedicineReports.Add(medicineReport);
            context.ImageDailyReports.AddRange(imageDailyReport1, imageDailyReport2);
            context.Foods.Add(food);
            context.Medicines.Add(medicine);
            context.ImageFoods.Add(foodImage);
            context.ImageMedicines.Add(medicineImage);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
                .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);

            var report1 = result.Data.Items.FirstOrDefault(x => x.Id == dailyReport1.Id);
            Assert.NotNull(report1);
            Assert.Equal(dailyReport1.Note, report1.Note);
          //  Assert.Equal(dailyReport1.Status, report1.Status);
            Assert.Equal(dailyReport1.CreatedDate, report1.CreatedDate);
            Assert.Equal(dailyReport1.DeadUnit, report1.DeadUnit);
            Assert.Equal(dailyReport1.GoodUnit, report1.GoodUnit);
            Assert.Equal(dailyReport1.BadUnit, report1.BadUnit);
            Assert.Equal(dailyReport1.AgeInDays, report1.AgeInDays);
            Assert.Single(report1.ImageLinks);
            Assert.Contains("image1.jpg", report1.ImageLinks);
            Assert.Equal("thumbnail1.jpg", report1.Thumbnail);
            Assert.Single(report1.FoodReports);
            Assert.Equal(foodReport.Quantity, report1.FoodReports[0].Quantity);
            Assert.Equal(food.FoodName, report1.FoodReports[0].Food.FoodName);
            Assert.Equal(foodImage.ImageLink, report1.FoodReports[0].Food.Thumbnail);
            Assert.Single(report1.MedicineReports);
            Assert.Equal(medicineReport.Quantity, report1.MedicineReports[0].Quantity);
            Assert.Equal(medicine.MedicineName, report1.MedicineReports[0].Medicine.MedicineName);
            Assert.Equal(medicineImage.ImageLink, report1.MedicineReports[0].Medicine.Thumbnail);

            var report2 = result.Data.Items.FirstOrDefault(x => x.Id == dailyReport2.Id);
            Assert.NotNull(report2);
            Assert.Empty(report2.ImageLinks);
            Assert.Null(report2.Thumbnail);
            Assert.Empty(report2.FoodReports);
            Assert.Empty(report2.MedicineReports);
            Assert.True(result.Data.Items[0].CreatedDate >= result.Data.Items[1].CreatedDate, "Items should be sorted by CreatedDate descending");
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_Success_WithFilter()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId,
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
                .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Status", Value = "today" } }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            var report = result.Data.Items.First();
            Assert.Equal(dailyReport.Id, report.Id);
         //   Assert.Equal("today", report.Status);
            Assert.Equal("Report 1", report.Note);
            Assert.Empty(report.ImageLinks);
            Assert.Null(report.Thumbnail);
            Assert.Empty(report.FoodReports);
            Assert.Empty(report.MedicineReports);
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_Success_WithSearch()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId,
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
                .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Note", Value = "Report 1" } }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            var report = result.Data.Items.First();
            Assert.Equal(dailyReport.Id, report.Id);
            Assert.Equal("Report 1", report.Note);
          //  Assert.Equal("today", report.Status);
            Assert.Empty(report.ImageLinks);
            Assert.Null(report.Thumbnail);
            Assert.Empty(report.FoodReports);
            Assert.Empty(report.MedicineReports);
        }

        [Fact]
        public async Task GetPaginatedDailyReportList_Success_WithLivestockCircleId()
        {
            // Arrange
            var livestockCircleId1 = Guid.NewGuid();
            var livestockCircleId2 = Guid.NewGuid();
            var dailyReport1 = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId1,
                Note = "Report 1",
                Status = "today",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _userId
            };
            var dailyReport2 = new DailyReport
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                LivestockCircleId = livestockCircleId2,
                Note = "Report 2",
                Status = "yesterday",
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                CreatedBy = _userId
            };
            var options = new DbContextOptionsBuilder<TestDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext2(options);
            context.DailyReports.AddRange(dailyReport1, dailyReport2);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
                .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
            _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.DailyReports);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
                .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
            _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
                .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" }
            };

            // Act
            var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId1, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            var report = result.Data.Items.First();
            Assert.Equal(dailyReport1.Id, report.Id);
            Assert.Equal(livestockCircleId1, report.LivestockCircleId);
            Assert.Empty(result.Data.Items.Where(x => x.LivestockCircleId == livestockCircleId2));
        }

        //[Fact]
        //public async Task GetPaginatedDailyReportList_NoDailyReports_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var options = new DbContextOptionsBuilder<TestDbContext2>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext2(options);
        //    context.SaveChanges();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
        //        .Returns(context.DailyReports);
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
        //        .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
        //    _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
        //        .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" }
        //    };

        //    // Act
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

            
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}, Errors: {string.Join(", ", result.Errors)}");
        //    Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Empty(result.Data.Items);
        //    Assert.Equal(1, result.Data.PageIndex);
        //    Assert.Equal(0, result.Data.TotalCount);
        //    Assert.Equal(0, result.Data.TotalPages);
        //}

        //[Fact]
        //public async Task GetPaginatedDailyReportList_NoRelatedData_ReturnsEmptyRelatedLists()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        IsActive = true,
        //        LivestockCircleId = livestockCircleId,
        //        Note = "Report 1",
        //        Status = "today",
        //        CreatedDate = DateTime.UtcNow,
        //        CreatedBy = _userId
        //    };
        //    var options = new DbContextOptionsBuilder<TestDbContext2>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext2(options);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
        //        .Returns(context.DailyReports);
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
        //        .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
        //    _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
        //        .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" }
        //    };

        //    // Act
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Single(result.Data.Items);
        //    var report = result.Data.Items.First();
        //    Assert.Equal(dailyReport.Id, report.Id);
        //    Assert.Empty(report.ImageLinks);
        //    Assert.Null(report.Thumbnail);
        //    Assert.Empty(report.FoodReports);
        //    Assert.Empty(report.MedicineReports);
        //}

        //[Fact]
        //public async Task GetPaginatedDailyReportList_MissingFoodAndMedicineDetails_ReturnsPartialData()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var dailyReport = new DailyReport
        //    {
        //        Id = Guid.NewGuid(),
        //        IsActive = true,
        //        LivestockCircleId = livestockCircleId,
        //        Note = "Report 1",
        //        Status = "today",
        //        CreatedDate = DateTime.UtcNow,
        //        CreatedBy = _userId
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
        //    var options = new DbContextOptionsBuilder<TestDbContext2>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext2(options);
        //    context.DailyReports.Add(dailyReport);
        //    context.FoodReports.Add(foodReport);
        //    context.MedicineReports.Add(medicineReport);
        //    context.SaveChanges();

        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Returns((Expression<Func<DailyReport, bool>> expr) => context.DailyReports.Where(expr));
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable())
        //        .Returns(context.DailyReports);
        //    _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
        //        .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _imageDailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageDailyReport, bool>>>()))
        //        .Returns((Expression<Func<ImageDailyReport, bool>> expr) => context.ImageDailyReports.Where(expr));
        //    _foodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Food, bool>>>()))
        //        .Returns((Expression<Func<Food, bool>> expr) => context.Foods.Where(expr));
        //    _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
        //        .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Where(expr));
        //    _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
        //        .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "CreatedDate", Value = "desc" }
        //    };

        //    // Act
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(request, livestockCircleId, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy danh sách báo cáo hàng ngày phân trang thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Single(result.Data.Items);
        //    var report = result.Data.Items.First();
        //    Assert.Single(report.FoodReports);
        //    //Assert.Equal(foodReport.FoodId, report.FoodReports[0].Food.Id);
        //    //Assert.Null(report.FoodReports[0].Food.FoodName);
        //    //Assert.Null(report.FoodReports[0].Food.Thumbnail);
        //    //Assert.Single(report.MedicineReports);
        //    //Assert.Equal(medicineReport.MedicineId, report.MedicineReports[0].Medicine.Id);
        //    //Assert.Null(report.MedicineReports[0].Medicine.MedicineName);
        //    //Assert.Null(report.MedicineReports[0].Medicine.Thumbnail);
        //}

        //[Fact]
        //public async Task GetPaginatedDailyReportList_RepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    _dailyReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<DailyReport, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act
        //    var result = await _dailyReportService.GetPaginatedDailyReportList(request, null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy danh sách phân trang", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database error", result.Errors.First());
        //}
    }

    // Extended InMemory DbContext for test
    public class TestDbContext2 : DbContext
    {
        public TestDbContext2(DbContextOptions<TestDbContext2> options) : base(options) { }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<ImageDailyReport> ImageDailyReports { get; set; }
        public DbSet<FoodReport> FoodReports { get; set; }
        public DbSet<MedicineReport> MedicineReports { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<ImageFood> ImageFoods { get; set; }
        public DbSet<ImageMedicine> ImageMedicines { get; set; }
    }
}