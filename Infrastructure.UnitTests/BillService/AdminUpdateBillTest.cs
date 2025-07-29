using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request.Bill.Admin;
using Entities.EntityModel;
using Infrastructure.Repository;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using System.Threading;
using MockQueryable;
using Infrastructure.UnitTests.BarnPlanService;

namespace Infrastructure.UnitTests.BillService
{
    public class AdminUpdateBillTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Breed>> _breedRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepoMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public AdminUpdateBillTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _breedRepoMock = new Mock<IRepository<Breed>>();
            _livestockCircleRepoMock = new Mock<IRepository<LivestockCircle>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                new Mock<IRepository<User>>().Object,
                _livestockCircleRepoMock.Object,
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
        public async Task AdminUpdateBill_ReturnsFalse_WhenBillNotFound()
        {
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Bill)null);
            var request = new Admin_UpdateBarnRequest { LivestockCircleId = Guid.NewGuid(), BreedId = Guid.NewGuid(), Stock = 5 };
            var result = await _service.AdminUpdateBill(request);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task AdminUpdateBill_ReturnsFalse_WhenBillStatusNotRequested()
        {
            var lscId = Guid.NewGuid();
            var bill = new Bill { Id = Guid.NewGuid(), Status = "APPROVED" , LivestockCircleId = lscId};
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            var request = new Admin_UpdateBarnRequest { LivestockCircleId = lscId, BreedId = Guid.NewGuid(), Stock = 5 };
            var result = await _service.AdminUpdateBill(request);
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task AdminUpdateBill_ThrowsException_WhenBreedStockNotValid()
        {
            var lscid = Guid.NewGuid();
            var bill = new Bill { Id = Guid.NewGuid(), Status = "REQUESTED", LivestockCircleId = lscid };
            
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _breedRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((Breed)null);
            var request = new Admin_UpdateBarnRequest { LivestockCircleId = lscid, BreedId = Guid.NewGuid(), Stock = 5 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await _service.AdminUpdateBill(request));
        }

        [Fact]
        public async Task AdminUpdateBill_Success()
        {
            var billId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var bill = new Bill { Id = billId, Status = "REQUESTED", LivestockCircleId = lscId };
            var breed = new Breed { Id = breedId, Stock = 10, IsActive = true };
            var billItem = new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true };
            var lsc = new LivestockCircle { Id = lscId };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(new List<BillItem> { billItem }.AsQueryable().BuildMock());
            _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(lsc);
            _billItemRepoMock.Setup(x => x.Update(It.IsAny<BillItem>()));
            _billItemRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _livestockCircleRepoMock.Setup(x => x.Update(It.IsAny<LivestockCircle>()));
            _livestockCircleRepoMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var request = new Admin_UpdateBarnRequest { LivestockCircleId = lscId, BreedId = breedId, Stock = 5 };
            var result = await _service.AdminUpdateBill(request);
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task AdminUpdateBill_ThrowsException_WhenExceptionOccurs()
        {
            var billId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var bill = new Bill { Id = billId, Status = "REQUESTED", LivestockCircleId = lscId };
            var breed = new Breed { Id = breedId, Stock = 10, IsActive = true };
            var billItem = new BillItem { Id = Guid.NewGuid(), BillId = billId, IsActive = true };
            var lsc = new LivestockCircle { Id = lscId };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(bill);
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(breed);
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>())).Returns(new List<BillItem> { billItem }.AsQueryable().BuildMock());
            _livestockCircleRepoMock.Setup(x => x.GetByIdAsync(lscId, It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(lsc);
            _billItemRepoMock.Setup(x => x.Update(It.IsAny<BillItem>())).Throws(new Exception("DB error"));
            var request = new Admin_UpdateBarnRequest { LivestockCircleId = lscId, BreedId = breedId, Stock = 5 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await _service.AdminUpdateBill(request));
        }
    }
}
