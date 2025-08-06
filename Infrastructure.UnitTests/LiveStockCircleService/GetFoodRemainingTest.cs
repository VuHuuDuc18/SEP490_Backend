using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.IServices;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;
using System.Linq.Expressions;
using Domain.Dto.Response;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<LivestockCircleFood> LivestockCircleFoods { get; set; }
        public DbSet<Food> Foods { get; set; }

        public DbSet<ImageFood> ImageFoods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LivestockCircleFood>()
                .HasOne(lcf => lcf.Food)
                .WithMany()
                .HasForeignKey(lcf => lcf.FoodId);

            modelBuilder.Entity<ImageFood>()
               .HasKey(ifood => new { ifood.FoodId, ifood.ImageLink }); 
        }
    }
    public class GetFoodRemainingTest
    {
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _livestockCircleImageRepoMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly LivestockCircleService _service;

        public GetFoodRemainingTest()
        {
            // Mock tất cả các repository và dịch vụ
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _livestockCircleImageRepoMock = new Mock<IRepository<ImageLivestockCircle>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _foodImageRepositoryMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _livestockCircleImageRepoMock = new Mock<IRepository<ImageLivestockCircle>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());

            // Khởi tạo service với các mock
            _service = new LivestockCircleService(
                _livestockCircleRepositoryMock.Object,
                _livestockCircleImageRepoMock.Object,
                _userRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _livestockCircleFoodRepositoryMock.Object,
                _livestockCircleMedicineRepositoryMock.Object,
                _foodRepositoryMock.Object,
                _medicineRepositoryMock.Object,
                _foodImageRepositoryMock.Object,
                _medicineImageRepositoryMock.Object,
                _livestockCircleImageRepoMock.Object,
                _cloudinaryCloudServiceMock.Object
            );
        }

        [Fact]
        public async Task GetFoodRemaining_NullRequest_ReturnsError()
        {
            // Arrange
            ListingRequest request = null;

            // Act
            var result = await _service.GetFoodRemaining(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Yêu cầu không được để trống", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Yêu cầu không được để trống", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), null), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_InvalidPageIndexOrPageSize_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest { PageIndex = 0, PageSize = 10 };

            // Act
            var result = await _service.GetFoodRemaining(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("PageIndex và PageSize phải lớn hơn 0", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), null), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_InvalidFilterField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "Test" } }
            };

            // Act
            var result = await _service.GetFoodRemaining(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Trường lọc không hợp lệ: InvalidField", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), null), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_InvalidSearchField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "Test" } }
            };

            // Act
            var result = await _service.GetFoodRemaining(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Trường tìm kiếm không hợp lệ: InvalidField", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), null), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_InvalidSortField_ReturnsError()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _service.GetFoodRemaining(Guid.NewGuid(), request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Trường sắp xếp không hợp lệ: InvalidField", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("Trường hợp lệ:", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), null), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_NonExistentLivestockCircle_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Remaining", Value = "asc" }
            };

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _service.GetFoodRemaining(livestockCircleId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Chu kỳ chăn nuôi không tồn tại", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Chu kỳ chăn nuôi không tồn tại", result.Errors[0]);
            Assert.Null(result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, null), Times.Once());
            _livestockCircleFoodRepositoryMock.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()), Times.Never());
        }

        [Fact]
        public async Task GetFoodRemaining_Success_ReturnsPaginatedData()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };
            var food = new Food { Id = foodId, FoodName = "Test Food" };
            var livestockCircleFood = new LivestockCircleFood
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircleId,
                FoodId = foodId,
                Food = food,
                Remaining = 100
            };
            var imageFood = new ImageFood { FoodId = foodId, ImageLink = "thumbnail.jpg", Thumnail = "true" };

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.Foods.Add(food);
            context.LivestockCircleFoods.Add(livestockCircleFood);
            context.ImageFoods.Add(imageFood);
            await context.SaveChangesAsync();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns(context.LivestockCircleFoods
                    .Include(lcf => lcf.Food)
                    .Where(lcf => lcf.LivestockCircleId == livestockCircleId && lcf.Remaining > 0));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns(context.ImageFoods.Where(ifood => ifood.FoodId == foodId && ifood.Thumnail == "true"));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Remaining", Value = "asc" }
            };

            // Act
            var result = await _service.GetFoodRemaining(livestockCircleId, request);

            // Assert
            Assert.True(result.Succeeded, $"Expected Succeeded = true, but got Succeeded = false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách thực phẩm còn lại thành công", result.Message);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal(1, result.Data.PageIndex);
           // Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);

            var item = result.Data.Items.First();
            Assert.Equal(livestockCircleFood.Id, item.Id);
            Assert.Equal(livestockCircleId, item.LivestockCircleId);
            Assert.Equal(foodId, item.Food.Id);
            Assert.Equal("Test Food", item.Food.FoodName);
            Assert.Equal("thumbnail.jpg", item.Food.Thumbnail);
            Assert.Equal(100, item.Remaining);
        }

        [Fact]
        public async Task GetFoodRemaining_Success_EmptyResult()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns(context.LivestockCircleFoods
                    .Include(lcf => lcf.Food)
                    .Where(lcf => lcf.LivestockCircleId == livestockCircleId && lcf.Remaining > 0));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns(new List<ImageFood>().AsQueryable());

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Remaining", Value = "asc" }
            };

            // Act
            var result = await _service.GetFoodRemaining(livestockCircleId, request);

            // Assert
            Assert.True(result.Succeeded, $"Expected Succeeded = true, but got Succeeded = false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách thực phẩm còn lại thành công", result.Message);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(1, result.Data.PageIndex);
           // Assert.Equal(10, result.Data.PageSize);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(0, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetFoodRemaining_Success_WithSearchAndFilter()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };
            var food = new Food { Id = foodId, FoodName = "Test Food" };
            var livestockCircleFood = new LivestockCircleFood
            {
                Id = Guid.NewGuid(),
                LivestockCircleId = livestockCircleId,
                FoodId = foodId,
                Food = food,
                Remaining = 100
            };
            var imageFood = new ImageFood { FoodId = foodId, ImageLink = "thumbnail.jpg", Thumnail = "true" };

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestDbContext(options);
            context.Foods.Add(food);
            context.LivestockCircleFoods.Add(livestockCircleFood);
            context.ImageFoods.Add(imageFood);
            await context.SaveChangesAsync();

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ReturnsAsync(livestockCircle);
            _livestockCircleFoodRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<LivestockCircleFood, bool>>>()))
                .Returns(context.LivestockCircleFoods
                    .Include(lcf => lcf.Food)
                    .Where(lcf => lcf.LivestockCircleId == livestockCircleId && lcf.Remaining == 100));
            _foodImageRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageFood, bool>>>()))
                .Returns(context.ImageFoods.Where(ifood => ifood.FoodId == foodId && ifood.Thumnail == "true"));

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Remaining", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Remaining", Value = "100" } },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Remaining", Value = "100" } }
            };

            // Act
            var result = await _service.GetFoodRemaining(livestockCircleId, request);

            // Assert
            Assert.True(result.Succeeded, $"Expected Succeeded = true, but got Succeeded = false. Message: {result.Message}, Errors: {string.Join(", ", result.Errors ?? new List<string>())}");
            Assert.Equal("Lấy danh sách thực phẩm còn lại thành công", result.Message);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Equal(livestockCircleFood.Id, result.Data.Items.First().Id);
            Assert.Equal(100, result.Data.Items.First().Remaining);
        }

        [Fact]
        public async Task GetFoodRemaining_ExceptionThrown_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Remaining", Value = "asc" }
            };

            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetFoodRemaining(livestockCircleId, request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi lấy danh sách thực phẩm còn lại", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Database error", result.Errors[0]);
            Assert.Null(result.Data);
        }
    }
}
