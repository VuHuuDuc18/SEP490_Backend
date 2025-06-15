using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services
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
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateBarnRequest requestDto, CancellationToken cancellationToken = default)
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
                    var base64Data = ExtractBase64Data(requestDto.Image);
                    if (string.IsNullOrEmpty(base64Data))
                        return (false, "Dữ liệu ảnh không đúng định dạng base64.");

                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                    var imageLink = await _cloudinaryCloudService.UploadImage(requestDto.Image, "barn", cancellationToken);
                    File.Delete(tempFilePath);

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
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateBarnRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu chuồng trại không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _barnRepository.GetById(id, checkError);
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
                x => x.BarnName == requestDto.BarnName && x.Address == requestDto.Address && x.Id != id && x.IsActive,
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

                    var base64Data = ExtractBase64Data(requestDto.Image);
                    if (string.IsNullOrEmpty(base64Data))
                        return (false, "Dữ liệu ảnh không đúng định dạng base64.");

                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(base64Data));
                    var imageLink = await _cloudinaryCloudService.UploadImage(requestDto.Image, "barn", cancellationToken);
                    File.Delete(tempFilePath);

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
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var barn = await _barnRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");

            if (barn == null)
                return (false, "Không tìm thấy chuồng trại.");

            try
            {
                barn.IsActive = false;
                if (!string.IsNullOrEmpty(barn.Image))
                {
                    await _cloudinaryCloudService.DeleteImage(barn.Image, cancellationToken);
                }
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
        public async Task<(BarnResponse Barn, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var barn = await _barnRepository.GetById(id, checkError);
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
        /// Lấy danh sách tất cả chuồng trại đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<BarnResponse> Barns, string ErrorMessage)> GetAllAsync(string barnName = null, Guid? workerId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _barnRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(barnName))
                    query = query.Where(x => x.BarnName.Contains(barnName));

                if (workerId.HasValue)
                    query = query.Where(x => x.WorkerId == workerId.Value);

                var barns = await query.ToListAsync(cancellationToken);
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
                return (null, $"Lỗi khi lấy danh sách chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của người gia công.
        /// </summary>
        public async Task<(List<BarnResponse> Barns, string ErrorMessage)> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Trích xuất phần dữ liệu base64 từ chuỗi (bỏ qua phần header như data:image/jpeg;base64).
        /// </summary>
        private string ExtractBase64Data(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            var parts = base64String.Split(',');
            if (parts.Length < 2)
                return null;

            return parts[1];
        }
    }
}