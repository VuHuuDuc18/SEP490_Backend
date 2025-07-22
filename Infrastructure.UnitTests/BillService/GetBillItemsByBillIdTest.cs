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
using MockQueryable.Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.BillService
{
    public class GetBillItemsByBillIdTest
    {
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<BillItem>> _billItemRepoMock;
        private readonly Mock<IRepository<Food>> _foodRepoMock;
        private readonly Mock<IRepository<ImageFood>> _imageFoodRepoMock;
        private readonly Mock<IRepository<Medicine>> _medicineRepoMock;
        private readonly Mock<IRepository<ImageMedicine>> _imageMedicineRepoMock;
        private readonly Mock<IRepository<Breed>> _breedRepoMock;
        private readonly Mock<IRepository<ImageBreed>> _imageBreedRepoMock;
        private readonly Infrastructure.Services.Implements.BillService _service;

        public GetBillItemsByBillIdTest()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _billItemRepoMock = new Mock<IRepository<BillItem>>();
            _foodRepoMock = new Mock<IRepository<Food>>();
            _imageFoodRepoMock = new Mock<IRepository<ImageFood>>();
            _medicineRepoMock = new Mock<IRepository<Medicine>>();
            _imageMedicineRepoMock = new Mock<IRepository<ImageMedicine>>();
            _breedRepoMock = new Mock<IRepository<Breed>>();
            _imageBreedRepoMock = new Mock<IRepository<ImageBreed>>();
            _service = new Infrastructure.Services.Implements.BillService(
                _billRepoMock.Object,
                _billItemRepoMock.Object,
                Mock.Of<IRepository<User>>(),
                Mock.Of<IRepository<LivestockCircle>>(),
                _foodRepoMock.Object,
                _medicineRepoMock.Object,
                _breedRepoMock.Object,
                Mock.Of<IRepository<Barn>>(),
                Mock.Of<IRepository<LivestockCircleFood>>(),
                Mock.Of<IRepository<LivestockCircleMedicine>>(),
                _imageFoodRepoMock.Object,
                _imageMedicineRepoMock.Object,
                _imageBreedRepoMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>()
            );
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenRequestNull()
        {
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), null);
            Assert.False(result.Succeeded);
            Assert.Contains("Yêu cầu không được để trống", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenPageIndexInvalid()
        {
            var req = new ListingRequest { PageIndex = 0, PageSize = 10 };
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("PageIndex", result.Message);
        }


        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenPageSizeInvalid()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 0 };
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("PageSize", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenFilterFieldInvalid()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường lọc không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenSearchFieldInvalid()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "test" } }
            };
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường tìm kiếm không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenSortFieldInvalid()
        {
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort =  new SearchObjectForCondition { Field = "InvalidField", Value = "test" } 
            };
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("Trường sắp xếp không hợp lệ", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsError_WhenBillNotFoundOrInactive()
        {
            var req = new ListingRequest { PageIndex = 1, PageSize = 10, Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" } };
            _billRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), null)).ReturnsAsync((Bill)null);
            var result = await _service.GetBillItemsByBillId(Guid.NewGuid(), req);
            Assert.False(result.Succeeded);
            Assert.Contains("Hóa đơn không tồn tại", result.Message);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsBillItems_WhenSuccess_FoodType()
        {
            var billId = Guid.NewGuid();
            var foodId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, TypeBill = Domain.Helper.Constants.TypeBill.FOOD };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);

            var billItems = new List<BillItem>
    {
        new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 5, IsActive = true }
    };
            var billItemsDbSet = billItems.AsQueryable().BuildMockDbSet();
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItemsDbSet.Object);

            var food = new Food { Id = foodId, FoodName = "Test Food", Stock = 100, WeighPerUnit = 1.5f };
            _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, null)).ReturnsAsync(food);

            var imageFoods = new List<ImageFood>
    {
        new ImageFood { Id = Guid.NewGuid(), FoodId = foodId, Thumnail = "true", ImageLink = "img.jpg" }
    };
            var imageFoodsDbSet = imageFoods.AsQueryable().BuildMockDbSet();
            _imageFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
                .Returns(imageFoodsDbSet.Object);

            // Sửa ở đây: thêm Sort hợp lệ
            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };

            var result = await _service.GetBillItemsByBillId(billId, req);
            Assert.True(result.Succeeded, $"Service message: {result.Message}");
            Assert.Equal("Lấy danh sách mục hóa đơn thành công", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Items);
            var item = result.Data.Items.First();
            Assert.NotNull(item.Food);
            Assert.Equal("Test Food", item.Food.FoodName);
            Assert.Equal("img.jpg", item.Food.Thumbnail);
            Assert.Equal(5, item.Stock);
            Assert.True(item.IsActive);
        }
        [Fact]
        public async Task GetBillItemsByBillId_ReturnsBillItems_WhenSuccess_MedicineType()
        {
            var billId = Guid.NewGuid();
            var medicineId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, TypeBill = Domain.Helper.Constants.TypeBill.MEDICINE };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);

            var billItems = new List<BillItem>
            {
                new BillItem { Id = Guid.NewGuid(), BillId = billId, MedicineId = medicineId, Stock = 3, IsActive = true }
            };
            var billItemsDbSet = billItems.AsQueryable().BuildMockDbSet();
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItemsDbSet.Object);

            var medicine = new Medicine { Id = medicineId, MedicineName = "Test Medicine", Stock = 50 };
            _medicineRepoMock.Setup(x => x.GetByIdAsync(medicineId, null)).ReturnsAsync(medicine);

            var imageMedicines = new List<ImageMedicine>
            {
                new ImageMedicine { Id = Guid.NewGuid(), MedicineId = medicineId, Thumnail = "true", ImageLink = "med.jpg" }
            };
            var imageMedicinesDbSet = imageMedicines.AsQueryable().BuildMockDbSet();
            _imageMedicineRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageMedicine, bool>>>()))
                .Returns(imageMedicinesDbSet.Object);

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await _service.GetBillItemsByBillId(billId, req);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.Items);
            var item = result.Data.Items.First();
            Assert.NotNull(item.Medicine);
            Assert.Equal("Test Medicine", item.Medicine.MedicineName);
            Assert.Equal("med.jpg", item.Medicine.Thumbnail);
            Assert.Equal(3, item.Stock);
            Assert.True(item.IsActive);
        }

        [Fact]
        public async Task GetBillItemsByBillId_ReturnsBillItems_WhenSuccess_BreedType()
        {
            var billId = Guid.NewGuid();
            var breedId = Guid.NewGuid();
            var bill = new Bill { Id = billId, IsActive = true, TypeBill = Domain.Helper.Constants.TypeBill.BREED };
            _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);

            var billItems = new List<BillItem>
            {
                new BillItem { Id = Guid.NewGuid(), BillId = billId, BreedId = breedId, Stock = 7, IsActive = true }
            };
            var billItemsDbSet = billItems.AsQueryable().BuildMockDbSet();
            _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
                .Returns(billItemsDbSet.Object);

            var breed = new Breed { Id = breedId, BreedName = "Test Breed", Stock = 20 };
            _breedRepoMock.Setup(x => x.GetByIdAsync(breedId, null)).ReturnsAsync(breed);

            var imageBreeds = new List<ImageBreed>
            {
                new ImageBreed { Id = Guid.NewGuid(), BreedId = breedId, Thumnail = "true", ImageLink = "breed.jpg" }
            };
            var imageBreedsDbSet = imageBreeds.AsQueryable().BuildMockDbSet();
            _imageBreedRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageBreed, bool>>>()))
                .Returns(imageBreedsDbSet.Object);

            var req = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 10,
                Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
            };
            var result = await _service.GetBillItemsByBillId(billId, req);
            Assert.True(result.Succeeded);
            Assert.Single(result.Data.Items);
            var item = result.Data.Items.First();
            Assert.NotNull(item.Breed);
            Assert.Equal("Test Breed", item.Breed.BreedName);
            Assert.Equal("breed.jpg", item.Breed.Thumbnail);
            Assert.Equal(7, item.Stock);
            Assert.True(item.IsActive);
        }

        //[Fact]
        //public async Task GetBillItemsByBillId_DoesNotReturnInactiveBillItems()
        //{
        //    var billId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true, TypeBill = Domain.Helper.Constants.TypeBill.FOOD };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);

        //    var billItems = new List<BillItem>
        //    {
        //        new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 5, IsActive = false }
        //    };
        //    var billItemsDbSet = billItems.AsQueryable().BuildMockDbSet();
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns(billItemsDbSet.Object);

        //    var req = new ListingRequest
        //    {
        //        PageIndex = 1,
        //        PageSize = 10,
        //        Sort = new SearchObjectForCondition { Field = "Id", Value = "asc" }
        //    };
        //    var result = await _service.GetBillItemsByBillId(billId, req);
        //    Assert.True(result.Succeeded);
        //    Assert.Empty(result.Data.Items);
        //}

        //[Fact]
        //public async Task GetBillItemsByBillId_ReturnsNullThumbnail_WhenNoImage()
        //{
        //    var billId = Guid.NewGuid();
        //    var foodId = Guid.NewGuid();
        //    var bill = new Bill { Id = billId, IsActive = true, TypeBill = Domain.Helper.Constants.TypeBill.FOOD };
        //    _billRepoMock.Setup(x => x.GetByIdAsync(billId, null)).ReturnsAsync(bill);

        //    var billItems = new List<BillItem>
        //    {
        //        new BillItem { Id = Guid.NewGuid(), BillId = billId, FoodId = foodId, Stock = 5, IsActive = true }
        //    };
        //    var billItemsDbSet = billItems.AsQueryable().BuildMockDbSet();
        //    _billItemRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<BillItem, bool>>>()))
        //        .Returns(billItemsDbSet.Object);

        //    var food = new Food { Id = foodId, FoodName = "Test Food", Stock = 100, WeighPerUnit = 1.5f };
        //    _foodRepoMock.Setup(x => x.GetByIdAsync(foodId, null)).ReturnsAsync(food);

        //    var imageFoods = new List<ImageFood>();
        //    var imageFoodsDbSet = imageFoods.AsQueryable().BuildMockDbSet();
        //    _imageFoodRepoMock.Setup(x => x.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<ImageFood, bool>>>()))
        //        .Returns(imageFoodsDbSet.Object);

        //    var req = new ListingRequest { PageIndex = 1, PageSize = 10 };
        //    var result = await _service.GetBillItemsByBillId(billId, req);
        //    Assert.True(result.Succeeded);
        //    Assert.Single(result.Data.Items);
        //    var item = result.Data.Items.First();
        //    Assert.NotNull(item.Food);
        //    Assert.Null(item.Food.Thumbnail);
        //}
    }
}
