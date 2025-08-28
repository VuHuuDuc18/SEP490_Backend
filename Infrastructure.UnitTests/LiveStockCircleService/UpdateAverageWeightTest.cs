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
using Infrastructure.Core;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class UpdateAverageWeightTest
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

        public UpdateAverageWeightTest()
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
        public async Task UpdateAverageWeight_Success_UpdatesWeightSuccessfully()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var averageWeight = 50.5f;
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                IsActive = true,
                AverageWeight = 40.0f
            };

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);
            _livestockCircleRepositoryMock
                .Setup(x => x.Update(It.IsAny<LivestockCircle>()));
            _livestockCircleRepositoryMock
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

            // Assert
            Assert.True(success);
            Assert.Null(errorMessage);
            Assert.Equal(averageWeight, livestockCircle.AverageWeight);
            _livestockCircleRepositoryMock.Verify(x => x.Update(livestockCircle), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAverageWeight_NegativeWeight_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var averageWeight = -10.0f;

            // Act
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

            // Assert
            Assert.False(success);
            Assert.Equal("Trọng lượng trung bình không thể âm.", errorMessage);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Ref<CheckError>>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAverageWeight_NonExistentCircle_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var averageWeight = 50.5f;

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

            // Assert
            Assert.False(success);
            Assert.Equal("Không tìm thấy chu kỳ chăn nuôi.", errorMessage);
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAverageWeight_InactiveCircle_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var averageWeight = 50.5f;
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                IsActive = false,
                AverageWeight = 40.0f
            };

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);

            // Act
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

            // Assert
            Assert.False(success);
            Assert.Equal("Chu kỳ chăn nuôi không còn hoạt động.", errorMessage);
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        //[Fact]
        //public async Task UpdateAverageWeight_RepositoryErrorOnGet_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var averageWeight = 50.5f;
        //    var checkError = new Ref<CheckError> { Value = new CheckError { isError = true, Message = "Database error" } };

        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .Callback((Guid id, Ref<CheckError> error) => error.Value = checkError.Value)
        //        .ReturnsAsync((LivestockCircle)null);

        //    // Act
        //    var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

        //    // Assert
        //    Assert.False(success);
        //    Assert.Equal("Lỗi khi lấy thông tin chu kỳ chăn nuôi: Database error", errorMessage);
        //    _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
        //    _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        //}

        [Fact]
        public async Task UpdateAverageWeight_RepositoryExceptionOnCommit_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var averageWeight = 50.5f;
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                IsActive = true,
                AverageWeight = 40.0f
            };

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);
            _livestockCircleRepositoryMock
                .Setup(x => x.Update(It.IsAny<LivestockCircle>()));
            _livestockCircleRepositoryMock
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Commit failed"));

            // Act
            var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

            // Assert
            Assert.False(success);
            Assert.Equal("Lỗi khi cập nhật trọng lượng trung bình: Commit failed", errorMessage);
            _livestockCircleRepositoryMock.Verify(x => x.Update(livestockCircle), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        //[Fact]
        //public async Task UpdateAverageWeight_ZeroWeight_UpdatesSuccessfully()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var averageWeight = 0.0f;
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        IsActive = true,
        //        AverageWeight = 40.0f
        //    };

        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .ReturnsAsync(livestockCircle);
        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.Update(It.IsAny<LivestockCircle>()));
        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(1);

        //    // Act
        //    var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

        //    // Assert
        //    Assert.True(success);
        //    Assert.Null(errorMessage);
        //    Assert.Equal(averageWeight, livestockCircle.AverageWeight);
        //    _livestockCircleRepositoryMock.Verify(x => x.Update(livestockCircle), Times.Once());
        //    _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        //}

        //[Fact]
        //public async Task UpdateAverageWeight_EmptyGuid_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.Empty;
        //    var averageWeight = 50.5f;

        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .ReturnsAsync((LivestockCircle)null);

        //    // Act
        //    var (success, errorMessage) = await _livestockCircleService.UpdateAverageWeight(livestockCircleId, averageWeight, default);

        //    // Assert
        //    Assert.False(success);
        //    Assert.Equal("Không tìm thấy chu kỳ chăn nuôi.", errorMessage);
        //    _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
        //    _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        //}
    }
}
