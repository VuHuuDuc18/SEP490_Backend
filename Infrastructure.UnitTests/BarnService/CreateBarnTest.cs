using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request.Barn;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using MockQueryable.Moq;
using MockQueryable;
using Xunit;

namespace Infrastructure.UnitTests.BarnService
{
    public class CreateBarnTest
    {
        private readonly Mock<IRepository<Barn>> _barnRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageLivestockCircle>> _imageLiveStockCircleRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BarnService _barnService;
        private readonly Guid _userId = Guid.Parse("3c9ef2d9-4b1a-4e4e-8f5e-9b2c8d1e7f3a");

        public CreateBarnTest()
        {
            _barnRepositoryMock = new Mock<IRepository<Barn>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _livestockCircleRepositoryMock = new Mock<IRepository<LivestockCircle>>();
            _imageLiveStockCircleRepositoryMock = new Mock<IRepository<ImageLivestockCircle>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>(Mock.Of<IOptions<CloudinaryConfig>>());
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Setup HttpContext with user claims
            var claims = new List<Claim> { new Claim("uid", _userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _barnService = new Infrastructure.Services.Implements.BarnService(
                _barnRepositoryMock.Object,
                _userRepositoryMock.Object,
                _livestockCircleRepositoryMock.Object,
                _imageLiveStockCircleRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                _breedRepositoryMock.Object,
                _httpContextAccessorMock.Object,
                _cloudinaryCloudServiceMock.Object);
        }

        //[Fact]
        //public async Task CreateBarn_RequestNull_ReturnsError()
        //{
        //    // Act
        //    var result = await _barnService.CreateBarn(null, default);

        //    // Assert
        //    Xunit.Assert.False(result.Succeeded);
        //    Xunit.Assert.Equal("Dữ liệu chuồng trại không được null", result.Message);
        //    Xunit.Assert.Null(result.Data);
        //    Xunit.Assert.Contains("Dữ liệu chuồng trại không được null", result.Errors);
        //}

        [Fact]
        public async Task CreateBarn_BarnNameBlank_ReturnsError()
        {
            // Arrange
            var request = new CreateBarnRequest
            {
                BarnName = "", // thiếu tên
                Address = "Nghe An",
                Image = "data:image/png;base64,xxx", 
                WorkerId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557")
            };

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("Tên chuồng trại là bắt buộc.", result.Errors);
           
        }

        [Fact]
        public async Task CreateBarn_AddressBlank_ReturnsError()
        {
            // Arrange
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A", 
                Address = "",
                Image = "data:image/png;base64,xxx",
                WorkerId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557")
            };

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("Địa chỉ là bắt buộc.", result.Errors);
        }

        [Fact]
        public async Task CreateBarn_ImageBlank_ReturnsError()
        {
            // Arrange
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A",
                Address = "Nghe An",
                Image = "",
                WorkerId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557")
            };

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Equal("Dữ liệu không hợp lệ", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("Hình ảnh chuồng là bắt buộc.", result.Errors);
        }

        //[Fact]
        //public async Task CreateBarn_WorkerIdBlank_ReturnsError()
        //{
        //    // Arrange
        //    var request = new CreateBarnRequest
        //    {
        //        BarnName = "Chuồng 1 của anh A",
        //        Address = "Nghe An",
        //        Image = "data:image/png;base64,xxx",
        //        WorkerId = Guid.Empty
        //    };

        //    // Act
        //    var result = await _barnService.CreateBarn(request, default);

        //    // Assert
        //    Xunit.Assert.False(result.Succeeded);
        //    Xunit.Assert.Equal("Lỗi khi tạo chuồng trại", result.Message);
        //}


        [Fact]
        public async Task CreateBarn_WorkerNotFoundOrInactive_ReturnsError()
        {
            // Arrange
            var workerId = Guid.NewGuid();
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A",
                Address = "Nghe An",
                Image = "data:image/png;base64,xxx",
                WorkerId = workerId
            };
            // Worker not found
            _userRepositoryMock.Setup(x => x.GetByIdAsync(workerId, default)).ReturnsAsync((User)null);
            var barnsMock = new List<Barn>().AsQueryable().BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>())).Returns(barnsMock);

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Equal("Người gia công không tồn tại hoặc đã bị xóa", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("Người gia công không tồn tại hoặc đã bị xóa", result.Errors);
        }

