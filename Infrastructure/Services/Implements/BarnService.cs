using Application.Wrappers;
using CloudinaryDotNet.Actions;
using Domain.Dto.Request;
using Domain.Dto.Request.Barn;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.LivestockCircle;
using Domain.Helper;
using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Infrastructure.Services.Implements
{
    public class BarnService : IBarnService
    {
        private readonly IRepository<Barn> _barnRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ImageLivestockCircle> _imageLiveStockCircleRepository;
        private readonly IRepository<ImageBreed> _imageBreedeRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        public BarnService(
            IRepository<Barn> barnRepository,
            IRepository<User> userRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<ImageLivestockCircle> imageLiveStockCircleRepository,
            IRepository<ImageBreed> imageBreedeRepository,
            CloudinaryCloudService cloudinaryCloudService)
        {
            _barnRepository = barnRepository ?? throw new ArgumentNullException(nameof(barnRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
            _imageLiveStockCircleRepository = imageLiveStockCircleRepository?? throw new ArgumentNullException(nameof(imageLiveStockCircleRepository));
            _imageBreedeRepository = imageBreedeRepository ?? throw new ArgumentNullException(nameof(imageBreedeRepository));
        }

        /// <summary>
        /// Tạo một chuồng trại mới với kiểm tra hợp lệ, bao gồm upload ảnh lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateBarn(CreateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu chuồng trại không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var checkError = new Ref<CheckError>();
            var exists = await _barnRepository.CheckExist(
                x => x.BarnName == requestDto.BarnName && x.Address == requestDto.Address && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chuồng trại tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chuồng trại với tên '{requestDto.BarnName}' và địa chỉ '{requestDto.Address}' đã tồn tại.");

            var worker = await _userRepository.GetById(requestDto.WorkerId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin người gia công: {checkError.Value.Message}");
            if (worker == null)
                return (false, "Người gia công không tồn tại.");

            var barn = new Barn
            {
                BarnName = requestDto.BarnName,
                Address = requestDto.Address,
                WorkerId = requestDto.WorkerId
            };

            try
            {
                if (!string.IsNullOrEmpty(requestDto.Image))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
     requestDto.Image, "barn", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        barn.Image = imageLink;
                    }
                }

                _barnRepository.Insert(barn);
                await _barnRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một chuồng trại, bao gồm upload ảnh lên Cloudinary trong folder được chỉ định.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateBarn(Guid BarnId, UpdateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu chuồng trại không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _barnRepository.GetById(BarnId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy chuồng trại.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var exists = await _barnRepository.CheckExist(
                x => x.BarnName == requestDto.BarnName && x.Address == requestDto.Address && x.Id != BarnId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chuồng trại tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chuồng trại với tên '{requestDto.BarnName}' và địa chỉ '{requestDto.Address}' đã tồn tại.");

            var worker = await _userRepository.GetById(requestDto.WorkerId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin người gia công: {checkError.Value.Message}");
            if (worker == null)
                return (false, "Người gia công không tồn tại.");

            try
            {
                existing.BarnName = requestDto.BarnName;
                existing.Address = requestDto.Address;
                existing.WorkerId = requestDto.WorkerId;

                if (!string.IsNullOrEmpty(requestDto.Image))
                {
                    if (!string.IsNullOrEmpty(existing.Image))
                    {

                        await _cloudinaryCloudService.DeleteImage(existing.Image, cancellationToken);
                    }

                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
requestDto.Image, "barn", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        existing.Image = imageLink;
                    }
                }

                _barnRepository.Update(existing);
                await _barnRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một chuồng trại bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DisableBarn(Guid BarnId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var barn = await _barnRepository.GetById(BarnId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");

            if (barn == null)
                return (false, "Không tìm thấy chuồng trại.");

            try
            {
                barn.IsActive = !barn.IsActive;
                //if (!string.IsNullOrEmpty(barn.Image))
                //{
                //    await _cloudinaryCloudService.DeleteImage(barn.Image, cancellationToken);
                //}
                _barnRepository.Update(barn);
                await _barnRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một chuồng trại theo ID.
        /// </summary>
        public async Task<(BarnResponse Barn, string ErrorMessage)> GetBarnById(Guid BarnId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var barn = await _barnRepository.GetById(BarnId);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");

            if (barn == null)
                return (null, "Không tìm thấy chuồng trại.");

            var wokerResponse = new WokerResponse()
            {
                Id = barn.Worker.Id,
                FullName = barn.Worker.FullName,
                Email = barn.Worker.Email
            };

            var response = new BarnResponse
            {
                Id = barn.Id,
                BarnName = barn.BarnName,
                Address = barn.Address,
                Image = barn.Image,
                Worker = wokerResponse,
                IsActive = barn.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của người gia công.
        /// </summary>
        public async Task<(List<BarnResponse> Barns, string ErrorMessage)> GetBarnByWorker(Guid workerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkError = new Ref<CheckError>();
                var worker = await _userRepository.GetById(workerId, checkError);
                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin người gia công: {checkError.Value.Message}");
                if (worker == null)
                    return (null, "Người gia công không tồn tại.");

                var barns = await _barnRepository.GetQueryable(x => x.WorkerId == workerId && x.IsActive).ToListAsync(cancellationToken);
                var responses = new List<BarnResponse>();
                foreach (var barn in barns)
                {
                    var wokerResponse = new WokerResponse()
                    {
                        Id = barn.Worker.Id,
                        FullName = barn.Worker.FullName,
                        Email = barn.Worker.Email
                    };

                    responses.Add(new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = wokerResponse,
                        IsActive = barn.IsActive
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách chuồng trại theo người gia công: {ex.Message}");
            }
        }
        /// <summary>
        /// Lấy danh sách chuồng trại phân trang cho người gia công.
        /// </summary>
        public async Task<(PaginationSet<BarnResponse> Result, string ErrorMessage)> GetPaginatedBarnList(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Barn).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _barnRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<BarnResponse>();
                foreach (var barn in paginationResult.Items)
                {
                    var wokerResponse = new WokerResponse()
                    {
                        Id = barn.Worker.Id,
                        FullName = barn.Worker.FullName,
                        Email = barn.Worker.Email
                    };

                    responses.Add(new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = wokerResponse,
                        IsActive = barn.IsActive
                    });
                }

                var result = new PaginationSet<BarnResponse>
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
                return (null, $"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách chuồng trại phân trang cho admin, bao gồm trạng thái có LivestockCircle đang hoạt động hay không
        /// </summary>
        public async Task<(PaginationSet<AdminBarnResponse> Result, string ErrorMessage)> GetPaginatedAdminBarnListAsync(
            ListingRequest request, CancellationToken cancellationToken = default)
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
                var validFields = typeof(Barn).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                // Lấy danh sách chuồng trại đang hoạt động
                var query = _barnRepository.GetQueryable(x => x.IsActive);

                // Áp dụng tìm kiếm nếu có chuỗi tìm kiếm
                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                // Áp dụng bộ lọc nếu có
                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                // Phân trang kết quả
                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                // Lấy danh sách LivestockCircle đang hoạt động
                var activeLivestockCircles = await _livestockCircleRepository
                    .GetQueryable(x => x.IsActive && x.Status != "Hủy" && x.Status != "Hoàn thành")
                    .ToListAsync(cancellationToken);

                var responses = new List<AdminBarnResponse>();
                foreach (var barn in paginationResult.Items)
                {
                    var workerResponse = new WokerResponse
                    {
                        Id = barn.Worker.Id,
                        FullName = barn.Worker.FullName,
                        Email = barn.Worker.Email
                    };

                    // Kiểm tra xem chuồng trại có LivestockCircle đang hoạt động hay không
                    bool hasActiveLivestockCircle = activeLivestockCircles.Any(lc => lc.BarnId == barn.Id);

                    responses.Add(new AdminBarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = workerResponse,
                        IsActive = barn.IsActive,
                        HasActiveLivestockCircle = hasActiveLivestockCircle
                    });
                }

                // Tạo đối tượng phân trang trả về
                var result = new PaginationSet<AdminBarnResponse>
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
                return (null, $"Lỗi khi lấy danh sách chuồng trại cho admin: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy chi tiết chuồng trại cho admin, bao gồm thông tin LivestockCircle đang hoạt động (nếu có)
        /// </summary>
        public async Task<(AdminBarnDetailResponse Barn, string ErrorMessage)> GetAdminBarnDetailAsync(
            Guid barnId, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkError = new Ref<CheckError>();
                var barn = await _barnRepository.GetQueryable(x => x.Id == barnId)
                    .Include(x => x.Worker)
                    .FirstOrDefaultAsync(cancellationToken);

                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");
                if (barn == null)
                    return (null, "Không tìm thấy chuồng trại.");

                var workerResponse = new WokerResponse
                {
                    Id = barn.Worker.Id,
                    FullName = barn.Worker.FullName,
                    Email = barn.Worker.Email
                };

                // Lấy LivestockCircle đang hoạt động (nếu có)
                var activeLivestockCircle = await _livestockCircleRepository
                    .GetQueryable(x => x.BarnId == barnId && x.IsActive && x.Status != "Hủy" && x.Status != "Hoàn thành")
                    .FirstOrDefaultAsync(cancellationToken);

                LivestockCircleResponse? activeLivestockCircleResponse = null;
                if (activeLivestockCircle != null)
                {
                    activeLivestockCircleResponse = new LivestockCircleResponse
                    {
                        Id = activeLivestockCircle.Id,
                        LivestockCircleName = activeLivestockCircle.LivestockCircleName,
                        Status = activeLivestockCircle.Status,
                        StartDate = activeLivestockCircle.StartDate,
                        EndDate = activeLivestockCircle.EndDate,
                        TotalUnit = activeLivestockCircle.TotalUnit,
                        DeadUnit = activeLivestockCircle.DeadUnit,
                        AverageWeight = activeLivestockCircle.AverageWeight,
                        GoodUnitNumber = activeLivestockCircle.GoodUnitNumber,
                        BadUnitNumber = activeLivestockCircle.BadUnitNumber,
                        BreedId = activeLivestockCircle.BreedId,
                        TechicalStaffId = activeLivestockCircle.TechicalStaffId,
                        BarnId = activeLivestockCircle.BarnId,
                        IsActive = activeLivestockCircle.IsActive
                        
                    };
                }

                var response = new AdminBarnDetailResponse
                {
                    Id = barn.Id,
                    BarnName = barn.BarnName,
                    Address = barn.Address,
                    Image = barn.Image,
                    Worker = workerResponse,
                    IsActive = barn.IsActive,
                    ActiveLivestockCircle = activeLivestockCircleResponse
                };

                return (response, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chi tiết chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách phân trang chuồng trại đang có sẵn cho khách hàng, bao gồm thông tin LivestockCircle đang hoạt động
        /// </summary>
        public async Task<Response<PaginationSet<ReleaseBarnResponse>>> GetPaginatedReleaseBarnListAsync(
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<ReleaseBarnResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<ReleaseBarnResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(ReleaseBarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<ReleaseBarnResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<ReleaseBarnResponse>>($"Trường sắp xếp không hợp lệ: {request.Sort?.Field}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                }

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive)
                    .Include(x => x.Barn)
                    .Include(x => x.Breed)
                    .ThenInclude(x => x.BreedCategory)
                    .Select(x => new ReleaseBarnResponse()
                    {
                        Id = x.Barn.Id,
                        BarnName = x.Barn.BarnName,
                        Address = x.Barn.Address,
                        Image = x.Barn.Image,

                        TotalUnit = x.TotalUnit,
                        DeadUnit = x.DeadUnit,
                        GoodUnitNumber = x.GoodUnitNumber,
                        BadUnitNumber = x.BadUnitNumber,
                        AverageWeight = x.AverageWeight,
                        BreedCategory = x.Breed.BreedCategory.Name,
                        Breed = x.Breed.BreedName,
                        StartDate = x.StartDate,
                        ReleaseDate = x.ReleaseDate,
                        Age = (DateTime.UtcNow - (DateTime)x.StartDate).Days < 0 ? 0 : (DateTime.UtcNow - (DateTime)x.StartDate).Days
                    });

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);
                
                return new Response<PaginationSet<ReleaseBarnResponse>>(paginationResult, "Lấy dữ liệu thành công.");
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<ReleaseBarnResponse>>($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<Response<ReleaseBarnDetailResponse>> GetReleaseBarnDetail(
            Guid BarnId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                //get current live stock circle
                var liveStockCircle = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.Barn.Id == BarnId && x.Status == StatusConstant.RELEASESTAT).FirstOrDefault();
                if (liveStockCircle == null)
                {
                    return new Response<ReleaseBarnDetailResponse>("Không tìm thấy thông tin chuồng nuôi.");
                }
                //get livestock circle images
                var circleImages = _imageLiveStockCircleRepository.GetQueryable(x=>x.IsActive && x.LivestockCircleId == liveStockCircle.Id)
                    .Select(x=> AutoMapperHelper.AutoMap<ImageLivestockCircle, ImageLivestockCircleResponse>(x))
                    .ToList();
                //get breed images
                var breedImages = _imageBreedeRepository.GetQueryable(x => x.IsActive && x.BreedId == liveStockCircle.BreedId).Select(x=> x.ImageLink).ToList();
                //map data to response object
                var result = AutoMapperHelper.AutoMap<Barn,ReleaseBarnDetailResponse>(liveStockCircle.Barn);
                result.LiveStockCircle = AutoMapperHelper.AutoMap<LivestockCircle, LivestockCircleResponse>(liveStockCircle);
                result.Breed = AutoMapperHelper.AutoMap<Breed, BreedResponse>(liveStockCircle.Breed);
                result.Breed.Thumbnail = breedImages.FirstOrDefault();
                result.Breed.BreedCategory = AutoMapperHelper.AutoMap<BreedCategory, BreedCategoryResponse>(liveStockCircle.Breed.BreedCategory);
                result.LiveStockCircle.Images = circleImages;
                result.Breed.ImageLinks = breedImages;

                return new Response<ReleaseBarnDetailResponse>(result, "Lấy thông tin thành công.");
            }
            catch (Exception e)
            {
                return new Response<ReleaseBarnDetailResponse>("Không thế lấy thông tin chuồng nuôi.")
                {
                    Errors = new List<string>(){
                        e.Message
                    }
                };

            }
            
        }

        public async Task<Response<PaginationSet<BarnResponse>>> GetAssignedBarn(Guid tsid, ListingRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<BarnResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<BarnResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(ReleaseBarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<BarnResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BarnResponse>>($"Trường sắp xếp không hợp lệ: {request.Sort?.Field}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                }

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive)
                    .Include(x => x.Barn)
                    .ThenInclude(x=>x.Worker)
                    .Select(x => new BarnResponse()
                    {
                        Id = x.Barn.Id,
                        BarnName = x.Barn.BarnName,
                        Address = x.Barn.Address,
                        Image = x.Barn.Image,
                        IsActive = x.Barn.IsActive,
                        Worker = new WokerResponse()
                        {
                            Id= x.Barn.Worker.Id,
                            Email = x.Barn.Worker.Email,
                            FullName = x.Barn.Worker.FullName,
                        }
                        
                    });

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                return new Response<PaginationSet<BarnResponse>>(paginationResult, "Lấy dữ liệu thành công.");
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BarnResponse>>($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }
    }
}