using Domain.Dto.Response;
using Domain.Dto.Response.Breed;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.BreedService
{
    public class GetAllBreedTest
    {
        private readonly Mock<IRepository<Breed>> _breedRepositoryMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepositoryMock;
        private readonly Mock<IRepository<BreedCategory>> _breedCategoryRepositoryMock;
        private readonly Mock<CloudinaryCloudService> _cloudinaryCloudServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BreedService _breedService;

        public GetAllBreedTest()
        {
            _breedRepositoryMock = new Mock<IRepository<Breed>>();
            _imageBreedRepositoryMock = new Mock<IRepository<ImageBreed>>();
            _breedCategoryRepositoryMock = new Mock<IRepository<BreedCategory>>();
            _cloudinaryCloudServiceMock = new Mock<CloudinaryCloudService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock HttpContext để lấy userId
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("uid", Guid.NewGuid().ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(user);

            // Mock IOptions<CloudinaryConfig>
            var cloudinaryConfigMock = new Mock<IOptions<Domain.Settings.CloudinaryConfig>>();
            cloudinaryConfigMock.Setup(x => x.Value).Returns(new Domain.Settings.CloudinaryConfig
            {
                CloudName = "dpgk5pqt9",
                ApiKey = "382542864398655",
                ApiSecret = "ct6gqlmsftVgmj2C3A8tYoiQk0M"
            });

            // Khởi tạo CloudinaryCloudService với mock config
            var cloudinaryService = new CloudinaryCloudService(cloudinaryConfigMock.Object);

            _breedService = new Infrastructure.Services.Implements.BreedService(
                _breedRepositoryMock.Object,
                _imageBreedRepositoryMock.Object,
                cloudinaryService,
                _breedCategoryRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetAllBreed_ReturnsListOfBreeds_WhenBreedsExist()
        {
            // Arrange
            var breeds = new List<Breed>
            {
                new Breed
                {
                    Id = Guid.NewGuid(),
                    BreedName = "Breed1",
                    BreedCategoryId = Guid.NewGuid(),
                    Stock = 10,
                    IsActive = true,
                    BreedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Category1", Description = "Desc1" }
                },
                new Breed
                {
                    Id = Guid.NewGuid(),
                    BreedName = "Breed2",
                    BreedCategoryId = Guid.NewGuid(),
                    Stock = 20,
                    IsActive = true,
                    BreedCategory = new BreedCategory { Id = Guid.NewGuid(), Name = "Category2", Description = "Desc2" }
                }
            };
            var images = new List<ImageBreed>
            {
                new ImageBreed { BreedId = breeds[0].Id, ImageLink = "image1.jpg", Thumnail = "true" },
                new ImageBreed { BreedId = breeds[0].Id, ImageLink = "image2.jpg", Thumnail = "false" },
                new ImageBreed { BreedId = breeds[1].Id, ImageLink = "image3.jpg", Thumnail = "true" }
            };

            var breedMock = breeds.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Breed, bool>>>()))
                .Returns((Expression<Func<Breed, bool>> predicate) => breedMock.Where(predicate));
            _breedRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(() => breedMock); // Loại bỏ mock Include, trả về IQueryable đã chứa dữ liệu
            var imageMock = images.AsQueryable().BuildMock();
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
                .Returns(() => imageMock);
            //_breedCategoryRepositoryMock.Setup(x => x.GetQueryable()).Returns(categories.AsQueryable());

            // Act
            var result = await _breedService.GetAllBreed(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Breed1", result[0].BreedName);
            Assert.Equal("Category1", result[0].BreedCategory.Name);
            Assert.Equal("image1.jpg", result[0].Thumbnail);
            Assert.Contains("image2.jpg", result[0].ImageLinks);
            Assert.Equal("Breed2", result[1].BreedName);
            Assert.Equal("Category2", result[1].BreedCategory.Name);
            Assert.Equal("image3.jpg", result[1].Thumbnail);
        }

        [Fact]
        public async Task GetAllBreed_ReturnsEmptyList_WhenNoBreedsExist()
        {
            // Arrange
            var breeds = new List<Breed>();
            var breedMock = breeds.AsQueryable().BuildMock();
            _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Breed, bool>>>()))
                .Returns((Expression<Func<Breed, bool>> predicate) => breedMock.Where(predicate));
            var imageMock = Enumerable.Empty<ImageBreed>().AsQueryable().BuildMock();
            _imageBreedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ImageBreed, bool>>>()))
                .Returns(() => imageMock);

            // Act
            var result = await _breedService.GetAllBreed(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //[Fact]
        //public async Task GetAllBreed_ThrowsException_WhenRepositoryFails()
        //{
        //    // Arrange
        //    _breedRepositoryMock.Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Breed, bool>>>()))
        //        .Throws(new Exception("Database error"));

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<Exception>(() => _breedService.GetAllBreed(CancellationToken.None));
        //    Assert.Equal("Database error", exception.Message);
        //}
    }
}