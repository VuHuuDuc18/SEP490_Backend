﻿using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response;
using Domain.Dto.Response.Bill;
using Domain.Extensions;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

        public BillService(
            IRepository<Bill> billRepository,
            IRepository<BillItem> billItemRepository,
            IRepository<User> userRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<Food> foodRepository,
            IRepository<Medicine> medicineRepository,
            IRepository<Breed> breedRepository)
        {
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
            _billItemRepository = billItemRepository ?? throw new ArgumentNullException(nameof(billItemRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
            _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
            _breedRepository = breedRepository ?? throw new ArgumentNullException(nameof(breedRepository));
        }

        public async Task<(bool Success, string ErrorMessage)> CreateBill(
            CreateBillRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu hóa đơn không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            // Kiểm tra từng mục hóa đơn
            if (request.Items.Any(item => !item.IsValidItem()))
                return (false, "Mỗi mục hóa đơn phải có chính xác một trong FoodId, MedicineId hoặc BreedId.");

            var checkError = new Ref<CheckError>();

            // Kiểm tra UserRequestId
            var user = await _userRepository.GetById(request.UserRequestId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra người dùng: {checkError.Value.Message}");
            if (user == null || !user.IsActive)
                return (false, "Người dùng không tồn tại hoặc không hoạt động.");

            // Kiểm tra LivestockCircleId
            var livestockCircle = await _livestockCircleRepository.GetById(request.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chu kỳ chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null || !livestockCircle.IsActive)
                return (false, "Chu kỳ chăn nuôi không tồn tại hoặc không hoạt động.");

            // Kiểm tra các mục hóa đơn
            foreach (var item in request.Items)
            {
                if (item.FoodId.HasValue)
                {
                    var food = await _foodRepository.GetById(item.FoodId.Value, checkError);
                    if (checkError.Value?.IsError == true)
                        return (false, $"Lỗi khi kiểm tra thức ăn: {checkError.Value.Message}");
                    if (food == null || !food.IsActive)
                        return (false, $"Thức ăn với ID {item.FoodId} không tồn tại hoặc không hoạt động.");
                }
                else if (item.MedicineId.HasValue)
                {
                    var medicine = await _medicineRepository.GetById(item.MedicineId.Value, checkError);
                    if (checkError.Value?.IsError == true)
                        return (false, $"Lỗi khi kiểm tra thuốc: {checkError.Value.Message}");
                    if (medicine == null || !medicine.IsActive)
                        return (false, $"Thuốc với ID {item.MedicineId} không tồn tại hoặc không hoạt động.");
                }
                else if (item.BreedId.HasValue)
                {
                    var breed = await _breedRepository.GetById(item.BreedId.Value, checkError);
                    if (checkError.Value?.IsError == true)
                        return (false, $"Lỗi khi kiểm tra giống: {checkError.Value.Message}");
                    if (breed == null || !breed.IsActive)
                        return (false, $"Giống với ID {item.BreedId} không tồn tại hoặc không hoạt động.");
                }
            }

            // Tạo Bill
            var bill = new Bill
            {
                UserRequestId = request.UserRequestId,
                LivestockCircleId = request.LivestockCircleId,
                Name = request.Name,
                Note = request.Note,
                Status = "Đang duyệt",
                Total = request.Items.Sum(i => i.Stock),
                Weight = request.Weight
            };

            try
            {
                // Thêm Bill
                _billRepository.Insert(bill);

                // Thêm các BillItem
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
                return (false, $"Lỗi khi tạo hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBillItem(Guid billItemId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var billItem = await _billItemRepository.GetById(billItemId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin mục hóa đơn: {checkError.Value.Message}");
            if (billItem == null || !billItem.IsActive)
                return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");

            var bill = await _billRepository.GetById(billItem.BillId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive)
                return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

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
                return (false, $"Lỗi khi xóa mục hóa đơn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableBill(Guid billId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive)
                return (false, "Hóa đơn không tồn tại hoặc đã bị vô hiệu hóa.");

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

        public async Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillId(
            Guid billId, ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
                if (bill == null)
                    return (null, "Hóa đơn không tồn tại.");

                var query = _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

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
    

        public async Task<(BillResponse Bill, string ErrorMessage)> GetBillById(Guid billId,
            CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
            if (bill == null)
                return (null, "Hóa đơn không tồn tại.");

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

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(
            ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _billRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

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

        public async Task<(bool Success, string ErrorMessage)> UpdateBill(Guid billId,
            UpdateBillRequest request, CancellationToken cancellationToken = default)
        {         
                if (request == null)
                    return (false, "Dữ liệu hóa đơn không được null.");

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                    return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true)
                    return (false, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
                if (bill == null || !bill.IsActive)
                    return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

                // Kiểm tra các thao tác với BillItem
                if (request.ItemOperations != null)
                {
                    foreach (var op in request.ItemOperations)
                    {
                        if (string.IsNullOrEmpty(op.OperationType))
                            return (false, "Loại thao tác không được để trống.");

                        if (!new[] { "Add", "Update", "Remove" }.Contains(op.OperationType, StringComparer.OrdinalIgnoreCase))
                            return (false, $"Loại thao tác không hợp lệ: {op.OperationType}. Phải là 'Add', 'Update', hoặc 'Remove'.");

                        if (op.OperationType.Equals("Add", StringComparison.OrdinalIgnoreCase) || op.OperationType.Equals("Update", StringComparison.OrdinalIgnoreCase))
                        {
                            if (op.ItemData == null)
                                return (false, $"Dữ liệu mục hóa đơn là bắt buộc cho thao tác {op.OperationType}.");
                            if (!op.ItemData.IsValidItem())
                                return (false, $"Mục hóa đơn cho thao tác {op.OperationType} phải có chính xác một trong FoodId, MedicineId hoặc BreedId.");

                            // Kiểm tra tham chiếu Food, Medicine, Breed
                            if (op.ItemData.FoodId.HasValue)
                            {
                                var food = await _foodRepository.GetById(op.ItemData.FoodId.Value, checkError);
                                if (checkError.Value?.IsError == true)
                                    return (false, $"Lỗi khi kiểm tra thức ăn: {checkError.Value.Message}");
                                if (food == null || !food.IsActive)
                                    return (false, $"Thức ăn với ID {op.ItemData.FoodId} không tồn tại hoặc không hoạt động.");
                            }
                            else if (op.ItemData.MedicineId.HasValue)
                            {
                                var medicine = await _medicineRepository.GetById(op.ItemData.MedicineId.Value, checkError);
                                if (checkError.Value?.IsError == true)
                                    return (false, $"Lỗi khi kiểm tra thuốc: {checkError.Value.Message}");
                                if (medicine == null || !medicine.IsActive)
                                    return (false, $"Thuốc với ID {op.ItemData.MedicineId} không tồn tại hoặc không hoạt động.");
                            }
                            else if (op.ItemData.BreedId.HasValue)
                            {
                                var breed = await _breedRepository.GetById(op.ItemData.BreedId.Value, checkError);
                                if (checkError.Value?.IsError == true)
                                    return (false, $"Lỗi khi kiểm tra giống: {checkError.Value.Message}");
                                if (breed == null || !breed.IsActive)
                                    return (false, $"Giống với ID {op.ItemData.BreedId} không tồn tại hoặc không hoạt động.");
                            }

                            if (op.OperationType.Equals("Update", StringComparison.OrdinalIgnoreCase) && !op.ItemId.HasValue)
                                return (false, "ItemId là bắt buộc cho thao tác Update.");
                        }
                        else if (op.OperationType.Equals("Remove", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!op.ItemId.HasValue)
                                return (false, "ItemId là bắt buộc cho thao tác Remove.");
                        }
                    }
                }

                try
                {
                    // Cập nhật Bill
                    bill.Name = request.Name;
                    bill.Note = request.Note;
                    bill.Weight = request.Weight;

                    // Xử lý BillItems
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
                                if (item == null || !item.IsActive)
                                    return (false, $"Mục hóa đơn với ID {op.ItemId} không tồn tại hoặc không hoạt động.");

                                item.FoodId = op.ItemData.FoodId;
                                item.MedicineId = op.ItemData.MedicineId;
                                item.BreedId = op.ItemData.BreedId;
                                item.Stock = op.ItemData.Stock;
                                _billItemRepository.Update(item);
                            }
                            else if (op.OperationType.Equals("Remove", StringComparison.OrdinalIgnoreCase))
                            {
                                var item = existingItems.FirstOrDefault(x => x.Id == op.ItemId.Value);
                                if (item == null || !item.IsActive)
                                    return (false, $"Mục hóa đơn với ID {op.ItemId} không tồn tại hoặc không hoạt động.");

                                item.IsActive = false;
                                _billItemRepository.Update(item);
                            }
                        }
                    }

                    // Cập nhật Total
                    var activeItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive)
                        .ToListAsync(cancellationToken);
                    bill.Total = activeItems.Sum(x => x.Stock);

                    _billRepository.Update(bill);
                    await _billRepository.CommitAsync(cancellationToken);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi khi cập nhật hóa đơn: {ex.Message}");
                }
            }

        // Lấy danh sách hóa đơn chỉ chứa các mục hóa đơn thuộc loại được chỉ định (Food, Medicine hoặc Breed)
        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemType(
            ListingRequest request, string itemType, CancellationToken cancellationToken = default)
        {
            try
            {
                // Kiểm tra request không được null
                if (request == null)
                    return (null, "Yêu cầu không được null.");

                // Kiểm tra PageIndex và PageSize phải lớn hơn 0
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                // Kiểm tra các trường lọc có hợp lệ không
                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                // Kiểm tra loại mục hóa đơn hợp lệ
                if (!new[] { "Food", "Medicine", "Breed" }.Contains(itemType, StringComparer.OrdinalIgnoreCase))
                    return (null, $"Loại mục hóa đơn không hợp lệ: {itemType}. Phải là 'Food', 'Medicine', hoặc 'Breed'.");

                // Xác định điều kiện lọc BillItems dựa trên itemType
                IQueryable<BillItem> billItemQuery = itemType.ToLower() switch
                {
                    "food" => _billItemRepository.GetQueryable(x => x.IsActive && x.FoodId.HasValue && !x.MedicineId.HasValue && !x.BreedId.HasValue),
                    "medicine" => _billItemRepository.GetQueryable(x => x.IsActive && x.MedicineId.HasValue && !x.FoodId.HasValue && !x.BreedId.HasValue),
                    "breed" => _billItemRepository.GetQueryable(x => x.IsActive && x.BreedId.HasValue && !x.FoodId.HasValue && !x.MedicineId.HasValue),
                    _ => throw new InvalidOperationException("Loại mục hóa đơn không được hỗ trợ.")
                };

                // Lấy danh sách BillId chỉ chứa các mục hóa đơn thuộc loại được chỉ định
                var billIds = await billItemQuery
                    .Select(x => x.BillId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                // Lấy các hóa đơn thỏa mãn điều kiện
                var query = _billRepository.GetQueryable(x => x.IsActive && billIds.Contains(x.Id));

                // Áp dụng tìm kiếm nếu có chuỗi tìm kiếm
                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                // Áp dụng bộ lọc nếu có
                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                // Phân trang kết quả
                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                // Chuyển đổi kết quả sang BillResponse
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

                // Tạo đối tượng phân trang trả về
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
                // Trả về lỗi nếu có ngoại lệ xảy ra
                return (null, $"Lỗi khi lấy danh sách hóa đơn chỉ chứa {itemType.ToLower()}: {ex.Message}");
            }
        }

        // Thay đổi trạng thái của hóa đơn
        public async Task<(bool Success, string ErrorMessage)> ChangeBillStatus(
            Guid billId, string newStatus, CancellationToken cancellationToken = default)
        {
            try
            {
                // Kiểm tra trạng thái mới có hợp lệ không
                var validStatuses = new[] { "Đang duyệt", "Đã duyệt", "Hủy", "Hoàn thành" };
                if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                    return (false, $"Trạng thái không hợp lệ: {newStatus}. Trạng thái phải là một trong: {string.Join(", ", validStatuses)}.");

                // Kiểm tra sự tồn tại của hóa đơn
                var checkError = new Ref<CheckError>();
                var bill = await _billRepository.GetById(billId, checkError);
                if (checkError.Value?.IsError == true)
                    return (false, $"Lỗi khi lấy thông tin hóa đơn: {checkError.Value.Message}");
                if (bill == null || !bill.IsActive)
                    return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

                // Kiểm tra trạng thái hiện tại để đảm bảo chuyển đổi hợp lệ
                if (bill.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
                    return (false, $"Hóa đơn đã ở trạng thái {newStatus}.");

                // Cập nhật trạng thái hóa đơn
                bill.Status = newStatus;

                try
                {
                    // Cập nhật vào cơ sở dữ liệu
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
    }


}




    

