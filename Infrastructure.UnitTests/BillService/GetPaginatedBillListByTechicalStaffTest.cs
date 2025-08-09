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
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BillService
{
    public class GetPaginatedBillListByTechicalStaffTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _lscRepoMock;
        private readonly Mock<IRepository<Barn>> _barnRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public GetPaginatedBillListByTechicalStaffTest()
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
                new Mock<IRepository<BillItem>>().Object,
                _userRepoMock.Object,
                _lscRepoMock.Object,
                new Mock<IRepository<Food>>().Object,
                new Mock<IRepository<Medicine>>().Object,
                new Mock<IRepository<Breed>>().Object,
                _barnRepoMock.Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_NotLoggedIn_ReturnsError()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                new Mock<IRepository<BillItem>>().Object,
                _userRepoMock.Object,
                _lscRepoMock.Object,
                new Mock<IRepository<Food>>().Object,
                new Mock<IRepository<Medicine>>().Object,
                new Mock<IRepository<Breed>>().Object,
                _barnRepoMock.Object,
                new Mock<IRepository<LivestockCircleFood>>().Object,
                new Mock<IRepository<LivestockCircleMedicine>>().Object,
                new Mock<IRepository<ImageFood>>().Object,
                new Mock<IRepository<ImageMedicine>>().Object,
                new Mock<IRepository<ImageBreed>>().Object,
                _httpContextAccessorMock.Object
            );
            var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
            var result = await service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("đăng nhập", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_RequestNull_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var result = await _service.GetPaginatedBillListByTechicalStaff(null, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("không được để trống", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_InvalidPageIndex_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_InvalidPageSize_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex và PageSize", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_InvalidFilterField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField" } } };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_InvalidSearchField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField" } } };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_InvalidSortField_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync(new User { Id = _userId });
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Sort = new SearchObjectForCondition { Field = "InvalidField" } };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_UserNotFound_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).ReturnsAsync((User?)null);
            var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("kỹ thuật không tồn tại", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_Success_ReturnsBills()
        {
            // Mock User trả về đúng userId và thông tin
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>()))
                .ReturnsAsync((Guid id, Infrastructure.Core.Ref<Infrastructure.Core.CheckError> _) => new User { Id = id, FullName = "User", Email = "user@email.com" });
            // Mock LivestockCircle trả về object hợp lệ
            _lscRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>()))
                .ReturnsAsync((Guid id, Infrastructure.Core.Ref<Infrastructure.Core.CheckError> _) => new LivestockCircle { Id = id, BarnId = Guid.NewGuid(), LivestockCircleName = "LSC" });
            // Mock Barn trả về object hợp lệ
            _barnRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>()))
                .ReturnsAsync((Guid id, Infrastructure.Core.Ref<Infrastructure.Core.CheckError> _) => new Barn { Id = id, BarnName = "Barn", Address = "Addr", Image = "img", WorkerId = Guid.NewGuid() });

            var bills = new List<Bill> {
                new Bill { Id = Guid.NewGuid(), UserRequestId = _userId, IsActive = true, TypeBill = "Food", Name = "Bill1",DeliveryDate = DateTime.Now,  Note = "N1", Total = 1, Status = "REQUESTED", Weight = 1 },
                new Bill { Id = Guid.NewGuid(), UserRequestId = _userId, IsActive = true, TypeBill = "Food", Name = "Bill2",DeliveryDate = DateTime.Now,  Note = "N2", Total = 2, Status = "REQUESTED", Weight = 2 }
            };
            var billsDb = new TestBillDbContext().SetupBills(bills);
            _billRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Bill, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Bill, bool>> expr) => billsDb.Bills.Where(expr));
            var req = new ListingRequest { PageIndex = 1, PageSize = 10 , Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" } };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
        }

        [Fact]
        public async Task GetPaginatedBillListByTechicalStaff_Exception_ReturnsError()
        {
            _userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Infrastructure.Core.Ref<Infrastructure.Core.CheckError>>())).Throws(new Exception("DB error"));
            var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
            var result = await _service.GetPaginatedBillListByTechicalStaff(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Lỗi khi lấy danh sách hóa đơn", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // Minimal InMemory DbContext for test
        public class TestBillDbContext : DbContext
        {
            public DbSet<Bill> Bills { get; set; }
            public TestBillDbContext() : base(new DbContextOptionsBuilder<TestBillDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { }
            public TestBillDbContext SetupBills(IEnumerable<Bill> bills)
            {
                Bills.AddRange(bills);
                SaveChanges();
                return this;
            }
        }
    }
}
