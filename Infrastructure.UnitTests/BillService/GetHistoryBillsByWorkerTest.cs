using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using MockQueryable;

namespace Infrastructure.UnitTests.BillService
{
    public class GetHistoryBillsByWorkerTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _lscRepoMock;
        private readonly Mock<IRepository<Barn>> _barnRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public GetHistoryBillsByWorkerTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _lscRepoMock = new Mock<IRepository<LivestockCircle>>();
            _barnRepoMock = new Mock<IRepository<Barn>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("uid", _userId.ToString()) }));
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                Mock.Of<IRepository<BillItem>>(),
                _userRepoMock.Object,
                _lscRepoMock.Object,
                Mock.Of<IRepository<Food>>(),
                Mock.Of<IRepository<Medicine>>(),
                Mock.Of<IRepository<Breed>>(),
                _barnRepoMock.Object,
                Mock.Of<IRepository<LivestockCircleFood>>(),
                Mock.Of<IRepository<LivestockCircleMedicine>>(),
                Mock.Of<IRepository<ImageFood>>(),
                Mock.Of<IRepository<ImageMedicine>>(),
                Mock.Of<IRepository<ImageBreed>>(),
                _httpContextAccessorMock.Object
            );
        }

        //[Fact]
        //public async Task NotLoggedIn_ReturnsError()
        //{
        //    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        //    var service = new Infrastructure.Services.Implements.BillService(
        //        _billRepoMock.Object,
        //        Mock.Of<IRepository<BillItem>>(),
        //        _userRepoMock.Object,
        //        _lscRepoMock.Object,
        //        Mock.Of<IRepository<Food>>(),
        //        Mock.Of<IRepository<Medicine>>(),
        //        Mock.Of<IRepository<Breed>>(),
        //        _barnRepoMock.Object,
        //        Mock.Of<IRepository<LivestockCircleFood>>(),
        //        Mock.Of<IRepository<LivestockCircleMedicine>>(),
        //        Mock.Of<IRepository<ImageFood>>(),
        //        Mock.Of<IRepository<ImageMedicine>>(),
        //        Mock.Of<IRepository<ImageBreed>>(),
        //        _httpContextAccessorMock.Object
        //    );
        //    var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    var result = await service.GetHistoryBillsByWorker(req);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        //}

        [Fact]
        public async Task WorkerNotFound_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((User?)null);
            var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RequestNull_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var result = await _service.GetHistoryBillsByWorker(null);
            Assert.False(result.Succeeded);
            Assert.Contains("không được để trống", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InvalidPageIndex_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InvalidPageSize_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InvalidFilterField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField" } } };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InvalidSearchField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField" } } };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InvalidSortField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Sort = new SearchObjectForCondition { Field = "InvalidField" } };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Success_ReturnsBills()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var barnId = Guid.NewGuid();
            var lscId = Guid.NewGuid();
            var bills = new List<Bill> {
                new Bill { Id = Guid.NewGuid(), LivestockCircleId = lscId, IsActive = true,DeliveryDate = DateTime.Now,  Status = "COMPLETED", UserRequestId = _userId },
                new Bill { Id = Guid.NewGuid(), LivestockCircleId = lscId, IsActive = true, DeliveryDate = DateTime.Now, Status = "DONE", UserRequestId = _userId }
            };
            var lscs = new List<LivestockCircle> { new LivestockCircle { Id = lscId, BarnId = barnId } };
            var barns = new List<Barn> { new Barn { Id = barnId, WorkerId = _userId } };
            _barnRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Barn, bool>>>())).Returns((System.Linq.Expressions.Expression<Func<Barn, bool>> expr) => barns.AsQueryable().BuildMock().Where(expr));
            _lscRepoMock.Setup(x => x.GetQueryable()).Returns(lscs.AsQueryable().BuildMock());
            _billRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Bill, bool>>>())).Returns((System.Linq.Expressions.Expression<Func<Bill, bool>> expr) => bills.AsQueryable().BuildMock().Where(expr));
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId, FullName = "Worker", Email = "worker@email.com" });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Sort = new SearchObjectForCondition { Field = "Id" , Value = "asc" } };
            var result = await _service.GetHistoryBillsByWorker(req);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        //[Fact]
        //public async Task Exception_ReturnsError()
        //{
        //    _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ThrowsAsync(new Exception("db error"));
        //    var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    var result = await _service.GetHistoryBillsByWorker(req);
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Lỗi khi lấy danh sách hóa đơn", result.Message, StringComparison.OrdinalIgnoreCase);
        //}
    }
}
