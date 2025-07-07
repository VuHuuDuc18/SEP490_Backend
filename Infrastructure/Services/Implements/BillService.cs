using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.Food;
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
        private readonly IRepository<ImageFood> _foodImageRepository;
        private readonly IRepository<ImageMedicine> _medicineImageRepository;
        private readonly IRepository<ImageBreed> _breedImageRepository;
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<Barn> _barnRepository;
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
            IRepository<Barn> barnRepository,
            IRepository<LivestockCircleFood> livestockCircleFoodRepository,
            IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository,
            IRepository<ImageFood> foodImageRepository,
            IRepository<ImageMedicine> medicineImageRepository,
            IRepository<ImageBreed> breedImageRepository
            )

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
            _barnRepository = barnRepository ?? throw new ArgumentNullException(nameof(barnRepository));
            _foodImageRepository = foodImageRepository;
            _medicineImageRepository = medicineImageRepository;
            _breedImageRepository = breedImageRepository;
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
                var responses = new List<BillItemResponse>();

                foreach (var billItem in paginationResult.Items)
                {
                    if (bill.TypeBill == TypeBill.FOOD)
                    {
                        var food = await _foodRepository.GetById(billItem.FoodId.Value, checkError);
                        var images = await _foodImageRepository.GetQueryable(x => x.FoodId == food.Id).ToListAsync(cancellationToken);
                        var foodResponse = new FoodBillResponse
                        {
                            Id = food.Id,
                            FoodName = food.FoodName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        };
                        responses = paginationResult.Items.Select(i => new BillItemResponse
                        {
                            Id = i.Id,
                           // BillId = i.BillId,
                            Food = foodResponse,
                            Stock = i.Stock,
                            IsActive = i.IsActive
                        }).ToList();
                    } else if(bill.TypeBill == TypeBill.MEDICINE)
                    {
                        var medicine = await _medicineRepository.GetById(billItem.MedicineId.Value, checkError);
                        var images = await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicine.Id).ToListAsync(cancellationToken);
                        var medicineResponse = new MedicineBillResponse
                        {
                            Id = medicine.Id,
                            MedicineName = medicine.MedicineName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        };
                        responses = paginationResult.Items.Select(i => new BillItemResponse
                        {
                            Id = i.Id,
                         //  BillId = i.BillId,
                            Medicine = medicineResponse,
                            Stock = i.Stock,
                            IsActive = i.IsActive
                        }).ToList();
                    } else
                    {
                        var breed = await _breedRepository.GetById(billItem.BreedId.Value, checkError);
                        var images = await _breedImageRepository.GetQueryable(x => x.BreedId == breed.Id).ToListAsync(cancellationToken);
                        var breedResponse = new BreedBillResponse
                        {
                            Id = breed.Id,
                            BreedName = breed.BreedName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        };
                        responses = paginationResult.Items.Select(i => new BillItemResponse
                        {
                            Id = i.Id,
                           // BillId = i.BillId,
                            Breed = breedResponse,
                            Stock = i.Stock,
                            IsActive = i.IsActive
                        }).ToList();
                    }
                }
                 
                

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
            var userRequest = await _userRepository.GetById(bill.UserRequestId, checkError);
            var lscInfo = await _livestockCircleRepository.GetById(bill.LivestockCircleId, checkError);
            var barnInfo = await _barnRepository.GetById(lscInfo.BarnId, checkError);
            var wokerInfo = await _userRepository.GetById(barnInfo.WorkerId, checkError);
            var userRequestResponse = new UserRequestResponse
            {
                Id = userRequest.Id,
                FullName = userRequest.FullName,
                Email = userRequest.Email
            };
            var workerReponse = new WokerResponse
            {
                Id = wokerInfo.Id,
                FullName = wokerInfo.FullName,
                Email = wokerInfo.Email
            };
            var barnInfoResponse = new BarnDetailResponse
            {
                Id = barnInfo.Id,
                Address = barnInfo.Address,
                BarnName = barnInfo.BarnName,
                Image = barnInfo.Image,
                Worker = workerReponse
            };
            var lscInfoResponse = new LivestockCircleBillResponse
            {
                Id= lscInfo.Id,
                BarnDetailResponse = barnInfoResponse,
                LivestockCircleName = lscInfo.LivestockCircleName,
            };
            var response = new BillResponse
            {
                Id = bill.Id,
                UserRequest = userRequestResponse,
                LivestockCircle = lscInfoResponse,
                Name = bill.Name,
                Note = bill.Note,
                Total = bill.Total,
                Status = bill.Status,
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

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetById(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetById(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetById(lscInfo.BarnId);
                    var wokerInfo = await _userRepository.GetById(barnInfo.WorkerId);
                    var userRequestResponse = new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    };
                    var workerReponse = new WokerResponse
                    {
                        Id = wokerInfo.Id,
                        FullName = wokerInfo.FullName,
                        Email = wokerInfo.Email
                    };
                    var barnInfoResponse = new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerReponse
                    };
                    var lscInfoResponse = new LivestockCircleBillResponse
                    {
                        Id = lscInfo.Id,
                        BarnDetailResponse = barnInfoResponse,
                        LivestockCircleName = lscInfo.LivestockCircleName,
                    };
                    responses.Add(new BillResponse
                    {
                        Id = bill.Id,
                        UserRequest = userRequestResponse,
                        LivestockCircle = lscInfoResponse,
                        Name = bill.Name,
                        Note = bill.Note,
                        Total = bill.Total,
                        Status = bill.Status,
                        Weight = bill.Weight,
                        IsActive = bill.IsActive
                    });
                }

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

        public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillRequestByType(ListingRequest request,string billType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null) return (null, "Yêu cầu không được để trống.");
                if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any()) return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");
                 // lay bill theo loai Food, Medicine hay Breed va lay nhung bill chx dc Approval
                var query = _billRepository.GetQueryable(x => x.IsActive && x.TypeBill.Equals(billType) && x.Status.Equals(StatusConstant.REQUESTED));

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetById(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetById(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetById(lscInfo.BarnId);
                    var wokerInfo = await _userRepository.GetById(barnInfo.WorkerId);
                    var userRequestResponse = new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    };
                    var workerReponse = new WokerResponse
                    {
                        Id = wokerInfo.Id,
                        FullName = wokerInfo.FullName,
                        Email = wokerInfo.Email
                    };
                    var barnInfoResponse = new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerReponse
                    };
                    var lscInfoResponse = new LivestockCircleBillResponse
                    {
                        Id = lscInfo.Id,
                        BarnDetailResponse = barnInfoResponse,
                        LivestockCircleName = lscInfo.LivestockCircleName,
                    };
                    responses.Add(new BillResponse
                    {
                        Id = bill.Id,
                        UserRequest = userRequestResponse,
                        LivestockCircle = lscInfoResponse,
                        Name = bill.Name,
                        Note = bill.Note,
                        Total = bill.Total,
                        Status = bill.Status,
                        Weight = bill.Weight,
                        IsActive = bill.IsActive
                    });
                }

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
            float weight = 0;
            foreach (var foodItem in request.FoodItems)
            {
                var (isValid, errorMessage) = await ValidateItem(foodItem.ItemId, foodItem.Quantity, true, false, false, cancellationToken);

                if (isValid)
                {
                    var food = await _foodRepository.GetById(foodItem.ItemId);
                    weight += food.WeighPerUnit * foodItem.Quantity;
                }
                else
                {
                    return (false, errorMessage);
                }
            }

            var bill = new Bill
            {
                UserRequestId = request.UserRequestId,
                LivestockCircleId = request.LivestockCircleId,
                Name = $"Yêu cầu thức ăn - {DateTime.UtcNow}",
                Note = request.Note,
                Status = StatusConstant.REQUESTED,
                TypeBill = TypeBill.FOOD,
                Total = request.FoodItems.Sum(x => x.Quantity),
                Weight = weight
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
                TypeBill = TypeBill.MEDICINE,
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
                TypeBill = TypeBill.BREED,
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

        public async Task<bool> AdminUpdateBill(Admin_UpdateBarnRequest request)
        {
            try
            {
                var BillToUpdate = await _billRepository.GetById(request.BillId);
                if (BillToUpdate == null || !BillToUpdate.Status.Equals(StatusConstant.REQUESTED))
                {
                    return false;
                }
                if ( !(await ValidBreedStock(request.BreedId, request.Stock)))
                {
                    throw new Exception("Giống không khả dụng hoặc giống không đủ số lượng");
                }

                var UpdatedBreed = await _billItemRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(it => it.BillId == BillToUpdate.Id);
                UpdatedBreed.BreedId = request.BreedId;
                UpdatedBreed.Stock = request.Stock;

                _billItemRepository.Update(UpdatedBreed);
                await _billItemRepository.CommitAsync();

                // cap nhat livestockCircle
                var UpdatedLivestockCircle = await _livestockCircleRepository.GetById(BillToUpdate.LivestockCircleId);
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

        public async Task<(bool Success, string ErrorMessage)> UpdateBillFood(Guid billId, UpdateBillFoodDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.FoodItems.Any()) return (false, "Phải cung cấp danh sách mặt hàng thức ăn.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive && x.FoodId.HasValue).ToListAsync(cancellationToken);
            if (existingItems.Any() && existingItems.Any(x => !x.FoodId.HasValue))
                return (false, "Hóa đơn chứa các loại mặt hàng không phải thức ăn.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));


            var newItemsDict = request.FoodItems.ToDictionary(x => x.ItemId, x => x.Quantity);

            try
            {
                // Lấy tất cả item hiện tại trong bill
                var currentItems = existingItems.ToDictionary(x => x.FoodId.Value, x => x);

                // Xử lý thêm item mới
                foreach (var newItem in request.FoodItems.Where(x => x.Quantity > 0))
                {
                    if (!currentItems.ContainsKey(newItem.ItemId))
                    {

                        var food = new Food();
                        var (isValid, errorMessage) = await ValidateItem(newItem.ItemId, newItem.Quantity, true, false, false, cancellationToken);
                        if (isValid)
                        {
                             food = await _foodRepository.GetById(newItem.ItemId);
                        }
                        else
                        {
                            return (false, errorMessage);
                        }


                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            FoodId = newItem.ItemId,
                            MedicineId = null,
                            BreedId = null,
                            Stock = newItem.Quantity
                        };
                        _billItemRepository.Insert(billItem);
                        bill.Total += newItem.Quantity;

                        bill.Weight += food.WeighPerUnit * newItem.Quantity;

                    }
                }

                // Xử lý cập nhật hoặc xóa item cũ
                foreach (var currentItem in currentItems.Values.ToList())
                {
                    if (newItemsDict.TryGetValue(currentItem.FoodId.Value, out int newQuantity))
                    {
                        if (newQuantity != currentItem.Stock)
                        {

                            var food = new Food();
                            var (isValid, errorMessage) = await ValidateItem(currentItem.FoodId.Value, newQuantity, true, false, false, cancellationToken);
                            if (isValid)
                            {
                                food = await _foodRepository.GetById(currentItem.FoodId.Value);
                            }
                            else
                            {
                                return (false, errorMessage);
                            }

                            bill.Total -= currentItem.Stock;
                            bill.Weight -= food.WeighPerUnit * currentItem.Stock;
                            currentItem.Stock = newQuantity;
                            bill.Total += newQuantity;
                            bill.Weight += food.WeighPerUnit * newQuantity;

                            _billItemRepository.Update(currentItem);
                        }
                    }
                    else if (newItemsDict.All(x => x.Key != currentItem.FoodId.Value))
                    {

                        var food = await _foodRepository.GetById(currentItem.FoodId.Value);
                        bill.Total -= currentItem.Stock;
                        bill.Weight -= food.WeighPerUnit * currentItem.Stock;

                        currentItem.IsActive = false;
                        _billItemRepository.Update(currentItem);
                    }
                }

                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật hóa đơn với mặt hàng thức ăn: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBillMedicine(Guid billId, UpdateBillMedicineDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.MedicineItems.Any()) return (false, "Phải cung cấp danh sách mặt hàng thuốc.");

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive && x.MedicineId.HasValue).ToListAsync(cancellationToken);
            if (existingItems.Any() && existingItems.Any(x => !x.MedicineId.HasValue))
                return (false, "Hóa đơn chứa các loại mặt hàng không phải thuốc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            // Chuyển danh sách mới thành dictionary để dễ so sánh
            var newItemsDict = request.MedicineItems.ToDictionary(x => x.ItemId, x => x.Quantity);

            try
            {
                // Lấy tất cả item hiện tại trong bill
                var currentItems = existingItems.ToDictionary(x => x.MedicineId.Value, x => x);

                // Xử lý thêm item mới
                foreach (var newItem in request.MedicineItems.Where(x => x.Quantity > 0))
                {
                    if (!currentItems.ContainsKey(newItem.ItemId))
                    {
                        var (isValid, errorMessage) = await ValidateItem(newItem.ItemId, newItem.Quantity, false, true, false, cancellationToken);
                        if (!isValid) return (false, errorMessage);

                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            FoodId = null,
                            MedicineId = newItem.ItemId,
                            BreedId = null,
                            Stock = newItem.Quantity
                        };
                        _billItemRepository.Insert(billItem);
                        bill.Total += newItem.Quantity;
                    }
                }

                // Xử lý cập nhật hoặc xóa item cũ
                foreach (var currentItem in currentItems.Values.ToList())
                {
                    if (newItemsDict.TryGetValue(currentItem.MedicineId.Value, out int newQuantity))
                    {
                        if (newQuantity != currentItem.Stock)
                        {
                            var (isValid, errorMessage) = await ValidateItem(currentItem.MedicineId.Value, newQuantity, false, true, false, cancellationToken);
                            if (!isValid) return (false, errorMessage);

                            bill.Total -= currentItem.Stock;
                            currentItem.Stock = newQuantity;
                            bill.Total += newQuantity;
                            _billItemRepository.Update(currentItem);
                        }
                    }
                    else if (newItemsDict.All(x => x.Key != currentItem.MedicineId.Value))
                    {
                        bill.Total -= currentItem.Stock;
                        currentItem.IsActive = false;
                        _billItemRepository.Update(currentItem);
                    }
                }

                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật hóa đơn với mặt hàng thuốc: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBillBreed(Guid billId, UpdateBillBreedDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.BreedItems.Any()) return (false, "Phải cung cấp danh sách mặt hàng giống.");
            foreach(var it in request.BreedItems)
            {
                if (!(await ValidBreedStock(it.ItemId, it.Quantity)))
                {
                    throw new Exception("Giống không khả dụng hoặc không đủ số lượng");
                }
            }

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetById(billId, checkError);
            if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
            if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

            var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive && x.BreedId.HasValue).ToListAsync(cancellationToken);
            if (existingItems.Any() && existingItems.Any(x => !x.BreedId.HasValue))
                return (false, "Hóa đơn chứa các loại mặt hàng không phải giống.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            // Chuyển danh sách mới thành dictionary để dễ so sánh
            var newItemsDict = request.BreedItems.ToDictionary(x => x.ItemId, x => x.Quantity);

            try
            {
                // Lấy tất cả item hiện tại trong bill
                var currentItems = existingItems.ToDictionary(x => x.BreedId.Value, x => x);

                // Xử lý thêm item mới
                foreach (var newItem in request.BreedItems.Where(x => x.Quantity > 0))
                {
                    if (!currentItems.ContainsKey(newItem.ItemId))
                    {
                        var (isValid, errorMessage) = await ValidateItem(newItem.ItemId, newItem.Quantity, false, false, true, cancellationToken);
                        if (!isValid) return (false, errorMessage);

                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            FoodId = null,
                            MedicineId = null,
                            BreedId = newItem.ItemId,
                            Stock = newItem.Quantity
                        };
                        _billItemRepository.Insert(billItem);
                        bill.Total += newItem.Quantity;
                    }
                }

                // Xử lý cập nhật hoặc xóa item cũ
                foreach (var currentItem in currentItems.Values.ToList())
                {
                    if (newItemsDict.TryGetValue(currentItem.BreedId.Value, out int newQuantity))
                    {
                        if (newQuantity != currentItem.Stock)
                        {
                            var (isValid, errorMessage) = await ValidateItem(currentItem.BreedId.Value, newQuantity, false, false, true, cancellationToken);
                            if (!isValid) return (false, errorMessage);

                            bill.Total -= currentItem.Stock;
                            currentItem.Stock = newQuantity;
                            bill.Total += newQuantity;
                            _billItemRepository.Update(currentItem);
                        }
                    }
                    else if (newItemsDict.All(x => x.Key != currentItem.BreedId.Value))
                    {
                        bill.Total -= currentItem.Stock;
                        currentItem.IsActive = false;
                        _billItemRepository.Update(currentItem);
                    }
                }

                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật hóa đơn với mặt hàng giống: {ex.Message}");
            }
        }


        // Common func
        protected async Task<bool> ValidBreedStock(Guid breedId, int stock)
        {
            var BreedToValid = await _breedRepository.GetById(breedId);
            if (BreedToValid == null)
            {
                return false;
            }
            if (BreedToValid.Stock < stock)
            {
                return false;
            }
            return true;
        }

    }
}
