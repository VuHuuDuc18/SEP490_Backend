using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Extensions;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Domain.Helper.Constants;
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

        private async Task<(bool Success, string ErrorMessage)> ValidateItem(CreateBillItemRequest item, CancellationToken cancellationToken)
        {
            var checkError = new Ref<CheckError>();
            if (item.FoodId.HasValue)
            {
                var food = await _foodRepository.GetById(item.FoodId.Value, checkError);
                return food != null && food.IsActive && food.Stock >= item.Stock
                    ? (true, null)
                    : (false, $"Food with ID {item.FoodId} not found, inactive, or insufficient stock.");
            }
            else if (item.MedicineId.HasValue)
            {
                var medicine = await _medicineRepository.GetById(item.MedicineId.Value, checkError);
                return medicine != null && medicine.IsActive && medicine.Stock >= item.Stock
                    ? (true, null)
                    : (false, $"Medicine with ID {item.MedicineId} not found, inactive, or insufficient stock.");
            }
            else if (item.BreedId.HasValue)
            {
                var breed = await _breedRepository.GetById(item.BreedId.Value, checkError);
                return breed != null && breed.IsActive && breed.Stock >= item.Stock
                    ? (true, null)
                    : (false, $"Breed with ID {item.BreedId} not found, inactive, or insufficient stock.");
            }
            return (false, "Each bill item must have exactly one of FoodId, MedicineId, or BreedId.");
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

        public async Task<(bool Success, string ErrorMessage)> CreateBill(CreateBillRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Bill data cannot be null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            if (request.Items.Any(item => !item.IsValidItem()))
                return (false, "Each bill item must have exactly one of FoodId, MedicineId, or BreedId.");

            var checkError = new Ref<CheckError>();
            var user = await _userRepository.GetById(request.UserRequestId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error checking user: {checkError.Value.Message}");
            if (user == null || !user.IsActive) return (false, "User not found or inactive.");

            var livestockCircle = await _livestockCircleRepository.GetById(request.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error checking livestock circle: {checkError.Value.Message}");
            if (livestockCircle == null || !livestockCircle.IsActive) return (false, "Livestock circle not found or inactive.");

            foreach (var item in request.Items)
            {
                var (isValid, errorMessage) = await ValidateItem(item, cancellationToken);
                if (!isValid) return (false, errorMessage);
            }

            var bill = new Bill
            {
                UserRequestId = request.UserRequestId,
                LivestockCircleId = request.LivestockCircleId,
                Name = request.Name,
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.Items.Sum(i => i.Stock),
                Weight = request.Weight
            };

            try
            {
                _billRepository.Insert(bill);
                foreach (var item in request.Items)
                {
                    var billItem = new BillItem
                    {
                        BillId = bill.Id,
                        FoodId = item.FoodId,
                        MedicineId = item.MedicineId,
                        BreedId = item.BreedId,
                        Stock = item.Stock
                    };
                    _billItemRepository.Insert(billItem);
                }
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating bill: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var billItem = await _billItemRepository.GetById(billItemId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill item: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Bill item not found or inactive.");

            var bill = await _billRepository.GetById(billItem.BillId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive.");

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
                return (false, $"Error disabling bill item: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive.");

            try
            {
                bill.IsActive = false;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error disabling bill: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(Guid billId, ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Request cannot be null.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0.");

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}");

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true) return (null, $"Error retrieving bill: {checkError.Value.Message}");
                if (bill == null) return (null, "Bill not found.");

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
                return (null, $"Error retrieving bill items: {ex.Message}");
            }
        }

        public async Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (null, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null) return (null, "Bill not found.");

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
                if (request == null) return (null, "Request cannot be null.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}");

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
                return (null, $"Error retrieving bill list: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBill(Guid billId, UpdateBillRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Bill data cannot be null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive.");

            if (request.ItemOperations != null)
            {
                foreach (var op in request.ItemOperations)
                {
                    if (string.IsNullOrEmpty(op.OperationType))
                        return (false, "Operation type cannot be empty.");

                    if (!new[] { "Add", "Update", "Remove" }.Contains(op.OperationType, StringComparer.OrdinalIgnoreCase))
                        return (false, $"Invalid operation type: {op.OperationType}. Must be 'Add', 'Update', or 'Remove'.");

                    if ((op.OperationType.Equals("Add", StringComparison.OrdinalIgnoreCase) || op.OperationType.Equals("Update", StringComparison.OrdinalIgnoreCase)) && op.ItemData == null)
                        return (false, $"Item data is required for {op.OperationType} operation.");
                    if (!op.ItemData.IsValidItem())
                        return (false, $"Bill item for {op.OperationType} operation must have exactly one of FoodId, MedicineId, or BreedId.");

                    if (op.OperationType.Equals("Update", StringComparison.OrdinalIgnoreCase) && !op.ItemId.HasValue)
                        return (false, "ItemId is required for Update operation.");
                    if (op.OperationType.Equals("Remove", StringComparison.OrdinalIgnoreCase) && !op.ItemId.HasValue)
                        return (false, "ItemId is required for Remove operation.");

                    var (isValid, errorMessage) = await ValidateItem(op.ItemData, cancellationToken);
                    if (!isValid) return (false, errorMessage);
                }
            }

            try
            {
                bill.Name = request.Name;
                bill.Note = request.Note;
                bill.Weight = request.Weight;

                if (request.ItemOperations != null && request.ItemOperations.Any())
                {
                    var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive)
                        .ToListAsync(cancellationToken);

                    foreach (var op in request.ItemOperations)
                    {
                        if (op.OperationType.Equals("Add", StringComparison.OrdinalIgnoreCase))
                        {
                            var newItem = new BillItem
                            {
                                BillId = bill.Id,
                                FoodId = op.ItemData.FoodId,
                                MedicineId = op.ItemData.MedicineId,
                                BreedId = op.ItemData.BreedId,
                                Stock = op.ItemData.Stock
                            };
                            _billItemRepository.Insert(newItem);
                        }
                        else if (op.OperationType.Equals("Update", StringComparison.OrdinalIgnoreCase))
                        {
                            var item = existingItems.FirstOrDefault(x => x.Id == op.ItemId.Value);
                            if (item == null || !item.IsActive) return (false, $"Bill item with ID {op.ItemId} not found or inactive.");

                            item.FoodId = op.ItemData.FoodId;
                            item.MedicineId = op.ItemData.MedicineId;
                            item.BreedId = op.ItemData.BreedId;
                            item.Stock = op.ItemData.Stock;
                            _billItemRepository.Update(item);
                        }
                        else if (op.OperationType.Equals("Remove", StringComparison.OrdinalIgnoreCase))
                        {
                            var item = existingItems.FirstOrDefault(x => x.Id == op.ItemId.Value);
                            if (item == null || !item.IsActive) return (false, $"Bill item with ID {op.ItemId} not found or inactive.");

                            item.IsActive = false;
                            _billItemRepository.Update(item);
                        }
                    }
                }

                var activeItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive)
                    .ToListAsync(cancellationToken);
                bill.Total = activeItems.Sum(x => x.Stock);

                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating bill: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(ListingRequest request, string itemType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Request cannot be null.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}");

                if (!new[] { "Food", "Medicine", "Breed" }.Contains(itemType, StringComparer.OrdinalIgnoreCase))
                    return (null, $"Invalid item type: {itemType}. Must be 'Food', 'Medicine', or 'Breed'.");

                var billItemQuery = itemType.ToLower() switch
                {
                    "food" => _billItemRepository.GetQueryable(x => x.IsActive && x.FoodId.HasValue && !x.MedicineId.HasValue && !x.BreedId.HasValue),
                    "medicine" => _billItemRepository.GetQueryable(x => x.IsActive && x.MedicineId.HasValue && !x.FoodId.HasValue && !x.BreedId.HasValue),
                    "breed" => _billItemRepository.GetQueryable(x => x.IsActive && x.BreedId.HasValue && !x.FoodId.HasValue && !x.MedicineId.HasValue),
                    _ => throw new InvalidOperationException("Unsupported item type.")
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
                return (null, $"Error retrieving bills by {itemType.ToLower()}: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                var validStatuses = new[] { StatusConstant.REQUESTED, StatusConstant.APPROVED, StatusConstant.CONFIRMED, StatusConstant.REJECTED, StatusConstant.COMPLETED, StatusConstant.CANCELLED };
                if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                    return (false, $"Invalid status: {newStatus}. Must be one of: {string.Join(", ", validStatuses)}.");

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
                if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive.");

                if (bill.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                    return (false, $"Bill is already in status {newStatus}.");

                try
                {
                    bill.Status = newStatus;
                  //  bill.UpdatedAt = DateTime.UtcNow;

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
                                await UpdateLivestockCircle(bill.LivestockCircleId, item.Stock, 0, null, cancellationToken); // Cần thêm DeadUnit và AverageWeight nếu có
                            }
                        }
                    }

                    _billRepository.Update(bill);
                    await _billRepository.CommitAsync(cancellationToken);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, $"Error updating bill status: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error changing bill status: {ex.Message}");
            }
        }
    }
}