using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Extensions;
using Domain.Helper.Constants;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class BillService : IBillService
    {
        private readonly IRepository<Bill> _billRepository;
        private readonly IRepository<BillItem> _billItemRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<Medicine> _medicineRepository;
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<LivestockCircleFood> _livestockCircleFoodRepository;
        private readonly IRepository<LivestockCircleMedicine> _livestockCircleMedicineRepository;

        public BillService(
            IRepository<Bill> billRepository,
            IRepository<BillItem> billItemRepository,
            IRepository<User> userRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<Food> foodRepository,
            IRepository<Medicine> medicineRepository,
            IRepository<Breed> breedRepository,
            IRepository<LivestockCircleFood> livestockCircleFoodRepository,
            IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository)
        {
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
            _billItemRepository = billItemRepository ?? throw new ArgumentNullException(nameof(billItemRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
            _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository));
            _livestockCircleFoodRepository = livestockCircleFoodRepository ?? throw new ArgumentNullException(nameof(livestockCircleFoodRepository));
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository ?? throw new ArgumentNullException(nameof(livestockCircleMedicineRepository));
        }

        private async Task<(bool Success, string ErrorMessage)> ValidateItem(Guid itemId, int quantity, bool isFood, bool isMedicine, bool isBreed, CancellationToken cancellationToken)
        {
            var checkError = new Ref<CheckError>();
            if (isFood)
            {
                var food = await _foodRepository.GetById(itemId, checkError);
                return food != null && food.IsActive && food.Stock >= quantity
                    ? (true, null)
                    : (false, $"Thức ăn với ID {itemId} không tồn tại, không hoạt động hoặc không đủ tồn kho.");
            }
            else if (isMedicine)
            {
                var medicine = await _medicineRepository.GetById(itemId, checkError);
                return medicine != null && medicine.IsActive && medicine.Stock >= quantity
                    ? (true, null)
                    : (false, $"Thuốc với ID {itemId} không tồn tại, không hoạt động hoặc không đủ tồn kho.");
            }
            else if (isBreed)
            {
                var breed = await _breedRepository.GetById(itemId, checkError);
                return breed != null && breed.IsActive && breed.Stock >= quantity
                    ? (true, null)
                    : (false, $"Giống với ID {itemId} không tồn tại, không hoạt động hoặc không đủ tồn kho.");
            }
            return (false, "Phải chỉ định chính xác một loại mặt hàng (Food, Medicine hoặc Breed).");
        }

        private async Task UpdateStock(BillItem billItem, int quantity, CancellationToken cancellationToken)
        {
            var checkError = new Ref<CheckError>();
            if (billItem.FoodId.HasValue)
            {
                var food = await _foodRepository.GetById(billItem.FoodId.Value, checkError);
                if (food != null) { food.Stock -= quantity; _foodRepository.Update(food); }
            }
            else if (billItem.MedicineId.HasValue)
            {
                var medicine = await _medicineRepository.GetById(billItem.MedicineId.Value, checkError);
                if (medicine != null) { medicine.Stock -= quantity; _medicineRepository.Update(medicine); }
            }
            else if (billItem.BreedId.HasValue)
            {
                var breed = await _breedRepository.GetById(billItem.BreedId.Value, checkError);
                if (breed != null) { breed.Stock -= quantity; _breedRepository.Update(breed); }
            }
            await Task.CompletedTask;
        }

        private async Task UpdateLivestockCircle(Guid livestockCircleId, int quantity, int? deadUnit, float? averageWeight, CancellationToken cancellationToken)
        {
            var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, new Ref<CheckError>());
            if (livestockCircle != null)
            {
                livestockCircle.TotalUnit = (quantity - (deadUnit ?? 0));
                livestockCircle.StartDate = DateTime.UtcNow;
                livestockCircle.AverageWeight = averageWeight ?? livestockCircle.AverageWeight;
                _livestockCircleRepository.Update(livestockCircle);
            }
            await Task.CompletedTask;
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var billItem = await _billItemRepository.GetById(billItemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");

            var bill = await _billRepository.GetById(billItem.BillId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            try
            {
                billItem.IsActive = false;
                bill.Total -= billItem.Stock;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi vô hiệu hóa mục hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            try
            {
                bill.IsActive = false;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi vô hiệu hóa hóa đơn: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(Guid billId, ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Yêu cầu không được để trống.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true) return (null, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
                if (bill == null) return (null, "Hóa đơn không tồn tại.");

                var query = _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive);

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(i => new BillItemResponse
                {
                    Id = i.Id,
                    BillId = i.BillId,
                    FoodId = i.FoodId,
                    MedicineId = i.MedicineId,
                    BreedId = i.BreedId,
                    Stock = i.Stock,
                    IsActive = i.IsActive
                }).ToList();

                var result = new PaginationSet<BillItemResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách mục hóa đơn: {ex.Message}");
            }
        }

        public async Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (null, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null) return (null, "Hóa đơn không tồn tại.");

            var response = new BillResponse
            {
                Id = bill.Id,
                UserRequestId = bill.UserRequestId,
                LivestockCircleId = bill.LivestockCircleId,
                Name = bill.Name,
                Note = bill.Note,
                Total = bill.Total,
                Weight = bill.Weight,
                IsActive = bill.IsActive
            };

            return (response, null);
        }

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Yêu cầu không được để trống.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _billRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(b => new BillResponse
                {
                    Id = b.Id,
                    UserRequestId = b.UserRequestId,
                    LivestockCircleId = b.LivestockCircleId,
                    Name = b.Name,
                    Note = b.Note,
                    Total = b.Total,
                    Weight = b.Weight,
                    IsActive = b.IsActive
                }).ToList();

                var result = new PaginationSet<BillResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(ListingRequest request, string itemCategory, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Yêu cầu không được để trống.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                if (!new[] { "Food", "Medicine", "Breed" }.Contains(itemCategory, StringComparer.OrdinalIgnoreCase))
                    return (null, $"Loại mặt hàng không hợp lệ: {itemCategory}. Phải là 'Food', 'Medicine' hoặc 'Breed'.");

                var billItemQuery = itemCategory.ToLower() switch
                {
                    "food" => _billItemRepository.GetQueryable(x => x.IsActive && x.FoodId.HasValue && !x.MedicineId.HasValue && !x.BreedId.HasValue),
                    "medicine" => _billItemRepository.GetQueryable(x => x.IsActive && x.MedicineId.HasValue && !x.FoodId.HasValue && !x.BreedId.HasValue),
                    "breed" => _billItemRepository.GetQueryable(x => x.IsActive && x.BreedId.HasValue && !x.FoodId.HasValue && !x.MedicineId.HasValue),
                    _ => throw new InvalidOperationException("Loại mặt hàng không được hỗ trợ.")
                };

                var billIds = await billItemQuery.Select(x => x.BillId).Distinct().ToListAsync(cancellationToken);
                var query = _billRepository.GetQueryable(x => x.IsActive && billIds.Contains(x.Id));

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(b => new BillResponse
                {
                    Id = b.Id,
                    UserRequestId = b.UserRequestId,
                    LivestockCircleId = b.LivestockCircleId,
                    Name = b.Name,
                    Note = b.Note,
                    Total = b.Total,
                    Weight = b.Weight,
                    IsActive = b.IsActive
                }).ToList();

                var result = new PaginationSet<BillResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách hóa đơn theo {itemCategory.ToLower()}: {ex.Message}");
            }
        }


        public async Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                var validStatuses = new[] { StatusConstant.REQUESTED, StatusConstant.APPROVED, StatusConstant.CONFIRMED, StatusConstant.REJECTED, StatusConstant.COMPLETED, StatusConstant.CANCELLED };
                if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                    return (false, $"Trạng thái không hợp lệ: {newStatus}. Phải là một trong: {string.Join(", ", validStatuses)}.");

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
                if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

                if (bill.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                    return (false, $"Hóa đơn đã ở trạng thái {newStatus}.");

                try
                {
                    bill.Status = newStatus;

                    if (newStatus == StatusConstant.APPROVED)
                    {
                        var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
                        foreach (var item in billItems)
                        {
                            await UpdateStock(item, item.Stock, cancellationToken);
                        }
                    }
                    else if (newStatus == StatusConstant.CONFIRMED)
                    {
                        var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
                        foreach (var item in billItems)
                        {
                            if (item.FoodId.HasValue)
                            {
                                var lcFood = await _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.FoodId == item.FoodId).FirstOrDefaultAsync(cancellationToken);
                                if (lcFood == null)
                                {
                                    lcFood = new LivestockCircleFood { LivestockCircleId = bill.LivestockCircleId, FoodId = item.FoodId.Value, Remaining = 0 };
                                    _livestockCircleFoodRepository.Insert(lcFood);
                                }
                                lcFood.Remaining += item.Stock;
                                _livestockCircleFoodRepository.Update(lcFood);
                            }
                            else if (item.MedicineId.HasValue)
                            {
                                var lcMedicine = await _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.MedicineId == item.MedicineId).FirstOrDefaultAsync(cancellationToken);
                                if (lcMedicine == null)
                                {
                                    lcMedicine = new LivestockCircleMedicine { LivestockCircleId = bill.LivestockCircleId, MedicineId = item.MedicineId.Value, Remaining = 0 };
                                    _livestockCircleMedicineRepository.Insert(lcMedicine);
                                }
                                lcMedicine.Remaining += item.Stock;
                                _livestockCircleMedicineRepository.Update(lcMedicine);
                            }
                            else if (item.BreedId.HasValue)
                            {
                                await UpdateLivestockCircle(bill.LivestockCircleId, item.Stock, 0, null, cancellationToken);
                            }
                        }
                    }

                    _billRepository.Update(bill);
                    await _billRepository.CommitAsync(cancellationToken);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi cập nhật trạng thái hóa đơn: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thay đổi trạng thái hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddFoodItemToBill(Guid billId, AddFoodItemToBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.FoodItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thức ăn.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
            if (existingItems.Any() && !existingItems.All(x => x.FoodId.HasValue))
                return (false, "Hóa đơn chỉ được chứa mặt hàng thức ăn.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            foreach (var item in request.FoodItems)
            {
                var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, true, false, false, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            try
            {
                foreach (var item in request.FoodItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = item.ItemId,
                        MedicineId = null,
                        BreedId = null,
                        Stock = item.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                    bill.Total += item.Quantity;
                }
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm mặt hàng thức ăn vào hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddMedicineItemToBill(Guid billId, AddMedicineItemToBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.MedicineItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thuốc.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
            if (existingItems.Any() && !existingItems.All(x => x.MedicineId.HasValue))
                return (false, "Hóa đơn chỉ được chứa mặt hàng thuốc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            foreach (var item in request.MedicineItems)
            {
                var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, true, false, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            try
            {
                foreach (var item in request.MedicineItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = null,
                        MedicineId = item.ItemId,
                        BreedId = null,
                        Stock = item.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                    bill.Total += item.Quantity;
                }
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm mặt hàng thuốc vào hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddBreedItemToBill(Guid billId, AddBreedItemToBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.BreedItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng giống.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
            if (existingItems.Any() && !existingItems.All(x => x.BreedId.HasValue))
                return (false, "Hóa đơn chỉ được chứa mặt hàng giống.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            foreach (var item in request.BreedItems)
            {
                var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, false, true, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            try
            {
                foreach (var item in request.BreedItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = null,
                        MedicineId = null,
                        BreedId = item.ItemId,
                        Stock = item.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                    bill.Total += item.Quantity;
                }
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi thêm mặt hàng giống vào hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateFoodItemInBill(Guid billId, Guid itemId, UpdateFoodItemInBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (request.FoodItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng thức ăn.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.FoodId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thức ăn.");

            var item = request.FoodItems.First();
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, true, false, false, cancellationToken);
            if (!isValid) return (false, errorMessage);

            try
            {
                bill.Total -= billItem.Stock;
                billItem.FoodId = item.ItemId;
                billItem.Stock = item.Quantity;
                bill.Total += item.Quantity;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật mặt hàng thức ăn trong hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateMedicineItemInBill(Guid billId, Guid itemId, UpdateMedicineItemInBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (request.MedicineItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng thuốc.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.MedicineId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thuốc.");

            var item = request.MedicineItems.First();
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, true, false, cancellationToken);
            if (!isValid) return (false, errorMessage);

            try
            {
                bill.Total -= billItem.Stock;
                billItem.MedicineId = item.ItemId;
                billItem.Stock = item.Quantity;
                bill.Total += item.Quantity;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật mặt hàng thuốc trong hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBreedItemInBill(Guid billId, Guid itemId, UpdateBreedItemInBillDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (request.BreedItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng giống.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.BreedId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng giống.");

            var item = request.BreedItems.First();
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, false, true, cancellationToken);
            if (!isValid) return (false, errorMessage);

            try
            {
                bill.Total -= billItem.Stock;
                billItem.BreedId = item.ItemId;
                billItem.Stock = item.Quantity;
                bill.Total += item.Quantity;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật mặt hàng giống trong hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteFoodItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.FoodId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thức ăn.");

            try
            {
                bill.Total -= billItem.Stock;
                billItem.IsActive = false;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa mặt hàng thức ăn khỏi hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteMedicineItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.MedicineId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thuốc.");

            try
            {
                bill.Total -= billItem.Stock;
                billItem.IsActive = false;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa mặt hàng thuốc khỏi hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteBreedItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var billItem = await _billItemRepository.GetById(itemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
            if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
            if (!billItem.BreedId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng giống.");

            try
            {
                bill.Total -= billItem.Stock;
                billItem.IsActive = false;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa mặt hàng giống khỏi hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestFood(CreateFoodRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.FoodItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thức ăn.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            foreach (var foodItem in request.FoodItems)
            {
                var (isValid, errorMessage) = await ValidateItem(foodItem.ItemId, foodItem.Quantity, true, false, false, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            var bill = new Bill
            {
                UserRequestId =request.UserRequestId, 
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Yêu cầu thức ăn - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.FoodItems.Sum(x => x.Quantity),
                Weight = 0
            };

            try
            {
                _billRepository.Insert(bill);
                foreach (var foodItem in request.FoodItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = foodItem.ItemId,
                        MedicineId = null,
                        BreedId = null,
                        Stock = foodItem.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                }
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo yêu cầu thức ăn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestMedicine(CreateMedicineRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.MedicineItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thuốc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            foreach (var medicineItem in request.MedicineItems)
            {
                var (isValid, errorMessage) = await ValidateItem(medicineItem.ItemId, medicineItem.Quantity, false, true, false, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            var bill = new Bill
            {
                UserRequestId =request.UserRequestId, 
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Yêu cầu thuốc - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.MedicineItems.Sum(x => x.Quantity),
                Weight = 0
            };

            try
            {
                _billRepository.Insert(bill);
                foreach (var medicineItem in request.MedicineItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = null,
                        MedicineId = medicineItem.ItemId,
                        BreedId = null,
                        Stock = medicineItem.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                }
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo yêu cầu thuốc: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestBreed(CreateBreedRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.BreedItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng giống.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            foreach (var breedItem in request.BreedItems)
            {
                var (isValid, errorMessage) = await ValidateItem(breedItem.ItemId, breedItem.Quantity, false, false, true, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            var bill = new Bill
            {
                UserRequestId =request.UserRequestId, 
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Yêu cầu giống - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.BreedItems.Sum(x => x.Quantity),
                Weight = 0
            };

            try
            {
                _billRepository.Insert(bill);
                foreach (var breedItem in request.BreedItems)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = null,
                        MedicineId = null,
                        BreedId = breedItem.ItemId,
                        Stock = breedItem.Quantity
                    };
                    _billItemRepository.Insert(billItem);
                }
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo yêu cầu giống: {ex.Message}");
            }
        }
    }
}
