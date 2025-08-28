using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class ReleaseBarnTest
    {
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _livestockCircleImageRepoMock;
        private readonly Mock<IRepository<LivestockCircleFood>> _livestockCircleFoodRepositoryMock;
        private readonly Mock<IRepository<LivestockCircleMedicine>> _livestockCircleMedicineRepositoryMock;
        private readonly Mock<IRepository<Food>> _foodRepositoryMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageFood>> _foodImageRepositoryMock;
        private readonly Mock<IRepository<ImageMedicine>> _medicineImageRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly LivestockCircleService _livestockCircleService;

        public ReleaseBarnTest()
        {
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _livestockCircleImageRepoMock = new Mock<IRepository<ImageLivestockCircle>>();
            _livestockCircleFoodRepositoryMock = new Mock<IRepository<LivestockCircleFood>>();
            _livestockCircleMedicineRepositoryMock = new Mock<IRepository<LivestockCircleMedicine>>();
            _foodRepositoryMock = new Mock<IRepository<Food>>();
            _medicineRepositoryMock = new Mock<IRepository<Medicine>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
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

        //[Fact]
        //public async Task ReleaseBarn_Success_UpdatesStatusAndCommits()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
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
        //    var result = await _livestockCircleService.ReleaseBarn(new Domain.DTOs.Request.LivestockCircle.ReleaseBarnRequest()
        //    {
        //        LivestockCircleId = livestockCircleId,
        //        ReleaseDate = DateTime.Now.Date
        //    });

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Equal("Xuất chuồng thành công vào ngày :"+ DateTime.Now.Date, result.Message);
        //    Assert.Null(result.Errors);
        //    _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
        //    //_livestockCircleRepositoryMock.Verify(x => x.Update(It.Is<LivestockCircle>(lc => lc.Id == livestockCircleId && lc.Status == StatusConstant.RELEASESTAT)), Times.Once());
        //    //_livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        //}

        [Fact]
        public async Task ReleaseBarn_NonExistentLivestockCircle_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _livestockCircleService.ReleaseBarn(new Domain.DTOs.Request.LivestockCircle.ReleaseBarnRequest()
            {
                LivestockCircleId = livestockCircleId,
                ReleaseDate = DateTime.Now.Date
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy chuồng", result.Message);
            Assert.Null(result.Errors);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReleaseBarn_InvalidStatus_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                Status = StatusConstant.PENDINGSTAT, // Not GROWINGSTAT
                IsActive = true
            };

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);

            // Act
            var result = await _livestockCircleService.ReleaseBarn(new Domain.DTOs.Request.LivestockCircle.ReleaseBarnRequest()
            {
                LivestockCircleId = livestockCircleId,
                ReleaseDate = DateTime.Now.Date
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi hệ thống. Không thể xuất chuồng trại vào lúc này. Vui lòng thử lại sau", result.Message);
            Assert.Null(result.Errors);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task ReleaseBarn_EmptyGuid_ReturnsError()
        {
            // Arrange
            var livestockCircleId = Guid.Empty;

            _livestockCircleRepositoryMock
                .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _livestockCircleService.ReleaseBarn(new Domain.DTOs.Request.LivestockCircle.ReleaseBarnRequest() { LivestockCircleId = livestockCircleId, ReleaseDate = DateTime.Now.Date });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy chuồng", result.Message);
            Assert.Null(result.Errors);
            _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
            _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
            _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        //[Fact]
        //public async Task ReleaseBarn_CommitFailure_ThrowsException()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var livestockCircle = new LivestockCircle
        //    {
        //        Id = livestockCircleId,
        //        Status = StatusConstant.GROWINGSTAT,
        //        IsActive = true
        //    };

        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .ReturnsAsync(livestockCircle);
        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.Update(It.IsAny<LivestockCircle>()));
        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new Exception("Commit failed"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(() => _livestockCircleService.ReleaseBarn(new Domain.DTOs.Request.LivestockCircle.ReleaseBarnRequest() { LivestockCircleId = livestockCircleId, ReleaseDate = DateTime.Now.Date }));
        //    _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
        //    //_livestockCircleRepositoryMock.Verify(x => x.Update(It.Is<LivestockCircle>(lc => lc.Id == livestockCircleId && lc.Status == StatusConstant.RELEASESTAT)), Times.Once());
        //    //_livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once());
        //}

        //[Fact]
        //public async Task ReleaseBarn_GetByIdError_ReturnsError()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var checkError = new Ref<CheckError> { Value = new CheckError { isError = true, Message = "Database error" } };

        //    _livestockCircleRepositoryMock
        //        .Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .Callback((Guid id, Ref<CheckError> error) => error.Value = checkError.Value)
        //        .ReturnsAsync((LivestockCircle)null);

        //    // Act
        //    var result = await _livestockCircleService.ReleaseBarn(livestockCircleId);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy chuồng", result.Message);
        //    Assert.Null(result.Errors);
        //    _livestockCircleRepositoryMock.Verify(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()), Times.Once());
        //    _livestockCircleRepositoryMock.Verify(x => x.Update(It.IsAny<LivestockCircle>()), Times.Never());
        //    _livestockCircleRepositoryMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never());
        //}
    }
}