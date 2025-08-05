using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Helper;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using Xunit;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services;
using Domain.Settings;
using Microsoft.Extensions.Options;
using Infrastructure.Core;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.LiveStockCircleService
{
    public class DisableLiveStockCircleTest
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

        public DisableLiveStockCircleTest()
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
        public async Task DisableLiveStockCircle_Successful()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            var livestockCircle = new LivestockCircle { Id = livestockCircleId, IsActive = true };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.DisableLiveStockCircle(livestockCircleId);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.False(livestockCircle.IsActive); // Kiểm tra trạng thái đã thay đổi
        }

        [Fact]
        public async Task DisableLiveStockCircle_NotFound()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync((LivestockCircle)null);

            // Act
            var result = await _service.DisableLiveStockCircle(livestockCircleId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Không tìm thấy chu kỳ chăn nuôi.", result.ErrorMessage);
        }

        [Fact]
        public async Task DisableLiveStockCircle_ExceptionOccurs()
        {
            // Arrange
            var livestockCircleId = Guid.NewGuid();
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(new LivestockCircle { Id = livestockCircleId, IsActive = true });
            _livestockCircleRepositoryMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DisableLiveStockCircle(livestockCircleId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Lỗi khi xóa chu kỳ chăn nuôi: Database error", result.ErrorMessage);
        }

        //[Fact]
        //public async Task DisableLiveStockCircle_CheckErrorFromRepository()
        //{
        //    // Arrange
        //    var livestockCircleId = Guid.NewGuid();
        //    var checkError = new Ref<CheckError> { Value = new CheckError { isError = true, Message = "Repository error" } };
        //    _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
        //        .Callback((Guid id, Ref<CheckError> error) => error.Value = checkError.Value)
        //        .ReturnsAsync((LivestockCircle)null);

        //    // Act
        //    var result = await _service.DisableLiveStockCircle(livestockCircleId);

        //    // Assert
        //    Assert.False(result.Success);
        //    Assert.Equal("Lỗi khi lấy thông tin chu kỳ chăn nuôi: Repository error", result.ErrorMessage);
        //}
    }

}