using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response;
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
    public class GetMedicineReportDetailsTest
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

        public GetMedicineReportDetailsTest()
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
        //public async Task GetMedicineReportDetails_NullRequest_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();

        //    // Act
        //    var result = await _dailyReportService.GetMedicineReportDetails(reportId, null, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Yêu cầu không được null", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Yêu cầu không được null", result.Errors.First());
        //}

        [Fact]
        public async Task GetMedicineReportDetails_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetMedicineReportDetails_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetMedicineReportDetails_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReport1, medicineReport2);
            context.Medicines.Add(medicine1);
            context.ImageMedicines.Add(imageMedicine1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync((Medicine)null);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetMedicineReportDetails_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReport1, medicineReport2);
            context.Medicines.Add(medicine1);
            context.ImageMedicines.Add(imageMedicine1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync((Medicine)null);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetMedicineReportDetails_InvalidSortField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReport1, medicineReport2);
            context.Medicines.Add(medicine1);
            context.ImageMedicines.Add(imageMedicine1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync((Medicine)null);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("Trường hợp lệ", result.Errors.First());
        }

        //[Fact]
        //public async Task GetMedicineReportDetails_NonExistentDailyReport_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
        //        .ReturnsAsync((DailyReport)null);

        //    // Act
        //    var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Errors.First());
        //}

        [Fact]
        public async Task GetMedicineReportDetails_InactiveDailyReport_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = false
            };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Errors.First());
        }

        [Fact]
        public async Task GetMedicineReportDetails_Success_ReturnsPaginatedMedicineReports()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicineReports = new List<MedicineReport> { medicineReport1, medicineReport2 };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var medicine2 = new Medicine
            {
                Id = medicineId2,
                MedicineName = "Medicine 2",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };
            var imageMedicine2 = new ImageMedicine
            {
                MedicineId = medicineId2,
                ImageLink = "image2.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReports);
            context.Medicines.AddRange(medicine1, medicine2);
            context.ImageMedicines.AddRange(imageMedicine1, imageMedicine2);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync(medicine2);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            var medicineReportResponse1 = result.Data.Items.FirstOrDefault(x => x.Medicine.Id == medicineId1);
            var medicineReportResponse2 = result.Data.Items.FirstOrDefault(x => x.Medicine.Id == medicineId2);
            Assert.NotNull(medicineReportResponse1);
            Assert.NotNull(medicineReportResponse2);
            Assert.Equal(medicineReport1.Quantity, medicineReportResponse1.Quantity);
            Assert.Equal(medicine1.MedicineName, medicineReportResponse1.Medicine.MedicineName);
            Assert.Equal(imageMedicine1.ImageLink, medicineReportResponse1.Medicine.Thumbnail);
            Assert.Equal(medicineReport2.Quantity, medicineReportResponse2.Quantity);
            Assert.Equal(medicine2.MedicineName, medicineReportResponse2.Medicine.MedicineName);
            Assert.Equal(imageMedicine2.ImageLink, medicineReportResponse2.Medicine.Thumbnail);
            Assert.True(result.Data.Items[0].Quantity <= result.Data.Items[1].Quantity, "Items should be sorted by Quantity ascending");
        }

        [Fact]
        public async Task GetMedicineReportDetails_Success_WithFilter()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReport1, medicineReport2);
            context.Medicines.Add(medicine1);
            context.ImageMedicines.Add(imageMedicine1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync((Medicine)null);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Quantity", Value = "5" } }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            var medicineReportResponse = result.Data.Items.FirstOrDefault();
            Assert.NotNull(medicineReportResponse);
            Assert.Equal(medicineId1, medicineReportResponse.Medicine.Id);
            Assert.Equal(5, medicineReportResponse.Quantity);
            Assert.Equal("Medicine 1", medicineReportResponse.Medicine.MedicineName);
            Assert.Equal("image1.jpg", medicineReportResponse.Medicine.Thumbnail);
        }

        [Fact]
        public async Task GetMedicineReportDetails_Success_WithSearch()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var medicineId1 = Guid.NewGuid();
            var medicineId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var medicineReport1 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId1,
                Quantity = 5,
                IsActive = true
            };
            var medicineReport2 = new MedicineReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                MedicineId = medicineId2,
                Quantity = 10,
                IsActive = true
            };
            var medicine1 = new Medicine
            {
                Id = medicineId1,
                MedicineName = "Medicine 1",
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicineId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext1(options);
            context.DailyReports.Add(dailyReport);
            context.MedicineReports.AddRange(medicineReport1, medicineReport2);
            context.Medicines.Add(medicine1);
            context.ImageMedicines.Add(imageMedicine1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
                .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
            _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.MedicineReports);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId1, null))
                .ReturnsAsync(medicine1);
            _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId2, null))
                .ReturnsAsync((Medicine)null);
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "MedicineId", Value = medicineId1.ToString() } }
            };

            // Act
            var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thuốc thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            var medicineReportResponse = result.Data.Items.FirstOrDefault();
            Assert.NotNull(medicineReportResponse);
            Assert.Equal(medicineId1, medicineReportResponse.Medicine.Id);
            Assert.Equal(5, medicineReportResponse.Quantity);
            Assert.Equal("Medicine 1", medicineReportResponse.Medicine.MedicineName);
            Assert.Equal("image1.jpg", medicineReportResponse.Medicine.Thumbnail);
        }

        //[Fact]
        //public async Task GetMedicineReportDetails_NoMedicineReports_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var dailyReport = new DailyReport
        //    {
        //        Id = reportId,
        //        IsActive = true,
        //        LivestockCircleId = Guid.NewGuid(),
        //        Note = "test",
        //        Status = "today",
        //        CreatedDate = DateTime.UtcNow
        //    };

        //    // Setup InMemory DbContext
        //    var options = new DbContextOptionsBuilder<TestDbContext1>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext1(options);
        //    context.DailyReports.Add(dailyReport);
        //    context.SaveChanges();

        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
        //        .ReturnsAsync(dailyReport);
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
        //        .Returns(context.MedicineReports);

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
        //    };

        //    // Act
        //    var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy chi tiết báo cáo thuốc thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Empty(result.Data.Items);
        //    Assert.Equal(1, result.Data.PageIndex);
        //    Assert.Equal(0, result.Data.TotalCount);
        //    Assert.Equal(0, result.Data.TotalPages);
        //}

        //[Fact]
        //public async Task GetMedicineReportDetails_NoMedicineDetailsOrImages_ReturnsPartialData()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var medicineId = Guid.NewGuid();
        //    var dailyReport = new DailyReport
        //    {
        //        Id = reportId,
        //        IsActive = true,
        //        LivestockCircleId = Guid.NewGuid(),
        //        Note = "test",
        //        Status = "today",
        //        CreatedDate = DateTime.UtcNow
        //    };
        //    var medicineReport = new MedicineReport
        //    {
        //        Id = Guid.NewGuid(),
        //        ReportId = reportId,
        //        MedicineId = medicineId,
        //        Quantity = 5,
        //        IsActive = true
        //    };

        //    // Setup InMemory DbContext
        //    var options = new DbContextOptionsBuilder<TestDbContext1>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestDbContext1(options);
        //    context.DailyReports.Add(dailyReport);
        //    context.MedicineReports.Add(medicineReport);
        //    context.SaveChanges();

        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
        //        .ReturnsAsync(dailyReport);
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<MedicineReport, bool>>>()))
        //        .Returns((Expression<Func<MedicineReport, bool>> expr) => context.MedicineReports.Where(expr));
        //    _medicineReportRepositoryMock.Setup(x => x.GetQueryable())
        //        .Returns(context.MedicineReports);
        //    _medicineRepositoryMock.Setup(x => x.GetByIdAsync(medicineId, null))
        //        .ReturnsAsync((Medicine)null);
        //    _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
        //        .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

        //    var request = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
        //    };

        //    // Act
        //    var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

        //    // Assert
        //    Assert.True(result.Succeeded, $"Service message: {result.Message}");
        //    Assert.Equal("Lấy chi tiết báo cáo thuốc thành công", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal(1, result.Data.Items.Count);
        //    //var medicineReportResponse = result.Data.Items.First();
        //    //Assert.Equal(medicineId, medicineReportResponse.Medicine.Id);
        //    //Assert.Equal(5, medicineReportResponse.Quantity);
        //    //Assert.Null(medicineReportResponse.Medicine.MedicineName);
        //    //Assert.Null(medicineReportResponse.Medicine.Thumbnail);
        //}

        //[Fact]
        //public async Task GetMedicineReportDetails_RepositoryThrowsException_ReturnsError()
        //{
        //    // Arrange
        //    var reportId = Guid.NewGuid();
        //    var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
        //        .ThrowsAsync(new Exception("Database error"));

        //    // Act
        //    var result = await _dailyReportService.GetMedicineReportDetails(reportId, request, default);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Lỗi khi lấy chi tiết báo cáo thuốc", result.Message);
        //    Assert.Single(result.Errors);
        //    Assert.Equal("Database error", result.Errors.First());
        //}
    }

    // Minimal InMemory DbContext for test
    public class TestDbContext1 : DbContext
    {
        public TestDbContext1(DbContextOptions<TestDbContext1> options) : base(options) { }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<MedicineReport> MedicineReports { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<ImageMedicine> ImageMedicines { get; set; }
    }
}