
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Infrastructure.Extensions;
using Domain.Helper.Constants;
using Domain.Dto.Response.Barn;
using Domain.IServices;
using Domain.Dto.Request.Bill.Admin;
using Domain.Dto.Response.User;
using Domain.Dto.Response.Bill;

namespace Infrastructure.Services.Implements
{
    public class LivestockCircleService : ILivestockCircleService
    {
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ImageBreed> _imageBreedRepository;
        private readonly IRepository<ImageLivestockCircle> _livestockCircleImageRepo;
        private readonly IRepository<Breed> _breedRepository;

        /// <summary>
        /// Khởi tạo service với repository của LivestockCircle.
        /// </summary>

        public LivestockCircleService
            (IRepository<LivestockCircle> livestockCircleRepository, 
            IRepository<ImageLivestockCircle> livestockCircleImageRepo,
            IRepository<User> userRepository,
            IRepository<ImageBreed> imageBreedRepository,
            IRepository<Breed> breedRepository)
        {
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _livestockCircleImageRepo = livestockCircleImageRepo;
            _breedRepository = breedRepository;
            _imageBreedRepository = imageBreedRepository;
            _userRepository = userRepository;

        }

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateLiveStockCircle(Guid livestockCircleId, UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu chu kỳ chăn nuôi không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy chu kỳ chăn nuôi.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra ngày hợp lệ
            if (request.StartDate > request.EndDate)
                return (false, "Ngày bắt đầu không thể muộn hơn ngày kết thúc.");

            // Kiểm tra xung đột tên với các chu kỳ đang hoạt động khác
            var exists = await _livestockCircleRepository.CheckExist(
                x => x.LivestockCircleName == request.LivestockCircleName && x.Id != livestockCircleId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chu kỳ tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chu kỳ chăn nuôi với tên '{request.LivestockCircleName}' đã tồn tại.");

            try
            {
                existing.LivestockCircleName = request.LivestockCircleName;
                existing.Status = request.Status;
                existing.StartDate = request.StartDate;
                existing.EndDate = request.EndDate;
                existing.TotalUnit = request.TotalUnit;
                existing.DeadUnit = request.DeadUnit;
                existing.AverageWeight = request.AverageWeight;
                existing.GoodUnitNumber = request.GoodUnitNumber;
                existing.BadUnitNumber = request.BadUnitNumber;
                existing.BreedId = request.BreedId;
                existing.BarnId = request.BarnId;
                existing.TechicalStaffId = request.TechicalStaffId;

                _livestockCircleRepository.Update(existing);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật chu kỳ chăn nuôi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một chu kỳ chăn nuôi bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DisableLiveStockCircle(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
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

        /// <summary>
        /// Lấy thông tin một chu kỳ chăn nuôi theo ID.
        /// </summary>
        public async Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetLiveStockCircleById(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin chu kỳ chăn nuôi: {checkError.Value.Message}");

            if (livestockCircle == null)
                return (null, "Không tìm thấy chu kỳ chăn nuôi.");

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
                IsActive = livestockCircle.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả chu kỳ chăn nuôi đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByBarnIdAndStatus(
            string status = null,
            Guid? barnId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(x => x.Status == status);

                if (barnId.HasValue)
                    query = query.Where(x => x.BarnId == barnId.Value);

                var circles = await query.ToListAsync(cancellationToken);
                var responses = circles.Select(c => new LivestockCircleResponse
                {
                    Id = c.Id,
                    LivestockCircleName = c.LivestockCircleName,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalUnit = c.TotalUnit,
                    DeadUnit = c.DeadUnit,
                    AverageWeight = c.AverageWeight,
                    GoodUnitNumber = c.GoodUnitNumber,
                    BadUnitNumber = c.BadUnitNumber,
                    BreedId = c.BreedId,
                    BarnId = c.BarnId,
                    TechicalStaffId = c.TechicalStaffId,
                    IsActive = c.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách chu kỳ chăn nuôi: {ex.Message}");
            }
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
                var technical = await _userRepository.GetById(circle.TechicalStaffId);
                var technicalStaffResponse = new UserItemResponse
                {
                    Email = technical.Email,
                    UserId = technical.Id,
                    Fullname = technical.FullName,
                    PhoneNumber = technical.PhoneNumber
                };
                var breed = await _breedRepository.GetById(circle.BreedId);
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
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        public async Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByTechnicalStaff(
            Guid technicalStaffId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var circles = await _livestockCircleRepository
                    .GetQueryable(x => x.TechicalStaffId == technicalStaffId && x.IsActive)
                    .ToListAsync(cancellationToken);
                var responses = circles.Select(c => new LivestockCircleResponse
                {
                    Id = c.Id,
                    LivestockCircleName = c.LivestockCircleName,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalUnit = c.TotalUnit,
                    DeadUnit = c.DeadUnit,
                    AverageWeight = c.AverageWeight,
                    GoodUnitNumber = c.GoodUnitNumber,
                    BadUnitNumber = c.BadUnitNumber,
                    BreedId = c.BreedId,
                    BarnId = c.BarnId,
                    TechicalStaffId = c.TechicalStaffId,
                    IsActive = c.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách chu kỳ chăn nuôi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách phân trang các chu kỳ chăn nuôi với lọc trực tiếp theo Status, 
        /// cùng với tìm kiếm và lọc bổ sung từ ListingRequest.
        /// </summary>
        public async Task<(PaginationSet<LivestockCircleResponse> Result, string ErrorMessage)> GetPaginatedListByStatus(
            string status,
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(LivestockCircle).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive);

                // Áp dụng lọc theo status nếu có
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(x => x.Status == status);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(c => new LivestockCircleResponse
                {
                    Id = c.Id,
                    LivestockCircleName = c.LivestockCircleName,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    TotalUnit = c.TotalUnit,
                    DeadUnit = c.DeadUnit,
                    AverageWeight = c.AverageWeight,
                    GoodUnitNumber = c.GoodUnitNumber,
                    BadUnitNumber = c.BadUnitNumber,
                    BreedId = c.BreedId,
                    BarnId = c.BarnId,
                    TechicalStaffId = c.TechicalStaffId,
                    IsActive = c.IsActive
                }).ToList();

                var result = new PaginationSet<LivestockCircleResponse>
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
                var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
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

        public async Task<bool> ReleaseBarn(Guid id)
        {
            var releaseItem = await _livestockCircleRepository.GetById(id);

            if (releaseItem == null) throw new Exception("Không tìm thấy chu kỳ chăn nuôi.");
            // BR-6
            if (releaseItem.Status.Equals(StatusConstant.DONESTAT))
            {
                throw new Exception("Không thể thay đổi trạng thái");
            }
            releaseItem.Status = StatusConstant.RELEASESTAT;
            return await _livestockCircleRepository.CommitAsync() > 0;
        }

        public async Task<PaginationSet<LivestockCircleResponse>> GetAssignedBarn(Guid tsid, ListingRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    throw new Exception("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Barn).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    throw new Exception($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.TechicalStaffId == tsid);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<LivestockCircleResponse>();
                foreach (var c in paginationResult.Items)
                {
                    responses.Add(new LivestockCircleResponse
                    {
                        Id = c.Id,
                        LivestockCircleName = c.LivestockCircleName,
                        Status = c.Status,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        TotalUnit = c.TotalUnit,
                        DeadUnit = c.DeadUnit,
                        AverageWeight = c.AverageWeight,
                        GoodUnitNumber = c.GoodUnitNumber,
                        BadUnitNumber = c.BadUnitNumber,
                        BreedId = c.BreedId,
                        BarnId = c.BarnId,
                        TechicalStaffId = c.TechicalStaffId,
                        IsActive = c.IsActive
                    });
                }

                var result = new PaginationSet<LivestockCircleResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<PaginationSet<LiveStockCircleHistoryItem>> GetLivestockCircleHistory(Guid barnId, ListingRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    throw new Exception("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Barn).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    throw new Exception($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

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



                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<PaginationSet<ReleasedLivetockItem>> GetReleasedLivestockCircleList(ListingRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    throw new Exception("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(Barn).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    throw new Exception($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive).Where(it=>it.Status.Equals(StatusConstant.RELEASESTAT));

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Include(it => it.Breed).ThenInclude(it => it.BreedCategory).Include(it => it.Barn)
                    .Select(it => new ReleasedLivetockItem
                    {
                        LivestockCircleId = it.Id,
                        BarnName = it.Barn.BarnName,
                        BreedCategoryName = it.Breed.BreedCategory.Name,
                        BreedName = it.Breed.BreedName,
                        TotalUnit = it.TotalUnit,

                    })
                    .Pagination(request.PageIndex, request.PageSize, request.Sort);


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        async Task<Guid> ILivestockCircleService.CreateLiveStockCircle(CreateLivestockCircleRequest request)
        {
            try
            {
                // valid breed
                var breedValidStock = await _breedRepository.GetById(request.BreedId);
                if (breedValidStock != null)
                {
                    if (breedValidStock.Stock <= request.TotalUnit)
                    {
                        throw new Exception("Không đủ số lượng giống");
                    }
                }
                else
                {
                    throw new Exception("Giống nuôi không khả dụng");

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
                    return LivestockCircleToCreate.Id;
                }
                else
                {
                    throw new Exception("Không thể tạo lứa mới");
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Dữ liệu nhận được lỗi");
            }
        }
        public async Task<ReleasedLivetockDetail> GetReleasedLivestockCircleById(Guid livestockCircleId)
        {
            try
            {
                var livestockCircleData = await _livestockCircleRepository.GetQueryable(it => it.IsActive)
                    .Include(it => it.Breed).ThenInclude(it => it.BreedCategory)
                    .Include(it => it.Barn).ThenInclude(it => it.Worker)
                    .FirstOrDefaultAsync(it => it.Id == livestockCircleId && it.Status.Equals(StatusConstant.RELEASESTAT));
                ReleasedLivetockDetail result = new ReleasedLivetockDetail()
                {
                    AverageWeight = livestockCircleData.AverageWeight,
                    BadUnitNumber = livestockCircleData.BadUnitNumber,
                    EndDate = livestockCircleData.EndDate,
                    StartDate = livestockCircleData.StartDate,
                    BreedName = livestockCircleData.Breed.BreedName,
                    BreedCategoryName = livestockCircleData.Breed.BreedCategory.Name,
                    GoodUnitNumber = livestockCircleData.GoodUnitNumber,
                    TotalUnit = livestockCircleData.TotalUnit,
                    LivestockCircleId = livestockCircleData.Id,
                    BarnDetail = new BarnResponse()
                    {
                        Id = livestockCircleData.BarnId,
                        Address = livestockCircleData.Barn.Address,
                        Image = livestockCircleData.Barn.Image,
                        BarnName = livestockCircleData.Barn.BarnName,
                        IsActive = livestockCircleData.Barn.IsActive,
                        Worker = new WokerResponse()
                        {
                            Id = livestockCircleData.Barn.WorkerId,
                            Email = livestockCircleData.Barn.Worker.Email,
                            FullName = livestockCircleData.Barn.Worker.FullName,
                        }
                    }
                };
                return result;

            }
            catch (Exception)
            {
                throw new Exception("Mã ID không hợp lệ");
            }
        }
    }
}



