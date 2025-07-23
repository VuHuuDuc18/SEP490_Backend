using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.Food;
using Infrastructure.Extensions;
using Domain.Helper.Constants;
using Domain.IServices;
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
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Application.Wrappers;

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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentUserId;

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
            IRepository<ImageBreed> breedImageRepository,
            IHttpContextAccessor httpContextAccessor
            )

        {
            _billRepository = billRepository;
            _billItemRepository = billItemRepository;
            _userRepository = userRepository;
            _livestockCircleRepository = livestockCircleRepository;
            _foodRepository = foodRepository;
            _medicineRepository = medicineRepository;
            _breedRepository = breedRepository;
            _livestockCircleFoodRepository = livestockCircleFoodRepository;
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository;
            _barnRepository = barnRepository;
            _foodImageRepository = foodImageRepository;
            _medicineImageRepository = medicineImageRepository;
            _breedImageRepository = breedImageRepository;
            _httpContextAccessor = httpContextAccessor;
            _currentUserId = Guid.Empty;
            // Lấy current user từ JWT token claims
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        private async Task<(bool Success, string ErrorMessage)> ValidateItem(Guid itemId, int quantity, bool isFood, bool isMedicine, bool isBreed, CancellationToken cancellationToken)
        {
            var checkError = new Ref<CheckError>();
            if (isFood)
            {
                var food = await _foodRepository.GetByIdAsync(itemId, checkError);
                return food != null && food.IsActive && food.Stock >= quantity
                    ? (true, null)
                    : (false, $"Thức ăn với ID {itemId} không tồn tại, không hoạt động hoặc không đủ tồn kho.");
            }
            else if (isMedicine)
            {
                var medicine = await _medicineRepository.GetByIdAsync(itemId, checkError);
                return medicine != null && medicine.IsActive && medicine.Stock >= quantity
                    ? (true, null)
                    : (false, $"Thuốc với ID {itemId} không tồn tại, không hoạt động hoặc không đủ tồn kho.");
            }
            else if (isBreed)
            {
                var breed = await _breedRepository.GetByIdAsync(itemId, checkError);
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
                var food = await _foodRepository.GetByIdAsync(billItem.FoodId.Value, checkError);
                if (food != null) { food.Stock -= quantity; _foodRepository.Update(food); }
            }
            else if (billItem.MedicineId.HasValue)
            {
                var medicine = await _medicineRepository.GetByIdAsync(billItem.MedicineId.Value, checkError);
                if (medicine != null) { medicine.Stock -= quantity; _medicineRepository.Update(medicine); }
            }
            else if (billItem.BreedId.HasValue)
            {
                var breed = await _breedRepository.GetByIdAsync(billItem.BreedId.Value, checkError);
                if (breed != null) { breed.Stock -= quantity; _breedRepository.Update(breed); }
            }
            await Task.CompletedTask;
        }

        private async Task UpdateLivestockCircle(Guid livestockCircleId, int quantity, int? deadUnit, float? averageWeight, CancellationToken cancellationToken)
        {
            var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId, new Ref<CheckError>());
            if (livestockCircle != null)
            {
                livestockCircle.Status = StatusConstant.GROWINGSTAT;
                livestockCircle.TotalUnit = quantity;
                livestockCircle.GoodUnitNumber = (quantity - (deadUnit ?? 0));
                livestockCircle.DeadUnit = deadUnit ?? 0;
                livestockCircle.StartDate = DateTime.UtcNow;
                livestockCircle.AverageWeight = averageWeight ?? livestockCircle.AverageWeight;
                _livestockCircleRepository.Update(livestockCircle);
            }
            await Task.CompletedTask;
        }

        public async Task<Response<bool>> DisableBillItem(
             Guid billItemId,
             CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<bool>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var billItem = await _billItemRepository.GetByIdAsync(billItemId);
                if (billItem == null || !billItem.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Mục hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Mục hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billItem.BillId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                billItem.IsActive = false;
                bill.Total -= billItem.Stock;
                _billItemRepository.Update(billItem);
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Vô hiệu hóa mục hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi vô hiệu hóa mục hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<bool>> DisableBill(
              Guid billId,
              CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<bool>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                bill.IsActive = false;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Vô hiệu hóa hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi vô hiệu hóa hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<BillItemResponse>>> GetBillItemsByBillId(
           Guid billId,
           ListingRequest request,
           CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<BillItemResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }


                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }
                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<PaginationSet<BillItemResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại",
                        Errors = new List<string> { "Hóa đơn không tồn tại" }
                    };
                }

                var query = _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive);

                if (request.SearchString?.Any() == true)
                {
                    query = query.SearchString(request.SearchString);
                }

                if (request.Filter?.Any() == true)
                {
                    query = query.Filter(request.Filter);
                }

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);
                var responses = new List<BillItemResponse>();

                foreach (var billItem in paginationResult.Items)
                {
                    if (bill.TypeBill == TypeBill.FOOD && billItem.FoodId.HasValue)
                    {
                        var food = await _foodRepository.GetByIdAsync(billItem.FoodId.Value);
                        var images = food != null ? await _foodImageRepository.GetQueryable(x => x.FoodId == food.Id).ToListAsync(cancellationToken) : new List<ImageFood>();
                        var foodResponse = food != null ? new FoodBillResponse
                        {
                            Id = food.Id,
                            FoodName = food.FoodName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new FoodBillResponse();
                        responses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Food = foodResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
                    }
                    else if (bill.TypeBill == TypeBill.MEDICINE && billItem.MedicineId.HasValue)
                    {
                        var medicine = await _medicineRepository.GetByIdAsync(billItem.MedicineId.Value);
                        var images = medicine != null ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicine.Id).ToListAsync(cancellationToken) : new List<ImageMedicine>();
                        var medicineResponse = medicine != null ? new MedicineBillResponse
                        {
                            Id = medicine.Id,
                            MedicineName = medicine.MedicineName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new MedicineBillResponse();
                        responses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Medicine = medicineResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
                    }
                    else if (bill.TypeBill == TypeBill.BREED && billItem.BreedId.HasValue)
                    {
                        var breed = await _breedRepository.GetByIdAsync(billItem.BreedId.Value);
                        var images = breed != null ? await _breedImageRepository.GetQueryable(x => x.BreedId == breed.Id).ToListAsync(cancellationToken) : new List<ImageBreed>();
                        var breedResponse = breed != null ? new BreedBillResponse
                        {
                            Id = breed.Id,
                            BreedName = breed.BreedName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new BreedBillResponse();
                        responses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Breed = breedResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
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

                return new Response<PaginationSet<BillItemResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách mục hóa đơn thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillItemResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách mục hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<BillResponse>> GetBillById(
             Guid billId,
             CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<BillResponse>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null)
                {
                    return new Response<BillResponse>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại",
                        Errors = new List<string> { "Hóa đơn không tồn tại" }
                    };
                }

                var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                var barnInfo = lscInfo != null ? await _barnRepository.GetByIdAsync(lscInfo.BarnId) : null;
                var workerInfo = barnInfo != null ? await _userRepository.GetByIdAsync(barnInfo.WorkerId) : null;

                // Lấy danh sách BillItems
                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive)
                    .ToListAsync(cancellationToken);
                var billItemResponses = new List<BillItemResponse>();

                foreach (var billItem in billItems)
                {
                    if (bill.TypeBill == TypeBill.FOOD && billItem.FoodId.HasValue)
                    {
                        var food = await _foodRepository.GetByIdAsync(billItem.FoodId.Value);
                        var images = food != null ? await _foodImageRepository.GetQueryable(x => x.FoodId == food.Id)
                            .ToListAsync(cancellationToken) : new List<ImageFood>();
                        var foodResponse = food != null ? new FoodBillResponse
                        {
                            Id = food.Id,
                            FoodName = food.FoodName,
                            Stock = food.Stock,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new FoodBillResponse();
                        billItemResponses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Food = foodResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
                    }
                    else if (bill.TypeBill == TypeBill.MEDICINE && billItem.MedicineId.HasValue)
                    {
                        var medicine = await _medicineRepository.GetByIdAsync(billItem.MedicineId.Value);
                        var images = medicine != null ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicine.Id)
                            .ToListAsync(cancellationToken) : new List<ImageMedicine>();
                        var medicineResponse = medicine != null ? new MedicineBillResponse
                        {
                            Id = medicine.Id,
                            MedicineName = medicine.MedicineName,
                            Stock = medicine.Stock,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new MedicineBillResponse();
                        billItemResponses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Medicine = medicineResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
                    }
                    else if (bill.TypeBill == TypeBill.BREED && billItem.BreedId.HasValue)
                    {
                        var breed = await _breedRepository.GetByIdAsync(billItem.BreedId.Value);
                        var images = breed != null ? await _breedImageRepository.GetQueryable(x => x.BreedId == breed.Id)
                            .ToListAsync(cancellationToken) : new List<ImageBreed>();
                        var breedResponse = breed != null ? new BreedBillResponse
                        {
                            Id = breed.Id,
                            BreedName = breed.BreedName,
                            Stock = breed.Stock,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        } : new BreedBillResponse();
                        billItemResponses.Add(new BillItemResponse
                        {
                            Id = billItem.Id,
                            Breed = breedResponse,
                            Stock = billItem.Stock,
                            IsActive = billItem.IsActive
                        });
                    }
                }

                var userRequestResponse = userRequest != null ? new UserRequestResponse
                {
                    Id = userRequest.Id,
                    FullName = userRequest.FullName,
                    Email = userRequest.Email
                } : new UserRequestResponse();

                var workerResponse = workerInfo != null ? new WokerResponse
                {
                    Id = workerInfo.Id,
                    FullName = workerInfo.FullName,
                    Email = workerInfo.Email
                } : new WokerResponse();

                var barnInfoResponse = barnInfo != null ? new BarnDetailResponse
                {
                    Id = barnInfo.Id,
                    Address = barnInfo.Address,
                    BarnName = barnInfo.BarnName,
                    Image = barnInfo.Image,
                    Worker = workerResponse
                } : new BarnDetailResponse();

                var lscInfoResponse = lscInfo != null ? new LivestockCircleBillResponse
                {
                    Id = lscInfo.Id,
                    BarnDetailResponse = barnInfoResponse,
                    LivestockCircleName = lscInfo.LivestockCircleName
                } : new LivestockCircleBillResponse();

                var response = new BillResponse
                {
                    Id = bill.Id,
                    UserRequest = userRequestResponse,
                    LivestockCircle = lscInfoResponse,
                    Name = bill.Name,
                    Note = bill.Note,
                    Total = bill.Total,
                    TypeBill = bill.TypeBill,
                    Status = bill.Status,
                    Weight = bill.Weight,
                    IsActive = bill.IsActive,
                    BillItem = billItemResponses // Thêm danh sách BillItems
                };

                return new Response<BillResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy hóa đơn thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<BillResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        //public async Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedBillList(ListingRequest request, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (request == null) return (null, "Yêu cầu không được để trống.");
        //        if (request.PageIndex < 1 || request.PageSize < 1) return (null, "PageIndex và PageSize phải lớn hơn 0.");

        //        var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        //        var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
        //            .Select(f => f.Field).ToList() ?? new List<string>();
        //        if (invalidFields.Any()) return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

        //        var query = _billRepository.GetQueryable(x => x.IsActive);

        //        if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
        //        if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

        //        var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

        //        var responses = new List<BillResponse>();
        //        foreach (var bill in paginationResult.Items)
        //        {
        //            var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
        //            var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
        //            var barnInfo = await _barnRepository.GetByIdAsync(lscInfo.BarnId);
        //            var wokerInfo = await _userRepository.GetByIdAsync(barnInfo.WorkerId);
        //            var userRequestResponse = new UserRequestResponse
        //            {
        //                Id = userRequest.Id,
        //                FullName = userRequest.FullName,
        //                Email = userRequest.Email
        //            };
        //            var workerReponse = new WokerResponse
        //            {
        //                Id = wokerInfo.Id,
        //                FullName = wokerInfo.FullName,
        //                Email = wokerInfo.Email
        //            };
        //            var barnInfoResponse = new BarnDetailResponse
        //            {
        //                Id = barnInfo.Id,
        //                Address = barnInfo.Address,
        //                BarnName = barnInfo.BarnName,
        //                Image = barnInfo.Image,
        //                Worker = workerReponse
        //            };
        //            var lscInfoResponse = new LivestockCircleBillResponse
        //            {
        //                Id = lscInfo.Id,
        //                BarnDetailResponse = barnInfoResponse,
        //                LivestockCircleName = lscInfo.LivestockCircleName,
        //            };
        //            responses.Add(new BillResponse
        //            {
        //                Id = bill.Id,
        //                UserRequest = userRequestResponse,
        //                LivestockCircle = lscInfoResponse,
        //                Name = bill.Name,
        //                Note = bill.Note,
        //                Total = bill.Total,
        //                Status = bill.Status,
        //                Weight = bill.Weight,
        //                IsActive = bill.IsActive
        //            });
        //        }

        //        var result = new PaginationSet<BillResponse>
        //        {
        //            PageIndex = paginationResult.PageIndex,
        //            Count = responses.Count,
        //            TotalCount = paginationResult.TotalCount,
        //            TotalPages = paginationResult.TotalPages,
        //            Items = responses
        //        };

        //        return (result, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (null, $"Lỗi khi lấy danh sách hóa đơn: {ex.Message}");
        //    }
        //}


        //Lấy tất cả các bill theo loại cho các room staff các hóa đơn đang đc request
        public async Task<Response<PaginationSet<BillResponse>>> GetBillRequestByType(
             ListingRequest request,
             string billType,
             CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }


                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                // Lấy hóa đơn theo loại (Food, Medicine, Breed) và những hóa đơn chưa được Approval
                var validBillTypes = new[] { TypeBill.FOOD, TypeBill.MEDICINE, TypeBill.BREED };
                if (string.IsNullOrEmpty(billType) || !validBillTypes.Contains(billType))
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Loại hóa đơn không hợp lệ, chỉ chấp nhận Food, Medicine hoặc Breed",
                        Errors = new List<string> { "Loại hóa đơn không hợp lệ, chỉ chấp nhận Food, Medicine hoặc Breed" }
                    };
                }

                var query = _billRepository.GetQueryable(x => x.IsActive && x.TypeBill.Equals(billType) && x.Status.Equals(StatusConstant.REQUESTED));

                if (request.SearchString?.Any() == true)
                {
                    query = query.SearchString(request.SearchString);
                }

                if (request.Filter?.Any() == true)
                {
                    query = query.Filter(request.Filter);
                }

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                    var barnInfo = lscInfo != null ? await _barnRepository.GetByIdAsync(lscInfo.BarnId) : null;
                    var workerInfo = barnInfo != null ? await _userRepository.GetByIdAsync(barnInfo.WorkerId) : null;

                    var userRequestResponse = userRequest != null ? new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    } : new UserRequestResponse();

                    var workerResponse = workerInfo != null ? new WokerResponse
                    {
                        Id = workerInfo.Id,
                        FullName = workerInfo.FullName,
                        Email = workerInfo.Email
                    } : new WokerResponse();

                    var barnInfoResponse = barnInfo != null ? new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerResponse
                    } : new BarnDetailResponse();

                    var lscInfoResponse = lscInfo != null ? new LivestockCircleBillResponse
                    {
                        Id = lscInfo.Id,
                        BarnDetailResponse = barnInfoResponse,
                        LivestockCircleName = lscInfo.LivestockCircleName,
                    } : new LivestockCircleBillResponse();

                    responses.Add(new BillResponse
                    {
                        Id = bill.Id,
                        UserRequest = userRequestResponse,
                        LivestockCircle = lscInfoResponse,
                        Name = bill.Name,
                        Note = bill.Note,
                        TypeBill = bill.TypeBill,
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

                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách hóa đơn thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Hủy hóa đơn (chỉ từ trạng thái REQUESTED).
        /// </summary>
        public async Task<Response<bool>> CancelBill(
               Guid billId,
               CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                if (bill.Status != StatusConstant.REQUESTED)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = $"Chỉ có thể hủy hóa đơn khi trạng thái là {StatusConstant.REQUESTED}.",
                        Errors = new List<string> { $"Chỉ có thể hủy hóa đơn khi trạng thái là {StatusConstant.REQUESTED}." }
                    };
                }

                bill.Status = StatusConstant.CANCELLED;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Hủy hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi hủy hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Duyệt hóa đơn (chỉ từ trạng thái REQUESTED).
        /// </summary>
        public async Task<Response<bool>> ApproveBill(
            Guid billId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                if (bill.Status != StatusConstant.REQUESTED)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = $"Chỉ có thể duyệt hóa đơn khi trạng thái là {StatusConstant.REQUESTED}.",
                        Errors = new List<string> { $"Chỉ có thể duyệt hóa đơn khi trạng thái là {StatusConstant.REQUESTED}." }
                    };
                }

                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
                foreach (var item in billItems)
                {
                    await UpdateStock(item, item.Stock, cancellationToken);
                }

                bill.Status = StatusConstant.APPROVED;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Duyệt hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi duyệt hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Xác nhận hóa đơn (chỉ từ trạng thái APPROVED).
        /// </summary>
        public async Task<Response<bool>> ConfirmBill(
            Guid billId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                if (bill.Status != StatusConstant.APPROVED)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = $"Chỉ có thể xác nhận hóa đơn khi trạng thái là {StatusConstant.APPROVED}.",
                        Errors = new List<string> { $"Chỉ có thể xác nhận hóa đơn khi trạng thái là {StatusConstant.APPROVED}." }
                    };
                }

                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
                foreach (var item in billItems)
                {
                    if (item.FoodId.HasValue)
                    {
                        var lcFood = await _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.FoodId == item.FoodId).FirstOrDefaultAsync(cancellationToken);
                        if (lcFood == null)
                        {
                            lcFood = new LivestockCircleFood
                            {
                                LivestockCircleId = bill.LivestockCircleId,
                                FoodId = item.FoodId.Value,
                                Remaining = item.Stock
                            };
                            _livestockCircleFoodRepository.Insert(lcFood);
                        }
                        else
                        {
                            lcFood.Remaining += item.Stock;
                            _livestockCircleFoodRepository.Update(lcFood);
                        }
                    }
                    else if (item.MedicineId.HasValue)
                    {
                        var lcMedicine = await _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.MedicineId == item.MedicineId).FirstOrDefaultAsync(cancellationToken);
                        if (lcMedicine == null)
                        {
                            lcMedicine = new LivestockCircleMedicine
                            {
                                LivestockCircleId = bill.LivestockCircleId,
                                MedicineId = item.MedicineId.Value,
                                Remaining = item.Stock
                            };
                            _livestockCircleMedicineRepository.Insert(lcMedicine);
                        }
                        else
                        {
                            lcMedicine.Remaining += item.Stock;
                            _livestockCircleMedicineRepository.Update(lcMedicine);
                        }
                    }
                    else if (item.BreedId.HasValue)
                    {
                        await UpdateLivestockCircle(bill.LivestockCircleId, item.Stock, 0, null, cancellationToken);
                    }
                }

                bill.Status = StatusConstant.CONFIRMED;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Xác nhận hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xác nhận hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Từ chối hóa đơn (từ REQUESTED hoặc APPROVED).
        /// </summary>
        public async Task<Response<bool>> RejectBill(
            Guid billId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(billId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                if (bill.Status != StatusConstant.REQUESTED && bill.Status != StatusConstant.APPROVED)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = $"Chỉ có thể từ chối hóa đơn khi trạng thái là {StatusConstant.REQUESTED} hoặc {StatusConstant.APPROVED}.",
                        Errors = new List<string> { $"Chỉ có thể từ chối hóa đơn khi trạng thái là {StatusConstant.REQUESTED} hoặc {StatusConstant.APPROVED}." }
                    };
                }

                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);

                if (bill.Status == StatusConstant.APPROVED)
                {
                    // Rollback stock nếu APPROVED
                    foreach (var item in billItems)
                    {
                        await UpdateStock(item, -item.Stock, cancellationToken);
                    }
                }

                bill.Status = StatusConstant.REJECTED;
                _billRepository.Update(bill);
                await _billRepository.CommitAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Từ chối hóa đơn thành công",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi từ chối hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        //public async Task<(bool Success, string ErrorMessage)> ChangeBillStatus(Guid billId, string newStatus, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var validStatuses = new[] { StatusConstant.REQUESTED, StatusConstant.APPROVED, StatusConstant.CONFIRMED, StatusConstant.REJECTED, StatusConstant.COMPLETED, StatusConstant.CANCELLED };
        //        if (string.IsNullOrEmpty(newStatus) || !validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
        //            return (false, $"Trạng thái không hợp lệ: {newStatus}. Phải là một trong: {string.Join(", ", validStatuses)}.");

        //        var checkError = new Ref<CheckError>();
        //        var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //        if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //        if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //        if (bill.Status.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
        //            return (false, $"Hóa đơn đã ở trạng thái {newStatus}.");

        //        try
        //        {
        //            bill.Status = newStatus;

        //            if (newStatus == StatusConstant.APPROVED)
        //            {
        //                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
        //                foreach (var item in billItems)
        //                {
        //                    await UpdateStock(item, item.Stock, cancellationToken);
        //                }
        //            }
        //            else if (newStatus == StatusConstant.CONFIRMED)
        //            {
        //                var billItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
        //                foreach (var item in billItems)
        //                {
        //                    if (item.FoodId.HasValue)
        //                    {
        //                        var lcFood = await _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.FoodId == item.FoodId).FirstOrDefaultAsync(cancellationToken);
        //                        if (lcFood == null)
        //                        {
        //                            lcFood = new LivestockCircleFood { LivestockCircleId = bill.LivestockCircleId, FoodId = item.FoodId.Value, Remaining = 0 };
        //                            _livestockCircleFoodRepository.Insert(lcFood);
        //                        }
        //                        lcFood.Remaining += item.Stock;
        //                        _livestockCircleFoodRepository.Update(lcFood);
        //                    }
        //                    else if (item.MedicineId.HasValue)
        //                    {
        //                        var lcMedicine = await _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == bill.LivestockCircleId && x.MedicineId == item.MedicineId).FirstOrDefaultAsync(cancellationToken);
        //                        if (lcMedicine == null)
        //                        {
        //                            lcMedicine = new LivestockCircleMedicine { LivestockCircleId = bill.LivestockCircleId, MedicineId = item.MedicineId.Value, Remaining = 0 };
        //                            _livestockCircleMedicineRepository.Insert(lcMedicine);
        //                        }
        //                        lcMedicine.Remaining += item.Stock;
        //                        _livestockCircleMedicineRepository.Update(lcMedicine);
        //                    }
        //                    else if (item.BreedId.HasValue)
        //                    {
        //                        await UpdateLivestockCircle(bill.LivestockCircleId, item.Stock, 0, null, cancellationToken);
        //                    }
        //                }
        //            }

        //            _billRepository.Update(bill);
        //            await _billRepository.CommitAsync(cancellationToken);
        //            return (true, null);
        //        }
        //        catch (Exception ex)
        //        {
        //            return (false, $"Lỗi khi cập nhật trạng thái hóa đơn: {ex.Message}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi thay đổi trạng thái hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> AddFoodItemToBill(Guid billId, AddFoodItemToBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (!request.FoodItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thức ăn.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
        //    if (existingItems.Any() && !existingItems.All(x => x.FoodId.HasValue))
        //        return (false, "Hóa đơn chỉ được chứa mặt hàng thức ăn.");

        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    foreach (var item in request.FoodItems)
        //    {
        //        var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, true, false, false, cancellationToken);
        //        if (!isValid) return (false, errorMessage);
        //    }

        //    try
        //    {
        //        foreach (var item in request.FoodItems)
        //        {
        //            var billItem = new BillItem
        //            {
        //                BillId = bill.Id,
        //                FoodId = item.ItemId,
        //                MedicineId = null,
        //                BreedId = null,
        //                Stock = item.Quantity
        //            };
        //            _billItemRepository.Insert(billItem);
        //            bill.Total += item.Quantity;
        //        }
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi thêm mặt hàng thức ăn vào hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> AddMedicineItemToBill(Guid billId, AddMedicineItemToBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (!request.MedicineItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng thuốc.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
        //    if (existingItems.Any() && !existingItems.All(x => x.MedicineId.HasValue))
        //        return (false, "Hóa đơn chỉ được chứa mặt hàng thuốc.");

        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    foreach (var item in request.MedicineItems)
        //    {
        //        var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, true, false, cancellationToken);
        //        if (!isValid) return (false, errorMessage);
        //    }

        //    try
        //    {
        //        foreach (var item in request.MedicineItems)
        //        {
        //            var billItem = new BillItem
        //            {
        //                BillId = bill.Id,
        //                FoodId = null,
        //                MedicineId = item.ItemId,
        //                BreedId = null,
        //                Stock = item.Quantity
        //            };
        //            _billItemRepository.Insert(billItem);
        //            bill.Total += item.Quantity;
        //        }
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi thêm mặt hàng thuốc vào hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> AddBreedItemToBill(Guid billId, AddBreedItemToBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (!request.BreedItems.Any()) return (false, "Phải cung cấp ít nhất một mặt hàng giống.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == billId && x.IsActive).ToListAsync(cancellationToken);
        //    if (existingItems.Any() && !existingItems.All(x => x.BreedId.HasValue))
        //        return (false, "Hóa đơn chỉ được chứa mặt hàng giống.");

        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    foreach (var item in request.BreedItems)
        //    {
        //        var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, false, true, cancellationToken);
        //        if (!isValid) return (false, errorMessage);
        //    }

        //    try
        //    {
        //        foreach (var item in request.BreedItems)
        //        {
        //            var billItem = new BillItem
        //            {
        //                BillId = bill.Id,
        //                FoodId = null,
        //                MedicineId = null,
        //                BreedId = item.ItemId,
        //                Stock = item.Quantity
        //            };
        //            _billItemRepository.Insert(billItem);
        //            bill.Total += item.Quantity;
        //        }
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi thêm mặt hàng giống vào hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> UpdateFoodItemInBill(Guid billId, Guid itemId, UpdateFoodItemInBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (request.FoodItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng thức ăn.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.FoodId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thức ăn.");

        //    var item = request.FoodItems.First();
        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, true, false, false, cancellationToken);
        //    if (!isValid) return (false, errorMessage);

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.FoodId = item.ItemId;
        //        billItem.Stock = item.Quantity;
        //        bill.Total += item.Quantity;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi cập nhật mặt hàng thức ăn trong hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> UpdateMedicineItemInBill(Guid billId, Guid itemId, UpdateMedicineItemInBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (request.MedicineItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng thuốc.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.MedicineId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thuốc.");

        //    var item = request.MedicineItems.First();
        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, true, false, cancellationToken);
        //    if (!isValid) return (false, errorMessage);

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.MedicineId = item.ItemId;
        //        billItem.Stock = item.Quantity;
        //        bill.Total += item.Quantity;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi cập nhật mặt hàng thuốc trong hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> UpdateBreedItemInBill(Guid billId, Guid itemId, UpdateBreedItemInBillDto request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
        //    if (request.BreedItems.Count != 1) return (false, "Phải cung cấp chính xác một mặt hàng giống.");

        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.BreedId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng giống.");

        //    var item = request.BreedItems.First();
        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

        //    var (isValid, errorMessage) = await ValidateItem(item.ItemId, item.Quantity, false, false, true, cancellationToken);
        //    if (!isValid) return (false, errorMessage);

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.BreedId = item.ItemId;
        //        billItem.Stock = item.Quantity;
        //        bill.Total += item.Quantity;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi cập nhật mặt hàng giống trong hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> DeleteFoodItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        //{
        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.FoodId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thức ăn.");

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.IsActive = false;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi xóa mặt hàng thức ăn khỏi hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> DeleteMedicineItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        //{
        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.MedicineId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng thuốc.");

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.IsActive = false;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi xóa mặt hàng thuốc khỏi hóa đơn: {ex.Message}");
        //    }
        //}

        //public async Task<(bool Success, string ErrorMessage)> DeleteBreedItemFromBill(Guid billId, Guid itemId, CancellationToken cancellationToken = default)
        //{
        //    var checkError = new Ref<CheckError>();
        //    var bill = await _billRepository.GetByIdAsync(billId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy hóa đơn: {checkError.Value.Message}");
        //    if (bill == null || !bill.IsActive) return (false, "Hóa đơn không tồn tại hoặc không hoạt động.");

        //    var billItem = await _billItemRepository.GetByIdAsync(itemId, checkError);
        //    if (checkError.Value?.IsError == true) return (false, $"Lỗi khi lấy mục hóa đơn: {checkError.Value.Message}");
        //    if (billItem == null || !billItem.IsActive) return (false, "Mục hóa đơn không tồn tại hoặc không hoạt động.");
        //    if (billItem.BillId != billId) return (false, "Mục hóa đơn không thuộc hóa đơn được chỉ định.");
        //    if (!billItem.BreedId.HasValue) return (false, "Mục hóa đơn không phải là mặt hàng giống.");

        //    try
        //    {
        //        bill.Total -= billItem.Stock;
        //        billItem.IsActive = false;
        //        _billItemRepository.Update(billItem);
        //        _billRepository.Update(bill);
        //        await _billRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi xóa mặt hàng giống khỏi hóa đơn: {ex.Message}");
        //    }
        //}

        public async Task<Response<bool>> RequestFood(
           CreateFoodRequestDto request,
           CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu yêu cầu là bắt buộc",
                        Errors = new List<string> { "Dữ liệu yêu cầu là bắt buộc" }
                    };
                }

                if (!request.FoodItems.Any())
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Phải cung cấp ít nhất một mặt hàng thức ăn",
                        Errors = new List<string> { "Phải cung cấp ít nhất một mặt hàng thức ăn" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = string.Join("; ", validationResults.Select(v => v.ErrorMessage)),
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                float weight = 0;
                var groupedFoodItems = request.FoodItems
                    .GroupBy(f => f.ItemId)
                    .Select(g => new { ItemId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                    .ToList();

                foreach (var groupedItem in groupedFoodItems)
                {
                    var (isValid, errorMessage) = await ValidateItem(groupedItem.ItemId, groupedItem.TotalQuantity, true, false, false, cancellationToken);
                    if (!isValid)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = errorMessage,
                            Errors = new List<string> { errorMessage }
                        };
                    }

                    var food = await _foodRepository.GetByIdAsync(groupedItem.ItemId);
                    if (food == null)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = $"Mặt hàng thức ăn với ID {groupedItem.ItemId} không tồn tại",
                            Errors = new List<string> { $"Mặt hàng thức ăn với ID {groupedItem.ItemId} không tồn tại" }
                        };
                    }
                    if (groupedItem.TotalQuantity <= 0)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = $"Tổng số lượng phải lớn hơn 0 cho mặt hàng ID {groupedItem.ItemId}",
                            Errors = new List<string> { $"Tổng số lượng phải lớn hơn 0 cho mặt hàng ID {groupedItem.ItemId}" }
                        };
                    }
                    weight += food.WeighPerUnit * (float)groupedItem.TotalQuantity;
                }

                var bill = new Bill
                {
                    UserRequestId = _currentUserId,
                    LivestockCircleId = request.LivestockCircleId,
                    Name = $"Yêu cầu thức ăn của {_currentUserId} đến {request.LivestockCircleId} - {DateTime.UtcNow}",
                    Note = request.Note,
                    Status = StatusConstant.REQUESTED,
                    TypeBill = TypeBill.FOOD,
                    Total = groupedFoodItems.Sum(x => x.TotalQuantity),
                    Weight = weight,
                    IsActive = true
                };

                try
                {
                    _billRepository.Insert(bill);
                    foreach (var groupedItem in groupedFoodItems)
                    {
                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            FoodId = groupedItem.ItemId,
                            MedicineId = null,
                            BreedId = null,
                            Stock = groupedItem.TotalQuantity,
                            IsActive = true
                        };
                        _billItemRepository.Insert(billItem);
                    }
                    await _billRepository.CommitAsync(cancellationToken);

                    return new Response<bool>()
                    {
                        Succeeded = true,
                        Message = "Tạo yêu cầu thức ăn thành công",
                        Data = true
                    };
                }
                catch (Exception ex)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Lỗi khi tạo yêu cầu thức ăn",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo yêu cầu thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<bool>> RequestMedicine(
            CreateMedicineRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu yêu cầu là bắt buộc",
                        Errors = new List<string> { "Dữ liệu yêu cầu là bắt buộc" }
                    };
                }

                if (!request.MedicineItems.Any())
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Phải cung cấp ít nhất một mặt hàng thuốc",
                        Errors = new List<string> { "Phải cung cấp ít nhất một mặt hàng thuốc" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = string.Join("; ", validationResults.Select(v => v.ErrorMessage)),
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var groupedMedicineItems = request.MedicineItems
                    .GroupBy(m => m.ItemId)
                    .Select(g => new { ItemId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                    .ToList();

                foreach (var groupedItem in groupedMedicineItems)
                {
                    var (isValid, errorMessage) = await ValidateItem(groupedItem.ItemId, groupedItem.TotalQuantity, false, true, false, cancellationToken);
                    if (!isValid)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = errorMessage,
                            Errors = new List<string> { errorMessage }
                        };
                    }

                    var medicine = await _medicineRepository.GetByIdAsync(groupedItem.ItemId);
                    if (medicine == null)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = $"Mặt hàng thuốc với ID {groupedItem.ItemId} không tồn tại",
                            Errors = new List<string> { $"Mặt hàng thuốc với ID {groupedItem.ItemId} không tồn tại" }
                        };
                    }
                    if (groupedItem.TotalQuantity <= 0)
                    {
                        return new Response<bool>()
                        {
                            Succeeded = false,
                            Message = $"Tổng số lượng phải lớn hơn 0 cho mặt hàng ID {groupedItem.ItemId}",
                            Errors = new List<string> { $"Tổng số lượng phải lớn hơn 0 cho mặt hàng ID {groupedItem.ItemId}" }
                        };
                    }
                }

                var bill = new Bill
                {
                    UserRequestId = _currentUserId,
                    LivestockCircleId = request.LivestockCircleId,
                    Name = $"Yêu cầu thuốc của {_currentUserId} đến {request.LivestockCircleId} - {DateTime.UtcNow}",
                    Note = request.Note,
                    Status = StatusConstant.REQUESTED,
                    TypeBill = TypeBill.MEDICINE,
                    Total = groupedMedicineItems.Sum(x => x.TotalQuantity),
                    Weight = 0,
                    IsActive = true
                };

                try
                {
                    _billRepository.Insert(bill);
                    foreach (var groupedItem in groupedMedicineItems)
                    {
                        var billItem = new BillItem
                        {
                            BillId = bill.Id,
                            FoodId = null,
                            MedicineId = groupedItem.ItemId,
                            BreedId = null,
                            Stock = groupedItem.TotalQuantity,
                            IsActive = true
                        };
                        _billItemRepository.Insert(billItem);
                    }
                    await _billRepository.CommitAsync(cancellationToken);

                    return new Response<bool>()
                    {
                        Succeeded = true,
                        Message = "Tạo yêu cầu thuốc thành công",
                        Data = true
                    };
                }
                catch (Exception ex)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Lỗi khi tạo yêu cầu thuốc",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo yêu cầu thuốc",
                    Errors = new List<string> { ex.Message }
                };
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
                UserRequestId = request.UserRequestId,
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

        public async Task<Response<bool>> AdminUpdateBill(Admin_UpdateBarnRequest request)
        {
            try
            {
                var BillToUpdate = await _billRepository.GetByIdAsync(request.BillId);
                if (BillToUpdate == null || !BillToUpdate.Status.Equals(StatusConstant.REQUESTED))
                {
                    return new Response<bool>("Yêu cầu đã duyệt, không thể cập nhật");
                }
                if (!(await ValidBreedStock(request.BreedId, request.Stock)))
                {
                    return new Response<bool>("Giống không khả dụng hoặc giống không đủ số lượng");
                }

                var UpdatedBreed = await _billItemRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(it => it.BillId == BillToUpdate.Id);
                UpdatedBreed.BreedId = request.BreedId;
                UpdatedBreed.Stock = request.Stock;

                _billItemRepository.Update(UpdatedBreed);
                await _billItemRepository.CommitAsync();

                // cap nhat livestockCircle
                var UpdatedLivestockCircle = await _livestockCircleRepository.GetByIdAsync(BillToUpdate.LivestockCircleId);
                UpdatedLivestockCircle.BreedId = request.BreedId;

                _livestockCircleRepository.Update(UpdatedLivestockCircle);
                await _livestockCircleRepository.CommitAsync();

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Yêu cầu cập nhật thành công"
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>("Yêu cầu cập nhật thất bại");
            }
        }
        public async Task<Response<bool>> UpdateBillFood(
            UpdateBillFoodDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu yêu cầu là bắt buộc",
                        Errors = new List<string> { "Dữ liệu yêu cầu là bắt buộc" }
                    };
                }

                if (!request.FoodItems.Any())
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Phải cung cấp danh sách mặt hàng thức ăn",
                        Errors = new List<string> { "Phải cung cấp danh sách mặt hàng thức ăn" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(request.BillId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == request.BillId && x.IsActive && x.FoodId.HasValue).ToListAsync(cancellationToken);
                if (existingItems.Any() && existingItems.Any(x => !x.FoodId.HasValue))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn chứa các loại mặt hàng không phải thức ăn",
                        Errors = new List<string> { "Hóa đơn chứa các loại mặt hàng không phải thức ăn" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = string.Join("; ", validationResults.Select(v => v.ErrorMessage)),
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var groupedFoodItems = request.FoodItems
                    .GroupBy(f => f.ItemId)
                    .Select(g => new { ItemId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                    .ToList();

                var currentItems = existingItems.ToDictionary(x => x.FoodId.Value, x => x);

                try
                {
                    // Xử lý thêm hoặc cập nhật item mới
                    foreach (var groupedItem in groupedFoodItems.Where(x => x.TotalQuantity > 0))
                    {
                        var (isValid, errorMessage) = await ValidateItem(groupedItem.ItemId, groupedItem.TotalQuantity, true, false, false, cancellationToken);
                        if (!isValid)
                        {
                            return new Response<bool>()
                            {
                                Succeeded = false,
                                Message = errorMessage,
                                Errors = new List<string> { errorMessage }
                            };
                        }

                        var food = await _foodRepository.GetByIdAsync(groupedItem.ItemId);
                        if (food == null)
                        {
                            return new Response<bool>()
                            {
                                Succeeded = false,
                                Message = $"Mặt hàng thức ăn với ID {groupedItem.ItemId} không tồn tại",
                                Errors = new List<string> { $"Mặt hàng thức ăn với ID {groupedItem.ItemId} không tồn tại" }
                            };
                        }

                        if (currentItems.TryGetValue(groupedItem.ItemId, out var currentItem))
                        {
                            // Cập nhật item hiện tại
                            var oldQuantity = currentItem.Stock;
                            if (groupedItem.TotalQuantity != oldQuantity)
                            {
                                bill.Total -= oldQuantity;
                                bill.Weight -= food.WeighPerUnit * oldQuantity;
                                currentItem.Stock = groupedItem.TotalQuantity;
                                bill.Total += groupedItem.TotalQuantity;
                                bill.Weight += food.WeighPerUnit * groupedItem.TotalQuantity;
                                _billItemRepository.Update(currentItem);
                            }
                            currentItems.Remove(groupedItem.ItemId); // Đánh dấu đã xử lý
                        }
                        else
                        {
                            // Thêm item mới
                            var billItem = new BillItem
                            {
                                BillId = bill.Id,
                                FoodId = groupedItem.ItemId,
                                MedicineId = null,
                                BreedId = null,
                                Stock = groupedItem.TotalQuantity,
                                IsActive = true
                            };
                            _billItemRepository.Insert(billItem);
                            bill.Total += groupedItem.TotalQuantity;
                            bill.Weight += food.WeighPerUnit * groupedItem.TotalQuantity;
                        }
                    }

                    // Xử lý xóa item cũ không còn trong request
                    foreach (var currentItem in currentItems.Values)
                    {
                        var food = await _foodRepository.GetByIdAsync(currentItem.FoodId.Value);
                        bill.Total -= currentItem.Stock;
                        bill.Weight -= food.WeighPerUnit * currentItem.Stock;
                        currentItem.IsActive = false;
                        _billItemRepository.Update(currentItem);
                    }

                    _billRepository.Update(bill);
                    await _billRepository.CommitAsync(cancellationToken);

                    return new Response<bool>()
                    {
                        Succeeded = true,
                        Message = "Cập nhật hóa đơn với mặt hàng thức ăn thành công",
                        Data = true
                    };
                }
                catch (Exception ex)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Lỗi khi cập nhật hóa đơn với mặt hàng thức ăn",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật hóa đơn với mặt hàng thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<bool>> UpdateBillMedicine(
            UpdateBillMedicineDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu yêu cầu là bắt buộc",
                        Errors = new List<string> { "Dữ liệu yêu cầu là bắt buộc" }
                    };
                }

                if (!request.MedicineItems.Any())
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Phải cung cấp danh sách mặt hàng thuốc",
                        Errors = new List<string> { "Phải cung cấp danh sách mặt hàng thuốc" }
                    };
                }

                var bill = await _billRepository.GetByIdAsync(request.BillId);
                if (bill == null || !bill.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn không tồn tại hoặc không hoạt động",
                        Errors = new List<string> { "Hóa đơn không tồn tại hoặc không hoạt động" }
                    };
                }

                var existingItems = await _billItemRepository.GetQueryable(x => x.BillId == request.BillId && x.IsActive && x.MedicineId.HasValue).ToListAsync(cancellationToken);
                if (existingItems.Any() && existingItems.Any(x => !x.MedicineId.HasValue))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Hóa đơn chứa các loại mặt hàng không phải thuốc",
                        Errors = new List<string> { "Hóa đơn chứa các loại mặt hàng không phải thuốc" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = string.Join("; ", validationResults.Select(v => v.ErrorMessage)),
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var groupedMedicineItems = request.MedicineItems
                    .GroupBy(m => m.ItemId)
                    .Select(g => new { ItemId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                    .ToList();

                var currentItems = existingItems.ToDictionary(x => x.MedicineId.Value, x => x);

                try
                {
                    // Xử lý thêm hoặc cập nhật item mới
                    foreach (var groupedItem in groupedMedicineItems.Where(x => x.TotalQuantity > 0))
                    {
                        var (isValid, errorMessage) = await ValidateItem(groupedItem.ItemId, groupedItem.TotalQuantity, false, true, false, cancellationToken);
                        if (!isValid)
                        {
                            return new Response<bool>()
                            {
                                Succeeded = false,
                                Message = errorMessage,
                                Errors = new List<string> { errorMessage }
                            };
                        }

                        if (currentItems.TryGetValue(groupedItem.ItemId, out var currentItem))
                        {
                            // Cập nhật item hiện tại
                            var oldQuantity = currentItem.Stock;
                            if (groupedItem.TotalQuantity != oldQuantity)
                            {
                                bill.Total -= oldQuantity;
                                currentItem.Stock = groupedItem.TotalQuantity;
                                bill.Total += groupedItem.TotalQuantity;
                                _billItemRepository.Update(currentItem);
                            }
                            currentItems.Remove(groupedItem.ItemId); // Đánh dấu đã xử lý
                        }
                        else
                        {
                            // Thêm item mới
                            var billItem = new BillItem
                            {
                                BillId = bill.Id,
                                FoodId = null,
                                MedicineId = groupedItem.ItemId,
                                BreedId = null,
                                Stock = groupedItem.TotalQuantity,
                                IsActive = true
                            };
                            _billItemRepository.Insert(billItem);
                            bill.Total += groupedItem.TotalQuantity;
                        }
                    }

                    // Xử lý xóa item cũ không còn trong request
                    foreach (var currentItem in currentItems.Values)
                    {
                        bill.Total -= currentItem.Stock;
                        currentItem.IsActive = false;
                        _billItemRepository.Update(currentItem);
                    }

                    _billRepository.Update(bill);
                    await _billRepository.CommitAsync(cancellationToken);

                    return new Response<bool>()
                    {
                        Succeeded = true,
                        Message = "Cập nhật hóa đơn với mặt hàng thuốc thành công",
                        Data = true
                    };
                }
                catch (Exception ex)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Lỗi khi cập nhật hóa đơn với mặt hàng thuốc",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật hóa đơn với mặt hàng thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBillBreed(
            Guid billId, UpdateBillBreedDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) return (false, "Dữ liệu yêu cầu là bắt buộc.");
            if (!request.BreedItems.Any()) return (false, "Phải cung cấp danh sách mặt hàng giống.");
            foreach (var it in request.BreedItems)
            {
                if (!(await ValidBreedStock(it.ItemId, it.Quantity)))
                {
                    throw new Exception("Giống không khả dụng hoặc không đủ số lượng");
                }
            }

            var checkError = new Ref<CheckError>();
            var bill = await _billRepository.GetByIdAsync(billId, checkError);
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
        public async Task<Response<PaginationSet<BillResponse>>> GetPaginatedBillListByTechicalStaff(
             ListingRequest request,
             string billType,
             CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var tech = await _userRepository.GetByIdAsync(_currentUserId);
                if (tech == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Nhân viên kỹ thuật không tồn tại",
                        Errors = new List<string> { "Nhân viên kỹ thuật không tồn tại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                var query = _billRepository.GetQueryable(x => x.UserRequestId == tech.Id && x.TypeBill == billType && x.IsActive);

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetByIdAsync(lscInfo.BarnId);
                    var workerInfo = await _userRepository.GetByIdAsync(barnInfo.WorkerId);

                    var userRequestResponse = new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    };
                    var workerResponse = new WokerResponse
                    {
                        Id = workerInfo.Id,
                        FullName = workerInfo.FullName,
                        Email = workerInfo.Email
                    };
                    var barnInfoResponse = new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerResponse
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
                        TypeBill = bill.TypeBill,
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

                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách hóa đơn thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Common func
        protected async Task<bool> ValidBreedStock(Guid breedId, int stock)
        {
            var BreedToValid = await _breedRepository.GetByIdAsync(breedId);
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

        public async Task<Response<PaginationSet<BillResponse>>> GetPaginatedBillListHistory(
            ListingRequest request,
            string billType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                // Lấy hóa đơn theo loại và không thuộc trạng thái REQUESTED và CANCELLED
                var query = _billRepository.GetQueryable(x => x.IsActive && x.TypeBill.Equals(billType)
                && !x.Status.Equals(StatusConstant.REQUESTED) && !x.Status.Equals(StatusConstant.CANCELLED));

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetByIdAsync(lscInfo.BarnId);
                    var workerInfo = await _userRepository.GetByIdAsync(barnInfo.WorkerId);

                    var userRequestResponse = new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    };
                    var workerResponse = new WokerResponse
                    {
                        Id = workerInfo.Id,
                        FullName = workerInfo.FullName,
                        Email = workerInfo.Email
                    };
                    var barnInfoResponse = new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerResponse
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
                        TypeBill = bill.TypeBill,
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

                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách hóa đơn thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<BillResponse>>> GetApprovedBillsByWorker(
     ListingRequest request,
     CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var worker = await _userRepository.GetByIdAsync(_currentUserId);
                if (worker == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại",
                        Errors = new List<string> { "Người gia công không tồn tại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                // Lấy các BarnId hợp lệ của worker
                var validBarnIds = await _barnRepository.GetQueryable(b => b.WorkerId == worker.Id)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);

                // Lấy hóa đơn đã được phê duyệt thuộc các LivestockCircle của các Barn của worker
                var query = from bill in _billRepository.GetQueryable(b => b.IsActive && b.Status.Equals(StatusConstant.APPROVED))
                            join livestockCircle in _livestockCircleRepository.GetQueryable()
                                on bill.LivestockCircleId equals livestockCircle.Id
                            where validBarnIds.Contains(livestockCircle.BarnId)
                            select bill;

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetByIdAsync(lscInfo?.BarnId ?? Guid.Empty);
                    var workerInfo = await _userRepository.GetByIdAsync(barnInfo?.WorkerId ?? Guid.Empty);

                    var userRequestResponse = userRequest != null ? new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    } : null;

                    var workerResponse = workerInfo != null ? new WokerResponse
                    {
                        Id = workerInfo.Id,
                        FullName = workerInfo.FullName,
                        Email = workerInfo.Email
                    } : null;

                    var barnInfoResponse = barnInfo != null ? new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerResponse
                    } : null;

                    var lscInfoResponse = lscInfo != null ? new LivestockCircleBillResponse
                    {
                        Id = lscInfo.Id,
                        BarnDetailResponse = barnInfoResponse,
                        LivestockCircleName = lscInfo.LivestockCircleName
                    } : null;

                    responses.Add(new BillResponse
                    {
                        Id = bill.Id,
                        UserRequest = userRequestResponse,
                        LivestockCircle = lscInfoResponse,
                        Name = bill.Name,
                        Note = bill.Note,
                        Total = bill.Total,
                        TypeBill = bill.TypeBill,
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

                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách hóa đơn được phê duyệt thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn được phê duyệt",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<BillResponse>>> GetHistoryBillsByWorker(
    ListingRequest request,
    CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var worker = await _userRepository.GetByIdAsync(_currentUserId);
                if (worker == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại",
                        Errors = new List<string> { "Người gia công không tồn tại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(Bill).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BillResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                // Lấy các BarnId hợp lệ của worker
                var validBarnIds = await _barnRepository.GetQueryable(b => b.WorkerId == worker.Id)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);

                // Lấy hóa đơn không thuộc các trạng thái REQUESTED, APPROVED, REJECTED, CANCELLED
                var excludedStatuses = new[] { StatusConstant.REQUESTED, StatusConstant.APPROVED, StatusConstant.REJECTED, StatusConstant.CANCELLED };
                var query = from bill in _billRepository.GetQueryable(b => b.IsActive && !excludedStatuses.Contains(b.Status))
                            join livestockCircle in _livestockCircleRepository.GetQueryable()
                                on bill.LivestockCircleId equals livestockCircle.Id
                            where validBarnIds.Contains(livestockCircle.BarnId)
                            select bill;

                if (request.SearchString?.Any() == true) query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true) query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BillResponse>();
                foreach (var bill in paginationResult.Items)
                {
                    var userRequest = await _userRepository.GetByIdAsync(bill.UserRequestId);
                    var lscInfo = await _livestockCircleRepository.GetByIdAsync(bill.LivestockCircleId);
                    var barnInfo = await _barnRepository.GetByIdAsync(lscInfo?.BarnId ?? Guid.Empty);
                    var workerInfo = await _userRepository.GetByIdAsync(barnInfo?.WorkerId ?? Guid.Empty);

                    var userRequestResponse = userRequest != null ? new UserRequestResponse
                    {
                        Id = userRequest.Id,
                        FullName = userRequest.FullName,
                        Email = userRequest.Email
                    } : null;

                    var workerResponse = workerInfo != null ? new WokerResponse
                    {
                        Id = workerInfo.Id,
                        FullName = workerInfo.FullName,
                        Email = workerInfo.Email
                    } : null;

                    var barnInfoResponse = barnInfo != null ? new BarnDetailResponse
                    {
                        Id = barnInfo.Id,
                        Address = barnInfo.Address,
                        BarnName = barnInfo.BarnName,
                        Image = barnInfo.Image,
                        Worker = workerResponse
                    } : null;

                    var lscInfoResponse = lscInfo != null ? new LivestockCircleBillResponse
                    {
                        Id = lscInfo.Id,
                        BarnDetailResponse = barnInfoResponse,
                        LivestockCircleName = lscInfo.LivestockCircleName
                    } : null;

                    responses.Add(new BillResponse
                    {
                        Id = bill.Id,
                        UserRequest = userRequestResponse,
                        LivestockCircle = lscInfoResponse,
                        Name = bill.Name,
                        Note = bill.Note,
                        Total = bill.Total,
                        TypeBill = bill.TypeBill,
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

                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách hóa đơn lịch sử thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BillResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn lịch sử",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
