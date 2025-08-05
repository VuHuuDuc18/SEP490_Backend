using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;
using Infrastructure.Services;
using MockQueryable;
using Domain.Settings;
using Microsoft.Extensions.Options;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class GetActiveLiveStockCircleByBarnIdTest
    {
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _livestockCircleImageRepoMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly LivestockCircleService _livestockCircleService;

        public GetActiveLiveStockCircleByBarnIdTest()
        {
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _livestockCircleImageRepoMock = new Mock<IRepository<ImageLivestockCircle>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _foodImageRepositoryMock = new Mock<IRepository<ImageFood>>();
            _medicineImageRepositoryMock = new Mock<IRepository<ImageMedicine>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());

            _livestockCircleService = new LivestockCircleService(
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
                _imageLiveStockCircleRepositoryMock.Object,
                _cloudinaryCloudServiceMock.Object
            );
        }

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_Success_ReturnsCircleWithRelatedData()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = circleId,
                BarnId = barnId,
                IsActive = true,
                LivestockCircleName = "Test Circle",
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-10),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = breedId,
                TechicalStaffId = technicalStaffId
            };
            var technicalStaff = new User
            {
                Id = technicalStaffId,
                Email = "staff@example.com",
                FullName = "John Doe",
                PhoneNumber = "1234567890"
            };
            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed"
            };
            var breedImages = new List<ImageBreed>
            {
                new ImageBreed { BreedId = breedId, ImageLink = "https://cloudinary.com/thumbnail.jpg", Thumnail = "true" }
            }.AsQueryable().BuildMock();

            var queryableCircles = new List<LivestockCircle> { livestockCircle }
                .AsQueryable()
                .BuildMock();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Returns(queryableCircles);
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(technicalStaffId, null))
                .ReturnsAsync(technicalStaff);
            _breedRepositoryMock
                .Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);
            _imageBreedRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImages);

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(errorMessage);
            Assert.NotNull(circle);
            Assert.Equal(circleId, circle.Id);
            Assert.Equal("Test Circle", circle.LivestockCircleName);
            Assert.Equal("Active", circle.Status);
            Assert.Equal(livestockCircle.StartDate, circle.StartDate);
            Assert.Equal(100, circle.TotalUnit);
            Assert.Equal(5, circle.DeadUnit);
            Assert.Equal(50.5f, circle.AverageWeight);
            Assert.Equal(90, circle.GoodUnitNumber);
            Assert.Equal(5, circle.BadUnitNumber);
            Assert.NotNull(circle.TechicalStaffId);
            Assert.Equal(technicalStaffId, circle.TechicalStaffId.Id);
            Assert.Equal("staff@example.com", circle.TechicalStaffId.Email);
            Assert.Equal("John Doe", circle.TechicalStaffId.Fullname);
            Assert.Equal("1234567890", circle.TechicalStaffId.PhoneNumber);
            Assert.NotNull(circle.Breed);
            Assert.Equal(breedId, circle.Breed.Id);
            Assert.Equal("Test Breed", circle.Breed.BreedName);
            Assert.Equal("https://cloudinary.com/thumbnail.jpg", circle.Breed.Thumbnail);
        }

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_NoActiveCircle_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var queryableCircles = new List<LivestockCircle>()
                .AsQueryable()
                .BuildMock();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Returns(queryableCircles);

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(circle);
            Assert.Equal("Không tìm thấy chu kỳ chăn nuôi đang hoạt động cho chuồng này.", errorMessage);
        }

        //[Fact]
        //public async Task GetActiveLiveStockCircleByBarnId_EmptyBarnId_ReturnsError()
        //{
        //    // Arrange
        //    var barnId = Guid.Empty;

        //    // Act
        //    var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

        //    // Assert
        //    Assert.Null(circle);
        //    Assert.Equal("Không tìm thấy chu kỳ chăn nuôi đang hoạt động cho chuồng này.", errorMessage);
        //}

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_NonExistentTechnicalStaff_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = circleId,
                BarnId = barnId,
                IsActive = true,
                LivestockCircleName = "Test Circle",
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-10),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = breedId,
                TechicalStaffId = technicalStaffId
            };
            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed"
            };
            var breedImages = new List<ImageBreed>
            {
                new ImageBreed { BreedId = breedId, ImageLink = "https://cloudinary.com/thumbnail.jpg", Thumnail = "true" }
            }.AsQueryable().BuildMock();

            var queryableCircles = new List<LivestockCircle> { livestockCircle }
                .AsQueryable()
                .BuildMock();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Returns(queryableCircles);
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(technicalStaffId, null))
                .ReturnsAsync((User)null);
            _breedRepositoryMock
                .Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);
            _imageBreedRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImages);

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(circle);
            Assert.Contains("Lỗi khi lấy chu kỳ chăn nuôi đang hoạt động theo BarnId", errorMessage);
        }

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_NonExistentBreed_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = circleId,
                BarnId = barnId,
                IsActive = true,
                LivestockCircleName = "Test Circle",
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-10),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = breedId,
                TechicalStaffId = technicalStaffId
            };
            var technicalStaff = new User
            {
                Id = technicalStaffId,
                Email = "staff@example.com",
                FullName = "John Doe",
                PhoneNumber = "1234567890"
            };

            var queryableCircles = new List<LivestockCircle> { livestockCircle }
                .AsQueryable()
                .BuildMock();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Returns(queryableCircles);
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(technicalStaffId, null))
                .ReturnsAsync(technicalStaff);
            _breedRepositoryMock
                .Setup(x => x.GetByIdAsync(breedId, null  ) )
                .ReturnsAsync((Breed)null);

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(circle);
            Assert.Contains("Lỗi khi lấy chu kỳ chăn nuôi đang hoạt động theo BarnId", errorMessage);
        }

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_NoBreedImages_ReturnsCircleWithoutThumbnail()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            var circleId = Guid.NewGuid();
            var technicalStaffId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = circleId,
                BarnId = barnId,
                IsActive = true,
                LivestockCircleName = "Test Circle",
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-10),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = breedId,
                TechicalStaffId = technicalStaffId
            };
            var technicalStaff = new User
            {
                Id = technicalStaffId,
                Email = "staff@example.com",
                FullName = "John Doe",
                PhoneNumber = "1234567890"
            };
            var breed = new Breed
            {
                Id = breedId,
                BreedName = "Test Breed"
            };
            var breedImages = new List<ImageBreed>()
                .AsQueryable()
                .BuildMock();

            var queryableCircles = new List<LivestockCircle> { livestockCircle }
                .AsQueryable()
                .BuildMock();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Returns(queryableCircles);
            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(technicalStaffId, null))
                .ReturnsAsync(technicalStaff);
            _breedRepositoryMock
                .Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);
            _imageBreedRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>()))
                .Returns(breedImages);

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(errorMessage);
            Assert.NotNull(circle);
            Assert.Equal(circleId, circle.Id);
            Assert.Equal("Test Circle", circle.LivestockCircleName);
            Assert.NotNull(circle.Breed);
            Assert.Equal(breedId, circle.Breed.Id);
            Assert.Equal("Test Breed", circle.Breed.BreedName);
            Assert.Null(circle.Breed.Thumbnail);
        }

        [Fact]
        public async Task GetActiveLiveStockCircleByBarnId_RepositoryThrowsException_ReturnsError()
        {
            // Arrange
            var barnId = Guid.NewGuid();
            _livestockCircleRepositoryMock
                .Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<LivestockCircle, bool>>>()))
                .Throws(new Exception("Database error"));

            // Act
            var (circle, errorMessage) = await _livestockCircleService.GetActiveLiveStockCircleByBarnId(barnId, default);

            // Assert
            Assert.Null(circle);
            Assert.Equal("Lỗi khi lấy chu kỳ chăn nuôi đang hoạt động theo BarnId: Database error", errorMessage);
        }
    }
}