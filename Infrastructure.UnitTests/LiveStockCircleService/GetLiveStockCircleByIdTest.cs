using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Core;
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
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MockQueryable.EntityFrameworkCore;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class GetLiveStockCircleByIdTest
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

        public GetLiveStockCircleByIdTest()
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
public async Task GetLiveStockCircleById_Successful()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var technicalStaffId = Guid.NewGuid();
            var breedCategoryId = Guid.NewGuid();

            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                LivestockCircleName = "Circle 1",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = breedId,
                BarnId = barnId,
                TechicalStaffId = technicalStaffId,
                IsActive = true,
                PreSoldDate = null,
                ReleaseDate = null,
                SamplePrice = null
            };

            var breedCategory = new BreedCategory
            {
                Id = breedCategoryId,
                Name = "Category 1",
                Description = "Test Category"
            };

            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed",
                Stock = 100,
                IsActive = true,
                BreedCategoryId = breedCategoryId,
                BreedCategory = breedCategory
            };

            var imageBreeds = new List<ImageBreed>
    {
        new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId, Thumnail= "test", ImageLink = "image1.jpg" },
        new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId,Thumnail= "test", ImageLink = "image2.jpg" }
    };

            var user = new User
            {
                Id = technicalStaffId,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890"
            };

            // Setup In-Memory DbContext
            var options = new DbContextOptionsBuilder<TestLivestockCircleDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestLivestockCircleDbContext2(options);
            context.LivestockCircles.Add(livestockCircle);
            context.BreedCategories.Add(breedCategory);
            context.Breeds.Add(breed);
            context.ImageBreeds.AddRange(imageBreeds);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Mock repositories to use the in-memory context
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(context.LivestockCircles.Find(livestockCircleId));

            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
                .Returns((Expression<Func<ImageBreed, bool>> expr) => context.ImageBreeds.Where(expr));

           _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Breed, bool>>>()))
                .Returns((Expression<Func<Breed, bool>> expr) => context.Breeds.Include(b => b.BreedCategory).Where(expr));

            _userRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns((Expression<Func<User, bool>> expr) => context.Users.Where(expr));

            // Act
            var result = await _service.GetLiveStockCircleById(livestockCircleId);

            // Assert
            Assert.NotNull(result.Circle);
            //Assert.Null(result.ErrorMessage);
            //Assert.Equal(livestockCircleId, result.Circle.Id);
            //Assert.Equal("Circle 1", result.Circle.LivestockCircleName);
            //Assert.Equal("Active", result.Circle.Status);
            //Assert.Equal(livestockCircle.TotalUnit, result.Circle.TotalUnit);
            //Assert.Equal(livestockCircle.AverageWeight, result.Circle.AverageWeight);
            //Assert.Equal(livestockCircle.GoodUnitNumber, result.Circle.GoodUnitNumber);
            //Assert.Equal(livestockCircle.BadUnitNumber, result.Circle.BadUnitNumber);
            //Assert.Equal(breedId, result.Circle.BreedId);
            //Assert.NotNull(result.Circle.Breed);
            //Assert.Equal("Test Breed", result.Circle.Breed.BreedName);
            //Assert.Equal(2, result.Circle.Breed.ImageLinks.Count);
            //Assert.Contains("image1.jpg", result.Circle.Breed.ImageLinks);
            //Assert.Contains("image2.jpg", result.Circle.Breed.ImageLinks);
            //Assert.NotNull(result.Circle.Breed.BreedCategory);
            //Assert.Equal("Category 1", result.Circle.Breed.BreedCategory.Name);
            //Assert.NotNull(result.Circle.TechicalStaff);
            //Assert.Equal("John Doe", result.Circle.TechicalStaff.Fullname);
            //Assert.Equal("john.doe@example.com", result.Circle.TechicalStaff.Email);
        }

        [Fact]
        public async Task GetLiveStockCircleById_NotFound()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _service.GetLiveStockCircleById(livestockCircleId);

            // Assert
            Assert.Null(result.Circle);
            Assert.Equal("Không tìm thấy chu kỳ chăn nuôi.", result.ErrorMessage);
        }

        //[Fact]
        //public async Task GetLiveStockCircleById_CheckErrorFromRepository()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var checkError = new Ref<CheckError> { Value = new CheckError { isError = true, Message = "Repository error" } };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .Callback((Guid id, Ref<CheckError> error) => error.Value = checkError.Value)
        //        .ReturnsAsync((LivestockCircle)null);

        //    // Act
        //    var result = await _service.GetLiveStockCircleById(livestockCircleId);

        //    // Assert
        //    Assert.Null(result.Circle);
        //    Assert.Equal("Lỗi khi lấy thông tin chu kỳ chăn nuôi: Repository error", result.ErrorMessage);
        //}
    }
    public class TestLivestockCircleDbContext2 : DbContext
    {
        public TestLivestockCircleDbContext2(DbContextOptions<TestLivestockCircleDbContext2> options) : base(options) { }

        public DbSet<LivestockCircle> LivestockCircles { get; set; }
        public DbSet<ImageBreed> ImageBreeds { get; set; }
        public DbSet<Breed> Breeds { get; set; }
        public DbSet<BreedCategory> BreedCategories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}


