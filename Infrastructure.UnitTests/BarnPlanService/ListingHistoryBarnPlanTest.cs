using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Dto.Request;
using Domain.Dto.Response.BarnPlan;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services.Implements;
using Moq;
using MockQueryable.Moq;
using Xunit;
using MockQueryable;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BarnPlanService
{
    public class ListingHistoryBarnPlanTest
    {
        private readonly Mock<IRepository<BarnPlan>> _barnPlanRepoMock;
        private readonly Mock<IRepository<BarnPlanFood>> _barnPlanFoodRepoMock;
        private readonly Mock<IRepository<BarnPlanMedicine>> _barnPlanMedicineRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Infrastructure.Services.Implements.BarnPlanService _service;

        public ListingHistoryBarnPlanTest()
        {
            _barnPlanRepoMock = new Mock<IRepository<BarnPlan>>();
            _barnPlanFoodRepoMock = new Mock<IRepository<BarnPlanFood>>();
            _barnPlanMedicineRepoMock = new Mock<IRepository<BarnPlanMedicine>>();
            _service = new Infrastructure.Services.Implements.BarnPlanService(
                _barnPlanRepoMock.Object,
                _barnPlanFoodRepoMock.Object,
                _barnPlanMedicineRepoMock.Object,
                _userRepoMock.Object
                );
        }

        //[Fact]
        //public async Task ListingHistoryBarnPlan_Throws_WhenRequestNull()
        //{
        //    var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), null));
        //    Assert.Contains("Yêu cầu không được null", ex.Message);
        //}

        [Fact]
        public async Task ListingHistoryBarnPlan_Throws_WhenPageIndexInvalid()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), req));
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", ex.Message);

        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Throws_WhenPageSizeInvalid()
        {

            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), req));
            Assert.Contains("PageIndex và PageSize phải lớn hơn 0", ex.Message);
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Throws_WhenInvalidFilterField()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), req));
            Assert.Contains("Trường lọc không hợp lệ", ex.Message);
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Throws_WhenInvalidSearchField()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), req));
            Assert.Contains("Trường tìm kiếm không hợp lệ", ex.Message);
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Throws_WhenInvalidSortField()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ListingHistoryBarnPlan(Guid.NewGuid(), req));
            Assert.Contains("Trường sắp xếp không hợp lệ", ex.Message);
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Success_ReturnsPaginatedData()
        {
            var livestockCircleId = Guid.NewGuid();
            var barnPlanList = new List<BarnPlan>
            {
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Plan 1",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    IsActive = true
                },
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Plan 2",
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    IsActive = true
                }
            };

            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(barnPlanList.AsQueryable().BuildMock());

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Note", Value = "asc" }
            };

            var result = await _service.ListingHistoryBarnPlan(livestockCircleId, req);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.Note == "Plan 1");
            Assert.Contains(result.Data.Items, x => x.Note == "Plan 2");
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Success_WithSearch()
        {
            var livestockCircleId = Guid.NewGuid();
            var barnPlanList = new List<BarnPlan>
            {
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Searchable",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    IsActive = true
                },
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Plan 2",
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    IsActive = true
                }
            };

            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(barnPlanList.AsQueryable().BuildMock());

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Note", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Note", Value = "Searchable" } }

            };

            var result = await _service.ListingHistoryBarnPlan(livestockCircleId, req);

            Assert.NotNull(result);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.Note == "Searchable");
        }

        [Fact]
        public async Task ListingHistoryBarnPlan_Success_WithFilter()
        {
            var livestockCircleId = Guid.NewGuid();
            var barnPlanList = new List<BarnPlan>
            {
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Filterable",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    IsActive = true
                },
                new BarnPlan
                {
                    Id = Guid.NewGuid(),
                    LivestockCircleId = livestockCircleId,
                    Note = "Plan 2",
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(3),
                    IsActive = true
                }
            };

            _barnPlanRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BarnPlan, bool>>>()))
                .Returns(barnPlanList.AsQueryable().BuildMock());

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Note", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Note", Value = "Filterable" } }

            };

            var result = await _service.ListingHistoryBarnPlan(livestockCircleId, req);

            Assert.NotNull(result);
            Assert.Equal(1, result.Data.Items.Count);
            Assert.Contains(result.Data.Items, x => x.Note == "Filterable");
        }
    }
}