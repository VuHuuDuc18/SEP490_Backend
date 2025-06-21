using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Services.Interfaces;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request.Barn;
using CloudinaryDotNet.Actions;
using Domain.Dto.Response.Barn;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Infrastructure.Extensions;
using Domain.Extensions;


namespace Infrastructure.Services.Implements
{
    public class BarnService : IBarnService
    {
        private readonly IRepository<Barn> _barnRepository;
        private readonly IRepository<User> _userRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        /// <summary>
        /// Khởi tạo service với repository của Barn và CloudinaryCloudService.
        /// </summary>
        public BarnService(IRepository<Barn> barnRepository, IRepository<User> userRepository, CloudinaryCloudService cloudinaryCloudService)
        {
            _barnRepository = barnRepository ?? throw new ArgumentNullException(nameof(barnRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
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

            var response = new BarnResponse
            {
                Id = barn.Id,
                BarnName = barn.BarnName,
                Address = barn.Address,
                Image = barn.Image,
                WorkerId = barn.WorkerId,
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
                    responses.Add(new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        WorkerId = barn.WorkerId,
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
                    responses.Add(new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        WorkerId = barn.WorkerId,
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

    }
}