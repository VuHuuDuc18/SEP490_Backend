using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Response.Bill;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;
using Domain.Helper.Constants;

namespace Infrastructure.UnitTests.BillService
{
    public class GetBillRequestByTypeTest
    {
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IRepository<LivestockCircle>> _livestockCircleRepoMock;
        private readonly Mock<IRepository<Barn>> _barnRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.BillService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public GetBillRequestByTypeTest()
        {
            _userRepoMock = new Mock<IRepository<User>>();
            _livestockCircleRepoMock = new Mock<IRepository<LivestockCircle>>();
            _barnRepoMock = new Mock<IRepository<Barn>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            // Setup HttpContext with user claims
            var claims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim("uid", _userId.ToString()) };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            // BillService sẽ được khởi tạo trong từng test với repo thật
            _service = null;
        }

        private Infrastructure.Services.Implements.BillService CreateServiceWithDb(TestBillDbContext context)
        {
            var billRepoMock = new Mock<IRepository<Bill>>();
            billRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Bill, bool>>>()))
                .Returns((System.Linq.Expressions.Expression<Func<Bill, bool>> expr) => context.Bills.Where(expr));
            billRepoMock.Setup(x => x.GetQueryable()).Returns(context.Bills);
            return new Infrastructure.Services.Implements.BillService(
                billRepoMock.Object,
                null,
                _userRepoMock.Object,
                _livestockCircleRepoMock.Object,
                null,
                null,
                null,
                _barnRepoMock.Object,
                null,
                null,
                null,
                null,
                null,
                _httpContextAccessorMock.Object
            );
        }

        //[Fact]
        //public async Task GetBillRequestByType_ReturnsError_WhenRequestNull()
        //{
        //    var context = new TestBillDbContext(InMemoryOptions());
        //    var service = CreateServiceWithDb(context);
        //    var result = await service.GetBillRequestByType(null, "Food");
        //    Assert.False(result.Succeeded);
        //    Assert.Contains("Yêu cầu không được để trống", result.Message);
        //}

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenPageIndexInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest { PageIndex = 0, PageSize = 10, Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" } };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenPageSizeInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest { PageIndex = 1, PageSize = 0, Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" } };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenFilterFieldInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "abc" } },
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenSearchFieldInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "abc" } },
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenSortFieldInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_ReturnsError_WhenBillTypeInvalid()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" } };
            var result = await service.GetBillRequestByType(req, "InvalidType");
            Assert.False(result.Succeeded);
            Assert.Contains("Loại hóa đơn không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillRequestByType_Success_WithValidFilter()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var bill = new Bill {
                Id = Guid.NewGuid(),
                Name = "Bill1",
                TypeBill = "Food",
                Status = StatusConstant.REQUESTED,
                IsActive = true,
                DeliveryDate = DateTime.Now,
                Note = "Test note",
                Total = 1,
                Weight = 1
            };
            context.Bills.Add(bill);
            context.SaveChanges();
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Bill1" } },
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.Items);
            Assert.Equal("Bill1", result.Data.Items[0].Name);
        }

        [Fact]
        public async Task GetBillRequestByType_Success_WithValidSearch()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var bill = new Bill {
                Id = Guid.NewGuid(),
                Name = "Bill1",
                TypeBill = "Food",
                Status = StatusConstant.REQUESTED,
                IsActive = true,
                Note = "Test note",
                DeliveryDate = DateTime.Now,
                Total = 1,
                Weight = 1
            };
            context.Bills.Add(bill);
            context.SaveChanges();
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Bill1" } },
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.Items);
            Assert.Equal("Bill1", result.Data.Items[0].Name);
        }

        [Fact]
        public async Task GetBillRequestByType_Success_WithValidSort()
        {
            var context = new TestBillDbContext(InMemoryOptions());
            var bill = new Bill {
                Id = Guid.NewGuid(),
                Name = "Bill1",
                TypeBill = "Food",
                Status = StatusConstant.REQUESTED,
                IsActive = true,
                DeliveryDate = DateTime.Now,
                Note = "Test note",
                Total = 1,
                Weight = 1
            };
            context.Bills.Add(bill);
            context.SaveChanges();
            var service = CreateServiceWithDb(context);
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" }
            };
            var result = await service.GetBillRequestByType(req, "Food");
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.Items);
            Assert.Equal("Bill1", result.Data.Items[0].Name);
        }

        //[Fact]
        //public async Task GetBillRequestByType_Success_WithAllValid()
        //{
        //    var context = new TestBillDbContext(InMemoryOptions());
        //    var bill = new Bill {
        //        Id = Guid.NewGuid(),
        //        Name = "Bill1",
        //        TypeBill = "Food",
        //        Status = StatusConstant.REQUESTED,
        //        IsActive = true,
        //        DeliveryDate = DateTime.Now,
        //        Note = "Test note",
        //        Total = 1,
        //        Weight = 1
        //    };
        //    context.Bills.Add(bill);
        //    context.SaveChanges();
        //    var service = CreateServiceWithDb(context);
        //    var req = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Bill1" } },
        //        SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Name", Value = "Bill1" } },
        //        Sort = new SearchObjectForCondition { Field = "Name", Value = "asc" }
        //    };
        //    var result = await service.GetBillRequestByType(req, "Food");
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data.Items);
        //    Assert.Equal("Bill1", result.Data.Items[0].Name);
        //}

        //[Fact]
        //public async Task GetBillRequestByType_Success_WithoutFilterSearchSort()
        //{
        //    var context = new TestBillDbContext(InMemoryOptions());
        //    var bill = new Bill {
        //        Id = Guid.NewGuid(),
        //        Name = "Bill1",
        //        TypeBill = "Food",
        //        Status = StatusConstant.REQUESTED,
        //        IsActive = true,
        //        DeliveryDate = DateTime.Now,
        //        Note = "Test note",
        //        Total = 1,
        //        Weight = 1
        //    };
        //    context.Bills.Add(bill);
        //    context.SaveChanges();
        //    var service = CreateServiceWithDb(context);
        //    var req = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
        //    };
        //    var result = await service.GetBillRequestByType(req, "Food");
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data.Items);
        //    Assert.Equal("Bill1", result.Data.Items[0].Name);
        //}

        private static DbContextOptions<TestBillDbContext> InMemoryOptions()
        {
            return new DbContextOptionsBuilder<TestBillDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }

    public class TestBillDbContext : DbContext
    {
        public TestBillDbContext(DbContextOptions<TestBillDbContext> options) : base(options) { }
        public DbSet<Bill> Bills { get; set; }
    }
}
