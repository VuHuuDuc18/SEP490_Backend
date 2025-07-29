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
    public class GetFoodReportDetailsTest
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

        public GetFoodReportDetailsTest()
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
        public async Task GetFoodReportDetails_NullRequest_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, null, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được null", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Yêu cầu không được null", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InvalidPageIndex_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InvalidPageSize_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 0 };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReport1, foodReport2);
            context.Foods.Add(food1);
            context.ImageFoods.Add(imageFood1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReport1, foodReport2);
            context.Foods.Add(food1);
            context.ImageFoods.Add(imageFood1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("InvalidField", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InvalidSortField_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReport1, foodReport2);
            context.Foods.Add(food1);
            context.ImageFoods.Add(imageFood1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Single(result.Errors);
            Assert.Contains("Trường hợp lệ", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_NonExistentDailyReport_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync((DailyReport)null);

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_InactiveDailyReport_ReturnsError()
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
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Không tìm thấy báo cáo hàng ngày", result.Errors.First());
        }

        [Fact]
        public async Task GetFoodReportDetails_Success_ReturnsPaginatedFoodReports()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var foodReports = new List<FoodReport> { foodReport1, foodReport2 };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var food2 = new Food
            {
                Id = foodId2,
                FoodName = "Food 2",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };
            var imageFood2 = new ImageFood
            {
                FoodId = foodId2,
                ImageLink = "image2.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReports);
            context.Foods.AddRange(food1, food2);
            context.ImageFoods.AddRange(imageFood1, imageFood2);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync(food2);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            var foodReportResponse1 = result.Data.Items.FirstOrDefault(x => x.Food.Id == foodId1);
            var foodReportResponse2 = result.Data.Items.FirstOrDefault(x => x.Food.Id == foodId2);
            Assert.NotNull(foodReportResponse1);
            Assert.NotNull(foodReportResponse2);
            //Assert.Equal(foodReport1.Quantity, foodReportResponse1.Quantity);
            //Assert.Equal(food1.FoodName, foodReportResponse1.Food.FoodName);
            //Assert.Equal(imageFood1.ImageLink, foodReportResponse1.Food.Thumbnail);
            //Assert.Equal(foodReport2.Quantity, foodReportResponse2.Quantity);
            //Assert.Equal(food2.FoodName, foodReportResponse2.Food.FoodName);
            //Assert.Equal(imageFood2.ImageLink, foodReportResponse2.Food.Thumbnail);
            //Assert.True(result.Data.Items[0].Quantity <= result.Data.Items[1].Quantity, "Items should be sorted by Quantity ascending");
        }

        [Fact]
        public async Task GetFoodReportDetails_Success_WithFilter()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReport1, foodReport2);
            context.Foods.Add(food1);
            context.ImageFoods.Add(imageFood1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Quantity", Value = "10" } }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            var foodReportResponse = result.Data.Items.FirstOrDefault();
            Assert.NotNull(foodReportResponse);
            Assert.Equal(foodId1, foodReportResponse.Food.Id);
            Assert.Equal(10, foodReportResponse.Quantity);
            Assert.Equal("Food 1", foodReportResponse.Food.FoodName);
            Assert.Equal("image1.jpg", foodReportResponse.Food.Thumbnail);
        }

        [Fact]
        public async Task GetFoodReportDetails_Success_WithSearch()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId1 = Guid.NewGuid();
            var foodId2 = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport1 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId1,
                Quantity = 10,
                IsActive = true
            };
            var foodReport2 = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId2,
                Quantity = 20,
                IsActive = true
            };
            var food1 = new Food
            {
                Id = foodId1,
                FoodName = "Food 1",
                IsActive = true
            };
            var imageFood1 = new ImageFood
            {
                FoodId = foodId1,
                ImageLink = "image1.jpg",
                Thumnail = "true"
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.AddRange(foodReport1, foodReport2);
            context.Foods.Add(food1);
            context.ImageFoods.Add(imageFood1);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId1, null))
                .ReturnsAsync(food1);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId2, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "FoodId", Value = foodId1.ToString() } }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
            var foodReportResponse = result.Data.Items.FirstOrDefault();
            Assert.NotNull(foodReportResponse);
            Assert.Equal(foodId1, foodReportResponse.Food.Id);
            Assert.Equal(10, foodReportResponse.Quantity);
            Assert.Equal("Food 1", foodReportResponse.Food.FoodName);
            Assert.Equal("image1.jpg", foodReportResponse.Food.Thumbnail);
        }

        [Fact]
        public async Task GetFoodReportDetails_NoFoodReports_ReturnsEmptyList()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(0, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetFoodReportDetails_NoFoodDetailsOrImages_ReturnsPartialData()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var dailyReport = new DailyReport
            {
                Id = reportId,
                IsActive = true,
                LivestockCircleId = Guid.NewGuid(),
                Note = "test",
                Status = "today",
                CreatedDate = DateTime.UtcNow
            };
            var foodReport = new FoodReport
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                FoodId = foodId,
                Quantity = 10,
                IsActive = true
            };

            // Setup InMemory DbContext
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.DailyReports.Add(dailyReport);
            context.FoodReports.Add(foodReport);
            context.SaveChanges();

            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ReturnsAsync(dailyReport);
            _foodReportRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<FoodReport, bool>>>()))
                .Returns((Expression<Func<FoodReport, bool>> expr) => context.FoodReports.Where(expr));
            _foodReportRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.FoodReports);
            _foodRepositoryMock.Setup(x => x.GetByIdAsync(foodId, null))
                .ReturnsAsync((Food)null);
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns((Expression<Func<ImageFood, bool>> expr) => context.ImageFoods.Where(expr));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Quantity", Value = "asc" }
            };

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy chi tiết báo cáo thức ăn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            //var foodReportResponse = result.Data.Items.First();
            //Assert.Equal(foodId, foodReportResponse.Food.Id);
            //Assert.Equal(10, foodReportResponse.Quantity);
            //Assert.Null(foodReportResponse.Food.FoodName);
            //Assert.Null(foodReportResponse.Food.Thumbnail);
        }

        [Fact]
        public async Task GetFoodReportDetails_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var request = new ListingRequest { PageIndex = 1, PageSize = 10 };
            _dailyReportRepositoryMock.Setup(x => x.GetByIdAsync(reportId, null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _dailyReportService.GetFoodReportDetails(reportId, request, default);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy chi tiết báo cáo thức ăn", result.Message);
            Assert.Single(result.Errors);
            Assert.Equal("Database error", result.Errors.First());
        }
    }

    // Minimal InMemory DbContext for test
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<DailyReport> DailyReports { get; set; }
        public DbSet<FoodReport> FoodReports { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<ImageFood> ImageFoods { get; set; }
    }
}