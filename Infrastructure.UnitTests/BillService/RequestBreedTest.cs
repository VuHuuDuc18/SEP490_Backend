using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Bill;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.UnitTests.BillService
{
    public class RequestBreedTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Breed>> _breedRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public RequestBreedTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _breedRepoMock = new Mock<IRepository<Breed>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                new Mock<IRepository<LivestockCircle>>().Object,
                new Mock<IRepository<Food>>().Object,
                new Mock<IRepository<Medicine>>().Object,
                _breedRepoMock.Object,
                new Mock<IRepository<Barn>>().Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task RequestBreed_ReturnsError_WhenRequestIsNull()
        {
            var (success, error) = await _service.RequestBreed(null);
            Assert.False(success);
            Assert.Contains("Dữ liệu yêu cầu là bắt buộc", error);
        }

        [Fact]
        public async Task RequestBreed_ReturnsError_WhenNoBreedItems()
        {
            var request = new CreateBreedRequestDto { BreedItems = new List<BreedItemRequest>() };
            var (success, error) = await _service.RequestBreed(request);
            Assert.False(success);
            Assert.Contains("Phải cung cấp ít nhất một mặt hàng giống", error);
        }

        //[Fact]
        //public async Task RequestBreed_ReturnsError_WhenValidationFails()
        //{
        //    var breedId = Guid.NewGuid();
        //    var request = new CreateBreedRequestDto
        //    {
        //        BreedItems = new List<BreedItemRequest> { new BreedItemRequest { ItemId = breedId, Quantity = 0 } }
        //    };
        //    var breed = new Breed { Id = breedId, IsActive = true, Stock = 10 };
        //    _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
        //    var (success, error) = await _service.RequestBreed(request);
        //    Assert.False(success);
        //    Assert.Contains("lớn hơn 0", error);
        //}

        [Fact]
        public async Task RequestBreed_ReturnsError_WhenBreedNotFound()
        {
            var breedId = Guid.NewGuid();
            var request = new CreateBreedRequestDto
            {
                BreedItems = new List<BreedItemRequest> { new BreedItemRequest { ItemId = breedId, Quantity = 2 } }
            };
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Breed)null);
            var (success, error) = await _service.RequestBreed(request);
            Assert.False(success);
            Assert.Contains("không tồn tại", error);
        }

        [Fact]
        public async Task RequestBreed_ReturnsError_WhenBreedStockNotEnough()
        {
            var breedId = Guid.NewGuid();
            var request = new CreateBreedRequestDto
            {
                BreedItems = new List<BreedItemRequest> { new BreedItemRequest { ItemId = breedId, Quantity = 20 } }
            };
            var breed = new Breed { Id = breedId, IsActive = true, Stock = 5 };
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
            var (success, error) = await _service.RequestBreed(request);
            Assert.False(success);
            Assert.Contains("không tồn tại, không hoạt động hoặc không đủ tồn kho", error);
        }

        [Fact]
        public async Task RequestBreed_Success()
        {
            var breedId = Guid.NewGuid();
            var request = new CreateBreedRequestDto
            {
                UserRequestId = _userId,
                LivestockCircleId = Guid.NewGuid(),
                Note = "Test",
                BreedItems = new List<BreedItemRequest> { new BreedItemRequest { ItemId = breedId, Quantity = 2 } }
            };
            var breed = new Breed { Id = breedId, IsActive = true, Stock = 10 };
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
            _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>()));
            _billItemRepoMock.Setup(x => x.Insert(It.IsAny<BillItem>()));
            _billRepoMock.Setup(x => x.CommitAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
            var (success, error) = await _service.RequestBreed(request);
            Assert.True(success);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public async Task RequestBreed_ReturnsError_WhenExceptionThrown()
        {
            var breedId = Guid.NewGuid();
            var request = new CreateBreedRequestDto
            {
                UserRequestId = _userId,
                LivestockCircleId = Guid.NewGuid(),
                Note = "Test",
                BreedItems = new List<BreedItemRequest> { new BreedItemRequest { ItemId = breedId, Quantity = 2 } }
            };
            var breed = new Breed { Id = breedId, IsActive = true, Stock = 10 };
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
            _billRepoMock.Setup(x => x.Insert(It.IsAny<Bill>())).Throws(new Exception("DB error"));
            var (success, error) = await _service.RequestBreed(request);
            Assert.False(success);
            Assert.Contains("Lỗi khi tạo yêu cầu giống", error);
        }
    }
}