        [Fact]
        public async Task CreateBarn_WorkerInactive_ReturnsError()
        {
            // Arrange
            var workerId = Guid.NewGuid();
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A",
                Address = "Nghe An",
                Image = "data:image/png;base64,xxx",
                WorkerId = workerId
            };
            // Worker inactive
            var worker = new User { Id = workerId, IsActive = false };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(workerId, default)).ReturnsAsync(worker);
            var barnsMock = new List<Barn>().AsQueryable().BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>())).Returns(barnsMock);

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Equal("Người gia công không tồn tại hoặc đã bị xóa", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("Người gia công không tồn tại hoặc đã bị xóa", result.Errors);
        }

        [Fact]
        public async Task CreateBarn_BarnAlreadyExists_ReturnsError()
        {
            // Arrange
            var workerId = Guid.NewGuid();
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A",
                Address = "Nghe An",
                Image = "data:image/png;base64,xxx",
                WorkerId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557")
            };
            var existingBarn = new Barn { BarnName = "Chuồng 1 của anh A", WorkerId = workerId, IsActive = true };
            var barns = new List<Barn> { existingBarn }.AsQueryable();
            var barnsMock = barns.BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>()))
                .Returns(barns.BuildMock());
            var worker = new User { Id = workerId, IsActive = true };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(workerId, default)).ReturnsAsync(worker);

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.False(result.Succeeded);
            Xunit.Assert.Contains("đã tồn tại", result.Message);
            Xunit.Assert.Null(result.Data);
            Xunit.Assert.Contains("đã tồn tại", result.Errors[0]);
        }

        [Fact]
        public async Task CreateBarn_Success_WithImage()
        {
            // Arrange
            var workerId = Guid.Parse("8808C9FB-825E-4BD2-FEA2-08DDAE35A557");
            var request = new CreateBarnRequest
            {
                BarnName = "Chuồng 1 của anh A",
                Address = "Nghe An",
                Image = "data:image/jpeg;base64,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" +
                "tQFL4w7WszXAXVFLoEuOcTL2RL6V04B8NOa3wSqR2L5rTt7CtNulPbt/F2v2dvcRXQuzkS7hk0wKjyzb4PXt2JcYLbFdgeweXozW6tOMotZ3SXdu/czRh6cozUrKzb73r+5Gp1ROiFAFAFAR/H5FW3ldvNRC5wMnC9o7eysoxcmkjGUlFNszMcc4aZOtKAvz1dUc58eXP086uLD4nZ2d3eUHicLtbW/uLLwLpFbzv1cbMW0lt1I2G" +
                "M7n11oqYepTW1JFiliqVWWzF5lvtuVaSwL0AUBXLzjl0lwLdbVGLh2Q9fjUiHGSOr7J3G1Wo0Kbp7bm8rXy48ypOvVVTYUFne2fDkI2/St3kdepiCRzSIxNwBJpjJ1MsWjJ2BOPrpUw8YQ2m3pfTL73FHETq1diMV8Vtc9eBUeCTJIp4lexJO096tuBKQY4YypYsAwIwOWD8nmMnPPwOG6y5SlrZvjpuPU+3sfL2aqeGw72Y5Xadm275t8r8+xFouulSW4nWFBLHAkbqRKCD1sgTQCFOAudtz4bDGOjSwTeym7Xb3aWVzyuI9oq85WTslmnrd2+4v+VpWO4eSEAwBMaJBKkjSeagcKMHOMjBwN6nql5RUXrfVWat2GPXLRm5R+G2junfcE/S4izS7WEs7TdUYdWCr5Yac6Tk7Du7" +
                "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", // phải có dấu phẩy
                WorkerId = workerId
            };
            var worker = new User { Id = workerId, IsActive = true, FullName = "Worker 1" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(workerId, default)).ReturnsAsync(worker);
            var barnsMock = new List<Barn>().AsQueryable().BuildMock();
            _barnRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Barn, bool>>>())).Returns(barnsMock);
            _cloudinaryCloudServiceMock.Setup(x => x.UploadImage(It.IsAny<string>(), "barn", It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://cloudinary.com/barn-image.jpg");

            // Act
            var result = await _barnService.CreateBarn(request, default);

            // Assert
            Xunit.Assert.True(result.Succeeded);
            Xunit.Assert.Equal("Tạo chuồng trại thành công", result.Message);
            Xunit.Assert.Contains("Chuồng trại đã được tạo thành công. ID:", result.Data);
            Xunit.Assert.Null(result.Errors);
            _barnRepositoryMock.Verify(x => x.Insert(It.IsAny<Barn>()), Times.Once());
            _barnRepositoryMock.Verify(x => x.CommitAsync(default), Times.Once());
        }
    }
}