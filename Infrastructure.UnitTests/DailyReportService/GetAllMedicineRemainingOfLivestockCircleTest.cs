using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
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
    public class GetAllMedicineRemainingOfLivestockCircleTest
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

        public GetAllMedicineRemainingOfLivestockCircleTest()
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
        public async Task GetAllMedicineRemainingOfLivestockCircle_MedicinesWithRelatedData_ReturnsMedicineResponses()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var medicineCategory1 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
            var medicineCategory2 = new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 2", Description = "Desc 2" };
            var medicine1 = new Medicine
            {
                Id = Guid.NewGuid(),
                MedicineName = "Medicine 1",
                MedicineCategoryId = medicineCategory1.Id,
                MedicineCategory = medicineCategory1,
                Stock = 100,
                IsActive = true
            };
            var medicine2 = new Medicine
            {
                Id = Guid.NewGuid(),
                MedicineName = "Medicine 2",
                MedicineCategoryId = medicineCategory2.Id,
                MedicineCategory = medicineCategory2,
                Stock = 200,
                IsActive = true
            };
            var livestockCircleMedicine1 = new LivestockCircleMedicine
            {
                LivestockCircleId = livestockCircleId,
                MedicineId = medicine1.Id,
                IsActive = true
            };
            var livestockCircleMedicine2 = new LivestockCircleMedicine
            {
                LivestockCircleId = livestockCircleId,
                MedicineId = medicine2.Id,
                IsActive = true
            };
            var imageMedicine1 = new ImageMedicine
            {
                MedicineId = medicine1.Id,
                ImageLink = "image1.jpg",
                Thumnail = "false"
            };
            var imageMedicine2 = new ImageMedicine
            {
                MedicineId = medicine1.Id,
                ImageLink = "thumbnail1.jpg",
                Thumnail = "true"
            };
            var imageMedicine3 = new ImageMedicine
            {
                MedicineId = medicine2.Id,
                ImageLink = "image2.jpg",
                Thumnail = "false"
            };

            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.MedicineCategories.AddRange(medicineCategory1, medicineCategory2);
            context.Medicines.AddRange(medicine1, medicine2);
            context.LivestockCircleMedicines.AddRange(livestockCircleMedicine1, livestockCircleMedicine2);
            context.ImageMedicines.AddRange(imageMedicine1, imageMedicine2, imageMedicine3);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
           // Assert.Equal(2, result.Count);
            var medicineResponse1 = result.FirstOrDefault(r => r.Id == medicine1.Id);
            Assert.NotNull(medicineResponse1);
            Assert.Equal(medicine1.MedicineName, medicineResponse1.MedicineName);
            Assert.Equal(medicine1.Stock, medicineResponse1.Stock);
            Assert.True(medicineResponse1.IsActive);
            Assert.Equal(medicineCategory1.Id, medicineResponse1.MedicineCategory.Id);
            Assert.Equal(medicineCategory1.Name, medicineResponse1.MedicineCategory.Name);
            Assert.Equal(medicineCategory1.Description, medicineResponse1.MedicineCategory.Description);
            Assert.Single(medicineResponse1.ImageLinks);
            Assert.Contains("image1.jpg", medicineResponse1.ImageLinks);
            Assert.Equal("thumbnail1.jpg", medicineResponse1.Thumbnail);

            var medicineResponse2 = result.FirstOrDefault(r => r.Id == medicine2.Id);
            Assert.NotNull(medicineResponse2);
            Assert.Equal(medicine2.MedicineName, medicineResponse2.MedicineName);
            Assert.Equal(medicine2.Stock, medicineResponse2.Stock);
            Assert.True(medicineResponse2.IsActive);
            Assert.Equal(medicineCategory2.Id, medicineResponse2.MedicineCategory.Id);
            Assert.Equal(medicineCategory2.Name, medicineResponse2.MedicineCategory.Name);
            Assert.Equal(medicineCategory2.Description, medicineResponse2.MedicineCategory.Description);
            Assert.Single(medicineResponse2.ImageLinks);
            Assert.Contains("image2.jpg", medicineResponse2.ImageLinks);
            Assert.Null(medicineResponse2.Thumbnail);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_MedicineWithNoImages_ReturnsMedicineResponse()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var medicineCategory = new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                MedicineName = "Medicine 1",
                MedicineCategoryId = medicineCategory.Id,
                MedicineCategory = medicineCategory,
                Stock = 100,
                IsActive = true
            };
            var livestockCircleMedicine = new LivestockCircleMedicine
            {
                LivestockCircleId = livestockCircleId,
                MedicineId = medicine.Id,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.MedicineCategories.Add(medicineCategory);
            context.Medicines.Add(medicine);
            context.LivestockCircleMedicines.Add(livestockCircleMedicine);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Single(result);
            var medicineResponse = result.First();
            Assert.Equal(medicine.Id, medicineResponse.Id);
            Assert.Equal(medicine.MedicineName, medicineResponse.MedicineName);
            Assert.Equal(medicine.Stock, medicineResponse.Stock);
            Assert.True(medicineResponse.IsActive);
            Assert.Equal(medicineCategory.Id, medicineResponse.MedicineCategory.Id);
            Assert.Equal(medicineCategory.Name, medicineResponse.MedicineCategory.Name);
            Assert.Equal(medicineCategory.Description, medicineResponse.MedicineCategory.Description);
            Assert.Empty(medicineResponse.ImageLinks);
            Assert.Null(medicineResponse.Thumbnail);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_NoLivestockCircleMedicines_ReturnsEmptyList()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_LivestockCircleMedicineButNoActiveMedicines_ReturnsEmptyList()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                MedicineName = "Medicine 1",
                MedicineCategoryId = Guid.NewGuid(),
                MedicineCategory = new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" },
                Stock = 100,
                IsActive = false // Inactive medicine
            };
            var livestockCircleMedicine = new LivestockCircleMedicine
            {
                LivestockCircleId = livestockCircleId,
                MedicineId = medicine.Id,
                IsActive = true
            };

            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.Medicines.Add(medicine);
            context.LivestockCircleMedicines.Add(livestockCircleMedicine);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_InactiveLivestockCircleMedicine_ReturnsEmptyList()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var medicineCategory = new MedicineCategory { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" };
            var medicine = new Medicine
            {
                Id = Guid.NewGuid(),
                MedicineName = "Medicine 1",
                MedicineCategoryId = medicineCategory.Id,
                MedicineCategory = medicineCategory,
                Stock = 100,
                IsActive = true
            };
            var livestockCircleMedicine = new LivestockCircleMedicine
            {
                LivestockCircleId = livestockCircleId,
                MedicineId = medicine.Id,
                IsActive = false // Inactive LivestockCircleMedicine
            };

            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.MedicineCategories.Add(medicineCategory);
            context.Medicines.Add(medicine);
            context.LivestockCircleMedicines.Add(livestockCircleMedicine);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_EmptyLivestockCircleId_ReturnsEmptyList()
        {
            // Arrange
            var livestockCircleId = Guid.Empty;
            var options = new DbContextOptionsBuilder<TestDbContext6>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext6(options);
            context.SaveChanges();

            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Returns((Expression<Func<LivestockCircleMedicine, bool>> expr) => context.LivestockCircleMedicines.Where(expr));
            _medicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Medicine, bool>>>()))
                .Returns((Expression<Func<Medicine, bool>> expr) => context.Medicines.Include(m => m.MedicineCategory).Where(expr));
            _medicineImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageMedicine, bool>>>()))
                .Returns((Expression<Func<ImageMedicine, bool>> expr) => context.ImageMedicines.Where(expr));

            // Act
            var result = await _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, default));
        }

        [Fact]
        public async Task GetAllMedicineRemainingOfLivestockCircle_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var cts = new CancellationTokenSource();
            _livestockCircleMedicineRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleMedicine, bool>>>()))
                .Throws(new OperationCanceledException("Operation cancelled"));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _dailyReportService.GetAllMedicineRemainingOfLivestockCircle(livestockCircleId, cts.Token));
        }

        
    }

    // InMemory DbContext for testing
    public class TestDbContext6 : DbContext
    {
        public TestDbContext6(DbContextOptions<TestDbContext6> options) : base(options) { }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedicineCategory> MedicineCategories { get; set; }
        public DbSet<LivestockCircleMedicine> LivestockCircleMedicines { get; set; }
        public DbSet<ImageMedicine> ImageMedicines { get; set; }
    }
}