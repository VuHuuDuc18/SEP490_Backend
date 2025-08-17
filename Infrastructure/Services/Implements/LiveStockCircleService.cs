
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Domain.DTOs.Request.LivestockCircle;
using Domain.DTOs.Request.Order;
using Domain.DTOs.Response.LivestockCircle;
using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Infrastructure.Services.Implements
{
    public class LivestockCircleService : ILivestockCircleService
    {
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ImageBreed> _imageBreedRepository;
        private readonly IRepository<ImageLivestockCircle> _livestockCircleImageRepo;
        private readonly IRepository<LivestockCircleFood> _livestockCircleFoodRepository;
        private readonly IRepository<LivestockCircleMedicine> _livestockCircleMedicineRepository;
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<Medicine> _medicineRepository;
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<ImageFood> _foodImageRepository;
        private readonly IRepository<ImageMedicine> _medicineImageRepository;
        private readonly IRepository<ImageLivestockCircle> _imageLiveStockCircleRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của LivestockCircle.
        /// </summary>

        public LivestockCircleService
            (IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<ImageLivestockCircle> livestockCircleImageRepo,
            IRepository<User> userRepository,
            IRepository<ImageBreed> imageBreedRepository,
            IRepository<Breed> breedRepository,
            IRepository<LivestockCircleFood> livestockCircleFoodRepository,
            IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository,
            IRepository<Food> foodRepository,
            IRepository<Medicine> medicineRepository,
            IRepository<ImageFood> foodImageRepository,
            IRepository<ImageMedicine> medicineImageRepository,
            IRepository<ImageLivestockCircle> imageLiveStockCircleRepository,
            CloudinaryCloudService cloudinaryCloudService
            )
        {
            _livestockCircleRepository = livestockCircleRepository;
            _livestockCircleImageRepo = livestockCircleImageRepo;
            _breedRepository = breedRepository;
            _imageBreedRepository = imageBreedRepository;
            _userRepository = userRepository;
            _livestockCircleFoodRepository = livestockCircleFoodRepository;
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository;
            _foodRepository = foodRepository;
            _medicineRepository = medicineRepository;
            _foodImageRepository = foodImageRepository;
            _medicineImageRepository = medicineImageRepository;
            _cloudinaryCloudService = cloudinaryCloudService;
            _imageLiveStockCircleRepository = imageLiveStockCircleRepository;
        }

        //public async Task<(bool Success, string ErrorMessage)> UpdateLiveStockCircle(Guid livestockCircleId, UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        //{
        //    if (request == null)
        //        return (false, "Dữ liệu chu kỳ chăn nuôi không được null.");

        //    var checkError = new Ref<CheckError>();
        //    var existing = await _livestockCircleRepository.GetByIdAsync(livestockCircleId, checkError);
        //    if (checkError.Value?.IsError == true)
        //        return (false, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

        //    if (existing == null)
        //        return (false, "Không tìm thấy chu kỳ chăn nuôi.");

        //    // Kiểm tra các trường bắt buộc
        //    var validationResults = new List<ValidationResult>();
        //    var validationContext = new ValidationContext(request);
        //    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        //    {
        //        return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
        //    }

        //    // Kiểm tra ngày hợp lệ
        //    if (request.StartDate > request.EndDate)
        //        return (false, "Ngày bắt đầu không thể muộn hơn ngày kết thúc.");

        //    // Kiểm tra xung đột tên với các chu kỳ đang hoạt động khác
        //    var exists = await _livestockCircleRepository.CheckExist(
        //        x => x.LivestockCircleName == request.LivestockCircleName && x.Id != livestockCircleId && x.IsActive,
        //        checkError,
        //        cancellationToken);

        //    if (checkError.Value?.IsError == true)
        //        return (false, $"Lỗi khi kiểm tra chu kỳ tồn tại: {checkError.Value.Message}");

        //    if (exists)
        //        return (false, $"Chu kỳ chăn nuôi với tên '{request.LivestockCircleName}' đã tồn tại.");

        //    try
        //    {
        //        existing.LivestockCircleName = request.LivestockCircleName;
        //        existing.Status = request.Status;
        //        existing.StartDate = request.StartDate;
        //        existing.EndDate = request.EndDate;
        //        existing.TotalUnit = request.TotalUnit;
        //        existing.DeadUnit = request.DeadUnit;
        //        existing.AverageWeight = request.AverageWeight;
        //        existing.GoodUnitNumber = request.GoodUnitNumber;
        //        existing.BadUnitNumber = request.BadUnitNumber;
        //        existing.BreedId = request.BreedId;
        //        existing.BarnId = request.BarnId;
        //        existing.TechicalStaffId = request.TechicalStaffId;

        //        _livestockCircleRepository.Update(existing);
        //        await _livestockCircleRepository.CommitAsync(cancellationToken);
        //        return (true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Lỗi khi cập nhật chu kỳ chăn nuôi: {ex.Message}");
        //    }
        //}

        public async Task<(bool Success, string ErrorMessage)> DisableLiveStockCircle(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

            if (livestockCircle == null)
                return (false, "Không tìm thấy chu kỳ chăn nuôi.");

            try
            {
                livestockCircle.IsActive = false;
                _livestockCircleRepository.Update(livestockCircle);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa chu kỳ chăn nuôi: {ex.Message}");
            }
        }

        public async Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetLiveStockCircleById(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

            if (livestockCircle == null)
                return (null, "Không tìm thấy chu kỳ chăn nuôi.");
            var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == livestockCircle.BreedId).ToListAsync(cancellationToken);

            var breed = await _breedRepository.GetQueryable(x=>x.Id == livestockCircle.BreedId)
                .Include(x=>x.BreedCategory)
                .Select( x => new BreedResponse()
                {
                    Id = x.Id,
                    BreedName = x.BreedName,
                    Stock = x.Stock,
                    ImageLinks = images.Select(x=>x.ImageLink).ToList(),
                    Thumbnail = "",
                    IsActive = x.IsActive,
                    BreedCategory = new BreedCategoryResponse
                    {
                        Id = x.BreedCategory.Id,
                        Name = x.BreedCategory.Name,
                        Description = x.BreedCategory.Description
                    }
                })
                .FirstOrDefaultAsync();
            var response = new LivestockCircleResponse
            {
                Id = livestockCircle.Id,
                LivestockCircleName = livestockCircle.LivestockCircleName,
                Status = livestockCircle.Status,
                StartDate = livestockCircle.StartDate,
                EndDate = livestockCircle.EndDate,
                TotalUnit = livestockCircle.TotalUnit,
                DeadUnit = livestockCircle.DeadUnit,
                AverageWeight = livestockCircle.AverageWeight,
                GoodUnitNumber = livestockCircle.GoodUnitNumber,
                BadUnitNumber = livestockCircle.BadUnitNumber,
                BreedId = livestockCircle.BreedId,
                BarnId = livestockCircle.BarnId,
                TechicalStaffId = livestockCircle.TechicalStaffId,
                IsActive = livestockCircle.IsActive,
                Breed = breed,
                PreSoldDate = livestockCircle.PreSoldDate,
                ReleaseDate = livestockCircle.ReleaseDate,
                SamplePrice = livestockCircle.SamplePrice,
                TechicalStaff = await _userRepository.GetQueryable(x => x.Id == livestockCircle.TechicalStaffId)
                    .Select(x => new UserItemResponse
                    {
                        Id = x.Id,
                        Fullname = x.FullName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber
                    }).FirstOrDefaultAsync(),
            };
            return (response, null);
        }

        public async Task<(LiveStockCircleActive Circle, string ErrorMessage)> GetActiveLiveStockCircleByBarnId(
            Guid barnId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.BarnId == barnId);
                var circle = await query.FirstOrDefaultAsync(cancellationToken);

                if (circle == null)
                    return (null, "Không tìm thấy chu kỳ chăn nuôi đang hoạt động cho chuồng này.");
                var technical = await _userRepository.GetByIdAsync(circle.TechicalStaffId);
                var technicalStaffResponse = new UserItemResponse
                {
                    Email = technical.Email,
                    Id = technical.Id,
                    Fullname = technical.FullName,
                    PhoneNumber = technical.PhoneNumber
                };
                var breed = await _breedRepository.GetByIdAsync(circle.BreedId);
                var images = await _imageBreedRepository.GetQueryable(x => x.BreedId == breed.Id).ToListAsync(cancellationToken);
                var breedResponse = new BreedBillResponse
                {
                    Id = breed.Id,
                    BreedName = breed.BreedName,
                    Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };
                var response = new LiveStockCircleActive
                {
                    Id = circle.Id,
                    LivestockCircleName = circle.LivestockCircleName,
                    Status = circle.Status,
                    StartDate = circle.StartDate,
                    TotalUnit = circle.TotalUnit,
                    DeadUnit = circle.DeadUnit,
                    AverageWeight = circle.AverageWeight,
                    GoodUnitNumber = circle.GoodUnitNumber,
                    BadUnitNumber = circle.BadUnitNumber,
                    Breed = breedResponse,
                    TechicalStaffId = technicalStaffResponse,
                };

                return (response, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chu kỳ chăn nuôi đang hoạt động theo BarnId: {ex.Message}");
            }
        }



        /// <summary>
        /// Cập nhật trọng lượng trung bình (AverageWeight) của một chu kỳ chăn nuôi.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAverageWeight(
            Guid livestockCircleId,
            float averageWeight,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (averageWeight < 0)
                    return (false, "Trọng lượng trung bình không thể âm.");

                var checkError = new Ref<CheckError>();
                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId, checkError);
                if (checkError.Value?.IsError == true)
                    return (false, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

                if (livestockCircle == null)
                    return (false, "Không tìm thấy chu kỳ chăn nuôi.");

                if (!livestockCircle.IsActive)
                    return (false, "Chu kỳ chăn nuôi không còn hoạt động.");

                livestockCircle.AverageWeight = averageWeight;

                _livestockCircleRepository.Update(livestockCircle);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật trọng lượng trung bình: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateImageLiveStocCircle(
            Guid livestockCircleId,
            UpdateImageLiveStockCircle request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu cập nhật hình ảnh lứa nuôi không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }
            try
            {
                //           // Upload thumbnail lên Cloudinary trong folder được chỉ định
                //           if (!string.IsNullOrEmpty(request.Thumbnail))
                //           {
                //               var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                //request.Thumbnail, "food", _cloudinaryCloudService, cancellationToken);

                //               if (!string.IsNullOrEmpty(imageLink))
                //               {
                //                   var imageFood = new ImageFood
                //                   {
                //                       FoodId = food.Id,
                //                       ImageLink = imageLink,
                //                       Thumnail = "true"
                //                   };
                //                   _imageFoodRepository.Insert(imageFood);
                //               }
                //           }

                // Upload ảnh khác lên Cloudinary trong folder được chỉ định
                if (request.Images != null && request.Images.Any())
                {
                    foreach (var imageLink in request.Images)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                           imageLink, "livestockcircle", _cloudinaryCloudService, cancellationToken);
                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageLivestockCircle = new ImageLivestockCircle
                            {
                                LivestockCircleId = livestockCircleId,
                                ImageLink = uploadedLink,
                                Thumnail = "false"
                            };
                            _imageLiveStockCircleRepository.Insert(imageLivestockCircle);
                        }
                    }
                }
                await _imageLiveStockCircleRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thức ăn: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ReleaseBarn(ReleaseBarnRequest req)
        {
            var releaseItem = await _livestockCircleRepository.GetByIdAsync(req.LivestockCircleId);

            if (releaseItem == null) return new Response<bool>()
            {
                Succeeded = false,
                Message = "Không tìm thấy chuồng"
            };
            // BR-3
            if (!releaseItem.Status.Equals(StatusConstant.GROWINGSTAT) && !releaseItem.Status.Equals(StatusConstant.RELEASESTAT))
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi hệ thống. Không thể xuất chuồng trại vào lúc này. Vui lòng thử lại sau"
                };
            }
            // BR
            if ((req.ReleaseDate.Date - DateTime.Now.Date).TotalDays > 15)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Ngày xuất chuồng quá xa"
                };
            }
            // ALT-1
            if (releaseItem.Status.Equals(StatusConstant.RELEASESTAT))
            {
                releaseItem.ReleaseDate = req.ReleaseDate;
                await _livestockCircleRepository.CommitAsync();
                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Ngày xuất chuồng cập nhật :" + req.ReleaseDate.ToString("dd/MM/yyyy"),
                };
            }
            releaseItem.Status = StatusConstant.RELEASESTAT;
            releaseItem.ReleaseDate = req.ReleaseDate;
            await _livestockCircleRepository.CommitAsync();
            return new Response<bool>()
            {
                Succeeded = true,
                Message = "Xuất chuồng thành công vào ngày :" + req.ReleaseDate.ToString("dd/MM/yyyy"),
            };
        }


        public async Task<Response<PaginationSet<LiveStockCircleHistoryItem>>> GetLivestockCircleHistory(Guid barnId, ListingRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<LiveStockCircleHistoryItem>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<LiveStockCircleHistoryItem>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(LivestockCircle).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<LiveStockCircleHistoryItem>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");
                }
                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<LiveStockCircleHistoryItem>>($"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}");
                }
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<LiveStockCircleHistoryItem>>($"Trường lọc không hợp lệ");

                }
                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.BarnId == barnId);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Include(it => it.Barn).Select(c => new LiveStockCircleHistoryItem()
                {
                    Id = c.Id,
                    LivestockCircleName = c.LivestockCircleName,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalUnit = c.TotalUnit,
                    DeadUnit = c.DeadUnit,
                    AverageWeight = c.AverageWeight,
                    BreedId = c.BreedId,
                    BreedName = c.Breed.BreedName
                }).Pagination(request.PageIndex, request.PageSize, request.Sort);



                return new Response<PaginationSet<LiveStockCircleHistoryItem>>()
                {
                    Succeeded = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<LiveStockCircleHistoryItem>>($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<Response<PaginationSet<ReleasedLivetockItem>>> GetReleasedLivestockCircleList(ListingRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<ReleasedLivetockItem>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<ReleasedLivetockItem>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(LivestockCircle).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<ReleasedLivetockItem>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");
                }
                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<ReleasedLivetockItem>>($"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}");
                }
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<ReleasedLivetockItem>>($"Trường lọc không hợp lệ");

                }

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive).Where(it => it.Status.Equals(StatusConstant.RELEASESTAT));

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Include(it => it.Breed).ThenInclude(it => it.BreedCategory).Include(it => it.Barn)
                    .Select(it => new ReleasedLivetockItem
                    {
                        Id = it.BarnId,
                        LivestockCircleId = it.Id,
                        BarnName = it.Barn.BarnName,
                        BreedCategoryName = it.Breed.BreedCategory.Name,
                        BreedName = it.Breed.BreedName,
                        TotalUnit = it.TotalUnit,

                    })
                    .Pagination(request.PageIndex, request.PageSize, request.Sort);


                return new Response<PaginationSet<ReleasedLivetockItem>>()
                {
                    Succeeded = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<Response<Guid>> CreateLiveStockCircle(CreateLivestockCircleRequest request)
        {
            try
            {
                // valid breed
                var breedValidStock = await _breedRepository.GetByIdAsync(request.BreedId);
                if (breedValidStock != null)
                {
                    if (breedValidStock.Stock <= request.TotalUnit)
                    {
                        return new Response<Guid>()
                        {
                            Succeeded = false,
                            Message = "Không đủ số lượng giống"
                        };
                    }
                }
                else
                {
                    return new Response<Guid>("Giống nuôi không khả dụng");

                }

                var LivestockCircleToCreate = new LivestockCircle()
                {
                    AverageWeight = 0,
                    BadUnitNumber = 0,
                    GoodUnitNumber = 0,
                    DeadUnit = 0,
                    BarnId = request.BarnId,
                    BreedId = request.BreedId,
                    TechicalStaffId = request.TechicalStaffId,
                    Status = StatusConstant.PENDINGSTAT,
                    TotalUnit = request.TotalUnit,
                    LivestockCircleName = request.LivestockCircleName,
                    //
                    EndDate = DateTime.Now,
                    StartDate = DateTime.Now,
                };
                _livestockCircleRepository.Insert(LivestockCircleToCreate);
                if (await _livestockCircleRepository.CommitAsync() > 0)
                {
                    return new Application.Wrappers.Response<Guid>()
                    {
                        Succeeded = true,
                        Data = LivestockCircleToCreate.Id
                    };
                }
                else
                {
                    return new Response<Guid>("Không thể tạo lứa mới");
                }

            }
            catch (Exception ex)
            {
                return new Response<Guid>("Dữ liệu nhận được lỗi");
            }
        }

        public async Task<Response<PaginationSet<FoodRemainingResponse>>> GetFoodRemaining(
             Guid liveStockCircleId,
             ListingRequest request,
             CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<FoodRemainingResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(LivestockCircleFood).GetProperties()
                    .Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();

                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }
                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}",
                        Errors = new List<string> { $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(liveStockCircleId);
                if (livestockCircle == null)
                {
                    return new Response<PaginationSet<FoodRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "Chu kỳ chăn nuôi không tồn tại",
                        Errors = new List<string> { "Chu kỳ chăn nuôi không tồn tại" }
                    };
                }

                var query = _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == liveStockCircleId && x.Remaining > 0);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query
                    .Include(x => x.Food)
                    .Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<FoodRemainingResponse>();
                foreach (var item in paginationResult.Items)
                {
                    var images = await _foodImageRepository.GetQueryable(x => x.FoodId == item.FoodId)
                        .ToListAsync(cancellationToken);
                    responses.Add(new FoodRemainingResponse
                    {
                        Id = item.Id,
                        LivestockCircleId = item.LivestockCircleId,
                        Food = new FoodBillResponse
                        {
                            Id = item.Food.Id,
                            FoodName = item.Food.FoodName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        },
                        Remaining = item.Remaining
                    });
                }

                var result = new PaginationSet<FoodRemainingResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<FoodRemainingResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách thực phẩm còn lại thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<FoodRemainingResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thực phẩm còn lại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<MedicineRemainingResponse>>> GetMedicineRemaining(
             Guid liveStockCircleId,
             ListingRequest request,
             CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<MedicineRemainingResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(LivestockCircleMedicine).GetProperties()
                    .Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                     .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();

                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }
                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}",
                        Errors = new List<string> { $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFieldsSearch)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(liveStockCircleId);
                if (livestockCircle == null)
                {
                    return new Response<PaginationSet<MedicineRemainingResponse>>()
                    {
                        Succeeded = false,
                        Message = "Chu kỳ chăn nuôi không tồn tại",
                        Errors = new List<string> { "Chu kỳ chăn nuôi không tồn tại" }
                    };
                }

                var query = _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == liveStockCircleId && x.Remaining > 0);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);
                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query
                    .Include(x => x.Medicine)
                    .Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<MedicineRemainingResponse>();
                foreach (var item in paginationResult.Items)
                {
                    var images = await _medicineImageRepository.GetQueryable(x => x.MedicineId == item.MedicineId)
                        .ToListAsync(cancellationToken);
                    responses.Add(new MedicineRemainingResponse
                    {
                        Id = item.Id,
                        LivestockCircleId = item.LivestockCircleId,
                        Medicine = new MedicineBillResponse
                        {
                            Id = item.Medicine.Id,
                            MedicineName = item.Medicine.MedicineName,
                            Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        },
                        Remaining = item.Remaining
                    });
                }

                var result = new PaginationSet<MedicineRemainingResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<MedicineRemainingResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách thuốc còn lại thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<MedicineRemainingResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thuốc còn lại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> SetPreOrderField(SetPreOrderFieldRequest request)
        {
            try
            {
                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(request.LivestockCircleId);
                if (livestockCircle == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Chu kỳ chăn nuôi không tồn tại",
                        Errors = new List<string> { "Chu kỳ chăn nuôi không tồn tại" }
                    };
                }
                if (livestockCircle.ReleaseDate == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Chu kỳ chăn nuôi chưa có ngày xuất chuồng",
                        Errors = new List<string> { "Chu kỳ chăn nuôi chưa có ngày xuất chuồng" }
                    };
                }
                if (((DateTime)livestockCircle.ReleaseDate).Date < DateTime.Now)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Đã xuất chuồng, không để đặt ngày pre-order",
                        Errors = new List<string> { "Chu kỳ chăn nuôi đã được xuất chuồng" }
                    };
                }
                livestockCircle.PreSoldDate = request.PreOrderDate;
                livestockCircle.SamplePrice = request.SamplePrice;

                _livestockCircleRepository.Update(livestockCircle);
                await _livestockCircleRepository.CommitAsync();

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Chuồng sẽ được mở bán vào ngày " + ((DateTime)request.PreOrderDate).ToString("dd/MM/yyyy"),
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = $"Lỗi khi cập nhật trường đặt trước",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}



