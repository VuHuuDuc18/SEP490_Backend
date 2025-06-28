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
using Domain.Dto.Request.Bill.Admin;

namespace Infrastructure.Services.Implements
{
    public class BillService : IBillService
    {
        private readonly IRepository<Bill> _billRepository; // Repository để thao tác với bảng Bill
        private readonly IRepository<BillItem> _billItemRepository; // Repository để thao tác với bảng BillItem
        private readonly IRepository<User> _userRepository; // Repository để thao tác với bảng User
        private readonly IRepository<LivestockCircle> _livestockCircleRepository; // Repository để thao tác với bảng LivestockCircle
        private readonly IRepository<Food> _foodRepository; // Repository để thao tác với bảng Food
        private readonly IRepository<Medicine> _medicineRepository; // Repository để thao tác với bảng Medicine
        private readonly IRepository<Breed> _breedRepository; // Repository để thao tác với bảng Breed
        private readonly IRepository<LivestockCircleFood> _livestockCircleFoodRepository; // Repository để thao tác với bảng LivestockCircleFood
        private readonly IRepository<LivestockCircleMedicine> _livestockCircleMedicineRepository; // Repository để thao tác với bảng LivestockCircleMedicine

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
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository)); // Kiểm tra null cho billRepository
            _billItemRepository = billItemRepository ?? throw new ArgumentNullException(nameof(billItemRepository)); // Kiểm tra null cho billItemRepository
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository)); // Kiểm tra null cho userRepository
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository)); // Kiểm tra null cho livestockCircleRepository
            _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository)); // Kiểm tra null cho foodRepository
            _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository)); // Kiểm tra null cho medicineRepository
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository)); // Kiểm tra null cho breedRepository
            _livestockCircleFoodRepository = livestockCircleFoodRepository ?? throw new ArgumentNullException(nameof(livestockCircleFoodRepository)); // Kiểm tra null cho livestockCircleFoodRepository
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository ?? throw new ArgumentNullException(nameof(livestockCircleMedicineRepository)); // Kiểm tra null cho livestockCircleMedicineRepository
        }

        private async Task<(bool Success, string ErrorMessage)> ValidateItem(CreateBillItemRequest item, CancellationToken cancellationToken)
        {
            // Hàm kiểm tra tính hợp lệ của item (Food, Medicine, hoặc Breed)
            var checkError = new Ref<CheckError>();
            if (item.FoodId.HasValue)
            {
                var food = await _foodRepository.GetById(item.FoodId.Value, checkError); // Lấy thông tin Food theo ID
                return food != null && food.IsActive && food.Stock >= item.Stock
                    ? (true, null) // Trả về thành công nếu Food tồn tại, active, và đủ stock
                    : (false, $"Food with ID {item.FoodId} not found, inactive, or insufficient stock."); // Trả về lỗi nếu không hợp lệ
            }
            else if (item.MedicineId.HasValue)
            {
                var medicine = await _medicineRepository.GetById(item.MedicineId.Value, checkError); // Lấy thông tin Medicine theo ID
                return medicine != null && medicine.IsActive && medicine.Stock >= item.Stock
                    ? (true, null) // Trả về thành công nếu Medicine tồn tại, active, và đủ stock
                    : (false, $"Medicine with ID {item.MedicineId} not found, inactive, or insufficient stock."); // Trả về lỗi nếu không hợp lệ
            }
            else if (item.BreedId.HasValue)
            {
                var breed = await _breedRepository.GetById(item.BreedId.Value, checkError); // Lấy thông tin Breed theo ID
                return breed != null && breed.IsActive && breed.Stock >= item.Stock
                    ? (true, null) // Trả về thành công nếu Breed tồn tại, active, và đủ stock
                    : (false, $"Breed with ID {item.BreedId} not found, inactive, or insufficient stock."); // Trả về lỗi nếu không hợp lệ
            }
            return (false, "Each bill item must have exactly one of FoodId, MedicineId, or BreedId."); // Trả về lỗi nếu không có ID hợp lệ
        }

        private async Task UpdateStock(BillItem billItem, int quantity, CancellationToken cancellationToken)
        {
            // Hàm cập nhật tồn kho (giảm stock) cho Food, Medicine, hoặc Breed
            var checkError = new Ref<CheckError>();
            if (billItem.FoodId.HasValue)
            {
                var food = await _foodRepository.GetById(billItem.FoodId.Value, checkError); // Lấy Food theo ID
                if (food != null) { food.Stock -= quantity; _foodRepository.Update(food); } // Giảm stock và cập nhật
            }
            else if (billItem.MedicineId.HasValue)
            {
                var medicine = await _medicineRepository.GetById(billItem.MedicineId.Value, checkError); // Lấy Medicine theo ID
                if (medicine != null) { medicine.Stock -= quantity; _medicineRepository.Update(medicine); } // Giảm stock và cập nhật
            }
            else if (billItem.BreedId.HasValue)
            {
                var breed = await _breedRepository.GetById(billItem.BreedId.Value, checkError); // Lấy Breed theo ID
                if (breed != null) { breed.Stock -= quantity; _breedRepository.Update(breed); } // Giảm stock và cập nhật
            }
            await Task.CompletedTask; // Đánh dấu task hoàn thành (không có hành động bất đồng bộ khác)
        }

        private async Task UpdateLivestockCircle(Guid livestockCircleId, int quantity, int? deadUnit, float? averageWeight, CancellationToken cancellationToken)
        {
            // Hàm cập nhật thông tin LivestockCircle (tổng số lượng, ngày bắt đầu, trọng lượng trung bình)
            var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, new Ref<CheckError>());
            if (livestockCircle != null)
            {
                livestockCircle.TotalUnit = (quantity - (deadUnit ?? 0)); // Cập nhật tổng số lượng, trừ đi số lượng chết
                livestockCircle.StartDate = DateTime.UtcNow; // Cập nhật ngày bắt đầu
                livestockCircle.AverageWeight = averageWeight ?? livestockCircle.AverageWeight; // Cập nhật trọng lượng trung bình nếu có
                _livestockCircleRepository.Update(livestockCircle); 
            }
            await Task.CompletedTask; 
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default)
        {
            // Hàm vô hiệu hóa một item trong hóa đơn
            var checkError = new Ref<CheckError>();
            var billItem = await _billItemRepository.GetById(billItemId, checkError); // Lấy thông tin item
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill item: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Bill item not found or inactive."); // Kiểm tra item tồn tại và active

            var bill = await _billRepository.GetById(billItem.BillId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

            try
            {
                billItem.IsActive = false; // Đặt trạng thái item thành inactive
                bill.Total -= billItem.Stock; // Cập nhật tổng số lượng hóa đơn
                _billItemRepository.Update(billItem); // Cập nhật item
                _billRepository.Update(bill); // Cập nhật hóa đơn
                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error disabling bill item: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default)
        {
            // Hàm vô hiệu hóa một hóa đơn
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

            try
            {
                bill.IsActive = false; // Đặt trạng thái hóa đơn thành inactive
                _billRepository.Update(bill); // Cập nhật hóa đơn
                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error disabling bill: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(Guid billId, ListingRequest request, CancellationToken cancellationToken = default)
        {
            // Hàm lấy danh sách item của một hóa đơn theo phân trang
            try
            {
                if (request == null) return (null, "Request cannot be null."); // Kiểm tra null cho request
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0."); // Kiểm tra giá trị phân trang

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}"); // Kiểm tra trường filter hợp lệ

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
                if (checkError.Value?.IsError == true) return (null, $"Error retrieving bill: {checkError.Value.Message}");
                if (bill == null) return (null, "Bill not found."); // Kiểm tra hóa đơn tồn tại

                var query = _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive); // Lấy query cho các item active

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString); // Áp dụng tìm kiếm
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter); // Áp dụng filter

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort); // Phân trang kết quả

                var responses = paginationResult.Items.Select(i => new BillItemResponse
                {
                    Id = i.Id,
                    BillId = i.BillId,
                    FoodId = i.FoodId,
                    MedicineId = i.MedicineId,
                    BreedId = i.BreedId,
                    Stock = i.Stock,
                    IsActive = i.IsActive
                }).ToList(); // Chuyển đổi sang danh sách response

                var result = new PaginationSet<BillItemResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                }; // Tạo đối tượng phân trang

                return (result, null); // Trả về kết quả
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving bill items: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId, CancellationToken cancellationToken = default)
        {
            // Hàm lấy thông tin chi tiết của một hóa đơn
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (null, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null) return (null, "Bill not found."); // Kiểm tra hóa đơn tồn tại

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
            }; // Chuyển đổi sang response

            return (response, null); // Trả về kết quả
        }

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            // Hàm lấy danh sách hóa đơn theo phân trang
            try
            {
                if (request == null) return (null, "Request cannot be null."); // Kiểm tra null cho request
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0."); // Kiểm tra giá trị phân trang

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}"); // Kiểm tra trường filter hợp lệ

                var query = _billRepository.GetQueryable(x => x.IsActive); // Lấy query cho các hóa đơn active

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString); // Áp dụng tìm kiếm
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter); // Áp dụng filter

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort); // Phân trang kết quả

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
                }).ToList(); // Chuyển đổi sang danh sách response

                var result = new PaginationSet<BillResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                }; // Tạo đối tượng phân trang

                return (result, null); // Trả về kết quả
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving bill list: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddItemToBill(Guid billId, CreateBillItemRequest item, CancellationToken cancellationToken = default)
        {
            // Hàm thêm một item mới vào hóa đơn
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(item);
            if (!Validator.TryValidateObject(item, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage))); // Kiểm tra validation

            var (isValid, errorMessage) = await ValidateItem(item, cancellationToken); // Kiểm tra tính hợp lệ của item
            if (!isValid) return (false, errorMessage);

            try
            {
                var billItem = new BillItem
                {
                    BillId = bill.Id, // Gán ID hóa đơn
                    FoodId = item.FoodId, // Gán ID Food nếu có
                    MedicineId = item.MedicineId, // Gán ID Medicine nếu có
                    BreedId = item.BreedId, // Gán ID Breed nếu có
                    Stock = item.Stock // Gán số lượng
                };
                _billItemRepository.Insert(billItem); // Thêm item vào database

                bill.Total += item.Stock; // Cập nhật tổng số lượng hóa đơn
                _billRepository.Update(bill); // Cập nhật hóa đơn

                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error adding item to bill: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateItemInBill(Guid billId, Guid itemId, CreateBillItemRequest item, CancellationToken cancellationToken = default)
        {
            // Hàm cập nhật thông tin một item trong hóa đơn
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

            var billItem = await _billItemRepository.GetById(itemId, checkError); // Lấy thông tin item
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill item: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Bill item not found or inactive."); // Kiểm tra item tồn tại và active
            if (billItem.BillId != billId) return (false, "Bill item does not belong to the specified bill."); // Kiểm tra item thuộc hóa đơn

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(item);
            if (!Validator.TryValidateObject(item, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage))); // Kiểm tra validation

            var (isValid, errorMessage) = await ValidateItem(item, cancellationToken); // Kiểm tra tính hợp lệ của item
            if (!isValid) return (false, errorMessage);

            try
            {
                bill.Total -= billItem.Stock; // Giảm số lượng cũ khỏi tổng
                billItem.FoodId = item.FoodId; // Cập nhật ID Food nếu có
                billItem.MedicineId = item.MedicineId; // Cập nhật ID Medicine nếu có
                billItem.BreedId = item.BreedId; // Cập nhật ID Breed nếu có
                billItem.Stock = item.Stock; // Cập nhật số lượng
                bill.Total += item.Stock; // Cập nhật tổng số lượng mới

                _billItemRepository.Update(billItem); // Cập nhật item
                _billRepository.Update(bill); // Cập nhật hóa đơn

                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error updating item in bill: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        {
            // Hàm xóa một item khỏi hóa đơn
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

            var billItem = await _billItemRepository.GetById(itemId, checkError); // Lấy thông tin item
            if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill item: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive) return (false, "Bill item not found or inactive."); // Kiểm tra item tồn tại và active
            if (billItem.BillId != billId) return (false, "Bill item does not belong to the specified bill."); // Kiểm tra item thuộc hóa đơn

            try
            {
                bill.Total -= billItem.Stock; // Giảm số lượng khỏi tổng
                billItem.IsActive = false; // Đặt trạng thái item thành inactive
                _billItemRepository.Update(billItem); // Cập nhật item
                _billRepository.Update(bill); // Cập nhật hóa đơn

                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting item from bill: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(ListingRequest request, string itemType, CancellationToken cancellationToken = default)
        {
            // Hàm lấy danh sách hóa đơn theo loại item (Food, Medicine, Breed) theo phân trang
            try
            {
                if (request == null) return (null, "Request cannot be null."); // Kiểm tra null cho request
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex and PageSize must be greater than 0."); // Kiểm tra giá trị phân trang

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Invalid filter fields: {string.Join(", ", invalidFields)}"); // Kiểm tra trường filter hợp lệ

                if (!new[] { "Food", "Medicine", "Breed" }.Contains(itemType, StringComparer.OrdinalIgnoreCase))
                    return (null, $"Invalid item type: {itemType}. Must be 'Food', 'Medicine', or 'Breed'."); // Kiểm tra loại item hợp lệ

                var billItemQuery = itemType.ToLower() switch
                {
                    "food" => _billItemRepository.GetQueryable(x => x.IsActive && x.FoodId.HasValue && !x.MedicineId.HasValue && !x.BreedId.HasValue),
                    "medicine" => _billItemRepository.GetQueryable(x => x.IsActive && x.MedicineId.HasValue && !x.FoodId.HasValue && !x.BreedId.HasValue),
                    "breed" => _billItemRepository.GetQueryable(x => x.IsActive && x.BreedId.HasValue && !x.FoodId.HasValue && !x.MedicineId.HasValue),
                    _ => throw new InvalidOperationException("Unsupported item type.") // Xử lý trường hợp không hỗ trợ
                }; // Lấy query cho các item theo loại

                var billIds = await billItemQuery.Select(x => x.BillId).Distinct().ToListAsync(cancellationToken); // Lấy danh sách ID hóa đơn
                var query = _billRepository.GetQueryable(x => x.IsActive && billIds.Contains(x.Id)); // Lấy query cho hóa đơn

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString); // Áp dụng tìm kiếm
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter); // Áp dụng filter

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort); // Phân trang kết quả

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
                }).ToList(); // Chuyển đổi sang danh sách response

                var result = new PaginationSet<BillResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                }; // Tạo đối tượng phân trang

                return (result, null); // Trả về kết quả
            }
            catch (Exception ex)
            {
                return (null, $"Error retrieving bills by {itemType.ToLower()}: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }


        public async Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default)
        {
            // Hàm thay đổi trạng thái của hóa đơn
            try
            {
                var validStatuses = new[] { StatusConstant.REQUESTED, StatusConstant.APPROVED, StatusConstant.CONFIRMED, StatusConstant.REJECTED, StatusConstant.COMPLETED, StatusConstant.CANCELLED };
                if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                    return (false, $"Invalid status: {newStatus}. Must be one of: {string.Join(", ", validStatuses)}."); // Kiểm tra trạng thái hợp lệ

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError); // Lấy thông tin hóa đơn
                if (checkError.Value?.IsError == true) return (false, $"Error retrieving bill: {checkError.Value.Message}");
                if (bill == null || !bill.IsActive) return (false, "Bill not found or inactive."); // Kiểm tra hóa đơn tồn tại và active

                if (bill.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                    return (false, $"Bill is already in status {newStatus}."); // Kiểm tra trạng thái đã đúng chưa

                try
                {
                    bill.Status = newStatus;

                    bill.Status = newStatus; // Cập nhật trạng thái
            

                    if (newStatus == StatusConstant.APPROVED)
                    {
                        var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken); // Lấy danh sách item active
                        foreach (var item in billItems)
                        {
                            await UpdateStock(item, item.Stock, cancellationToken); // Cập nhật tồn kho khi duyệt
                        }
                    }
                    else if (newStatus == StatusConstant.CONFIRMED)
                    {
                        var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken); // Lấy danh sách item active
                        foreach (var item in billItems)
                        {
                            if (item.FoodId.HasValue)
                            {
                                var lcFood = await _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.FoodId == item.FoodId).FirstOrDefaultAsync(cancellationToken); // Lấy thông tin LivestockCircleFood
                                if (lcFood == null)
                                {
                                    lcFood = new LivestockCircleFood { LivestockCircleId = bill.LivestockCircleId, FoodId = item.FoodId.Value, Remaining = 0 }; // Tạo mới nếu không tồn tại
                                    _livestockCircleFoodRepository.Insert(lcFood);
                                }
                                lcFood.Remaining += item.Stock; // Cập nhật số lượng còn lại
                                _livestockCircleFoodRepository.Update(lcFood); // Lưu thay đổi
                            }
                            else if (item.MedicineId.HasValue)
                            {
                                var lcMedicine = await _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.MedicineId == item.MedicineId).FirstOrDefaultAsync(cancellationToken); // Lấy thông tin LivestockCircleMedicine
                                if (lcMedicine == null)
                                {
                                    lcMedicine = new LivestockCircleMedicine { LivestockCircleId = bill.LivestockCircleId, MedicineId = item.MedicineId.Value, Remaining = 0 }; // Tạo mới nếu không tồn tại
                                    _livestockCircleMedicineRepository.Insert(lcMedicine);
                                }
                                lcMedicine.Remaining += item.Stock; // Cập nhật số lượng còn lại
                                _livestockCircleMedicineRepository.Update(lcMedicine); // Lưu thay đổi
                            }
                            else if (item.BreedId.HasValue)
                            {
                                await UpdateLivestockCircle(bill.LivestockCircleId, item.Stock, 0, null, cancellationToken); // Cập nhật LivestockCircle cho Breed (cần bổ sung DeadUnit và AverageWeight nếu có)
                            }
                        }
                    }

                    _billRepository.Update(bill); // Cập nhật hóa đơn
                    await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                    return (true, null); // Trả về thành công
                }
                catch (Exception ex)
                {
                    return (false, $"Error updating bill status: {ex.Message}"); // Xử lý lỗi nếu có
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error changing bill status: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestFood(CreateRequestDto request, CancellationToken cancellationToken = default)
        {
            // Hàm tạo yêu cầu cho Food
            if (request == null || request.ItemType != "Food")
                return (false, "Request data is required and ItemType must be 'Food'."); // Kiểm tra request và loại item

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage))); // Kiểm tra validation

            var checkError = new Ref<CheckError>();
            var food = await _foodRepository.GetById(request.ItemId, checkError); // Lấy thông tin Food
            if (checkError.Value?.IsError == true) return (false, $"Error checking food: {checkError.Value.Message}");
            if (food == null || !food.IsActive || food.Stock < request.Quantity)
                return (false, "Food not found, inactive, or insufficient stock."); // Kiểm tra Food tồn tại, active, và đủ stock

            var bill = new Bill
            {
                UserRequestId = Guid.Empty, // Cần cung cấp UserRequestId thực tế
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Request Food - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.Quantity,
                Weight = 0
            }; // Tạo hóa đơn mới

            try
            {
                _billRepository.Insert(bill); // Thêm hóa đơn
                var billItem = new BillItem
                {
                    BillId = bill.Id,
                    FoodId = request.ItemId,
                    MedicineId = null,
                    BreedId = null,
                    Stock = request.Quantity
                }; // Tạo item cho Food
                _billItemRepository.Insert(billItem); // Thêm item
                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error creating food request: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestMedicine(CreateRequestDto request, CancellationToken cancellationToken = default)
        {
            // Hàm tạo yêu cầu cho Medicine
            if (request == null || request.ItemType != "Medicine")
                return (false, "Request data is required and ItemType must be 'Medicine'."); // Kiểm tra request và loại item

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage))); // Kiểm tra validation

            var checkError = new Ref<CheckError>();
            var medicine = await _medicineRepository.GetById(request.ItemId, checkError); // Lấy thông tin Medicine
            if (checkError.Value?.IsError == true) return (false, $"Error checking medicine: {checkError.Value.Message}");
            if (medicine == null || !medicine.IsActive || medicine.Stock < request.Quantity)
                return (false, "Medicine not found, inactive, or insufficient stock."); // Kiểm tra Medicine tồn tại, active, và đủ stock

            var bill = new Bill
            {
                UserRequestId = Guid.Empty, // Cần cung cấp UserRequestId thực tế
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Request Medicine - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.Quantity,
                Weight = 0
            }; // Tạo hóa đơn mới

            try
            {
                _billRepository.Insert(bill); // Thêm hóa đơn
                var billItem = new BillItem
                {
                    BillId = bill.Id,
                    FoodId = null,
                    MedicineId = request.ItemId,
                    BreedId = null,
                    Stock = request.Quantity
                }; // Tạo item cho Medicine
                _billItemRepository.Insert(billItem); // Thêm item
                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error creating medicine request: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RequestBreed(CreateRequestDto request, CancellationToken cancellationToken = default)
        {
            // Hàm tạo yêu cầu cho Breed
            if (request == null || request.ItemType != "Breed")
                return (false, "Request data is required and ItemType must be 'Breed'."); // Kiểm tra request và loại item

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage))); // Kiểm tra validation

            var checkError = new Ref<CheckError>();
            var breed = await _breedRepository.GetById(request.ItemId, checkError); // Lấy thông tin Breed
            if (checkError.Value?.IsError == true) return (false, $"Error checking breed: {checkError.Value.Message}");
            if (breed == null || !breed.IsActive || breed.Stock < request.Quantity)
                return (false, "Breed not found, inactive, or insufficient stock."); // Kiểm tra Breed tồn tại, active, và đủ stock

            var bill = new Bill
            {
                UserRequestId = Guid.Empty, // Cần cung cấp UserRequestId thực tế
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Request Breed - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                Total = request.Quantity,
                Weight = 0
            }; // Tạo hóa đơn mới

            try
            {
                _billRepository.Insert(bill); // Thêm hóa đơn
                var billItem = new BillItem
                {
                    BillId = bill.Id,
                    FoodId = null,
                    MedicineId = null,
                    BreedId = request.ItemId,
                    Stock = request.Quantity
                }; // Tạo item cho Breed
                _billItemRepository.Insert(billItem); // Thêm item
                await _billRepository.CommitAsync(cancellationToken); // Lưu thay đổi
                return (true, null); // Trả về thành công
            }
            catch (Exception ex)
            {
                return (false, $"Error creating breed request: {ex.Message}"); // Xử lý lỗi nếu có
            }
        }

        public async Task<bool> AdminUpdateBill(Admin_UpdateBarnRequest request)
        {
            try
            {
                var BillToUpdate = await _billRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(it => it.LivestockCircleId == request.LivestockCicleId);
                if (BillToUpdate == null || !BillToUpdate.Status.Equals(StatusConstant.REQUESTED))
                {
                    return false;
                }
                var UpdatedBreed = await _billItemRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(it => it.BillId == BillToUpdate.Id);
                UpdatedBreed.BreedId = request.BreedId;
                UpdatedBreed.Stock = request.Stock;

                _billItemRepository.Update(UpdatedBreed);
                await _billItemRepository.CommitAsync();

                // cap nhat livestockCircle
                var UpdatedLivestockCircle = await _livestockCircleRepository.GetById(request.LivestockCicleId);
                UpdatedLivestockCircle.BreedId = request.BreedId;

                _livestockCircleRepository.Update(UpdatedLivestockCircle);
                await _livestockCircleRepository.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}