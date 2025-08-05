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
using Domain.Dto.Request.LivestockCircle;
using Domain.Helper.Constants;
using Domain.IServices;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class CreateLiveStockCircleTest
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

        public CreateLiveStockCircleTest()
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
        public async Task CreateLiveStockCircle_Success_CreatesLivestockCircle()
        {
            // Arrange
            var breedId = Guid.NewGuid();
            var barnId = Guid.NewGuid();
            var techicalStaffId = Guid.NewGuid();
            var request = new CreateLivestockCircleRequest
            {
                BarnId = barnId,
                BreedId = breedId,
                TechicalStaffId = techicalStaffId,
                TotalUnit = 50,
                LivestockCircleName = "Test Cycle"
            };

            var breed = new Breed
            {
                Id = breedId,
                Stock = 100,
                BreedName = "Test Breed",
                IsActive = true
            };

            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);

            var insertedLivestockCircle = new LivestockCircle();
            _livestockCircleRepositoryMock.Setup(x => x.Insert(It.IsAny<LivestockCircle>()))
                .Callback<LivestockCircle>(lc => insertedLivestockCircle = lc);
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1); // Simulate successful commit

            // Act
            var result = await _service.CreateLiveStockCircle(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Message);
            //Assert.NotEqual(Guid.Empty, result.Data);
            //Assert.Equal(request.BarnId, insertedLivestockCircle.BarnId);
            //Assert.Equal(request.BreedId, insertedLivestockCircle.BreedId);
            //Assert.Equal(request.TechicalStaffId, insertedLivestockCircle.TechicalStaffId);
            //Assert.Equal(request.TotalUnit, insertedLivestockCircle.TotalUnit);
            //Assert.Equal(request.LivestockCircleName, insertedLivestockCircle.LivestockCircleName);
            //Assert.Equal(StatusConstant.PENDINGSTAT, insertedLivestockCircle.Status);
            //Assert.Equal(0, insertedLivestockCircle.AverageWeight);
            //Assert.Equal(0, insertedLivestockCircle.DeadUnit);
            //Assert.Equal(0, insertedLivestockCircle.GoodUnitNumber);
            //Assert.Equal(0, insertedLivestockCircle.BadUnitNumber);
            //_livestockCircleRepositoryMock.Verify(x => x.Insert(It.IsAny<LivestockCircle>()), Times.Once());
            //_livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateLiveStockCircle_BreedNotFound_ReturnsError()
        {
            // Arrange
            var breedId = Guid.NewGuid();
            var request = new CreateLivestockCircleRequest
            {
                BarnId = Guid.NewGuid(),
                BreedId = breedId,
                TechicalStaffId = Guid.NewGuid(),
                TotalUnit = 50,
                LivestockCircleName = "Test Cycle"
            };

            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync((Breed)null);

            // Act
            var result = await _service.CreateLiveStockCircle(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Giống nuôi không khả dụng", result.Message);
            Assert.Equal(Guid.Empty, result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.Insert(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateLiveStockCircle_InsufficientStock_ReturnsError()
        {
            // Arrange
            var breedId = Guid.NewGuid();
            var request = new CreateLivestockCircleRequest
            {
                BarnId = Guid.NewGuid(),
                BreedId = breedId,
                TechicalStaffId = Guid.NewGuid(),
                TotalUnit = 100,
                LivestockCircleName = "Test Cycle"
            };

            var breed = new Breed
            {
                Id = breedId,
                Stock = 50, // Less than TotalUnit
                BreedName = "Test Breed",
                IsActive = true
            };

            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);

            // Act
            var result = await _service.CreateLiveStockCircle(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không đủ số lượng giống", result.Message);
            Assert.Equal(Guid.Empty, result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.Insert(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task CreateLiveStockCircle_CommitFailure_ReturnsError()
        {
            // Arrange
            var breedId = Guid.NewGuid();
            var request = new CreateLivestockCircleRequest
            {
                BarnId = Guid.NewGuid(),
                BreedId = breedId,
                TechicalStaffId = Guid.NewGuid(),
                TotalUnit = 50,
                LivestockCircleName = "Test Cycle"
            };

            var breed = new Breed
            {
                Id = breedId,
                Stock = 100,
                BreedName = "Test Breed",
                IsActive = true
            };

            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, null))
                .ReturnsAsync(breed);
            _livestockCircleRepositoryMock.Setup(x => x.Insert(It.IsAny<LivestockCircle>()))
                .Callback<LivestockCircle>(lc => { });
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0); // Simulate commit failure

            // Act
            var result = await _service.CreateLiveStockCircle(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không thể tạo lứa mới", result.Message);
            Assert.Equal(Guid.Empty, result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.Insert(It.IsAny<LivestockCircle>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateLiveStockCircle_ExceptionThrown_ReturnsError()
        {
            // Arrange
            var breedId = Guid.NewGuid();
            var request = new CreateLivestockCircleRequest
            {
                BarnId = Guid.NewGuid(),
                BreedId = breedId,
                TechicalStaffId = Guid.NewGuid(),
                TotalUnit = 50,
                LivestockCircleName = "Test Cycle"
            };

            _breedRepositoryMock.Setup(x => x.GetByIdAsync(breedId, null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateLiveStockCircle(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu nhận được lỗi", result.Message);
            Assert.Equal(Guid.Empty, result.Data);
            _livestockCircleRepositoryMock.Verify(x => x.Insert(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
