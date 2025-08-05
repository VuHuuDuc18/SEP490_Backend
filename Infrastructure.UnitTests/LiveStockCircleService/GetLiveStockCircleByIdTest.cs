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
            var livestockCircle = new LivestockCircle
            {
                Id = livestockCircleId,
                LivestockCircleName = "Circle 1",
                Status = "Active",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                TotalUnit = 100,
                DeadUnit = 5,
                AverageWeight = 50.5f,
                GoodUnitNumber = 90,
                BadUnitNumber = 5,
                BreedId = Guid.NewGuid(),
                BarnId = Guid.NewGuid(),
                TechicalStaffId = Guid.NewGuid(),
                IsActive = true
            };
            _livestockCircleRepositoryMock.Setup(x => x.GetByIdAsync(livestockCircleId, It.IsAny<Ref<CheckError>>()))
                .ReturnsAsync(livestockCircle);

            // Act
            var result = await _service.GetLiveStockCircleById(livestockCircleId);

            // Assert
            Assert.True(result.Circle != null);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(livestockCircleId, result.Circle.Id);
            Assert.Equal("Circle 1", result.Circle.LivestockCircleName);
            Assert.Equal("Active", result.Circle.Status);
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
}
