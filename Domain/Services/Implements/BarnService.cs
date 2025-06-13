
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request;
using Domain.Dto.Response;

namespace Services
{
    public class BarnService : IBarnService
    {
        private readonly IRepository<Barn> _barnRepository;

        /// <summary>
        /// Khởi tạo service với repository của Barn.
        /// </summary>
        public BarnService(IRepository<Barn> barnRepository)
        {
            _barnRepository = barnRepository ?? throw new ArgumentNullException(nameof(barnRepository));
        }

        /// <summary>
        /// Tạo một chuồng trại mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateBarnRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu chuồng trại không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem chuồng trại với tên này đã tồn tại chưa
            var checkError = new Ref<CheckError>();
            var exists = await _barnRepository.CheckExist(
                x => x.BarnName == request.BarnName && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chuồng trại tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chuồng trại với tên '{request.BarnName}' đã tồn tại.");

            var barn = new Barn
            {
                BarnName = request.BarnName,
                Address = request.Address,
                Image = request.Image,
                WorkerId = request.WorkerId
            };

            try
            {
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
        /// Cập nhật thông tin một chuồng trại.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateBarnRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu chuồng trại không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _barnRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin chuồng trại: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy chuồng trại.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các chuồng trại đang hoạt động khác
            var exists = await _barnRepository.CheckExist(
                x => x.BarnName == request.BarnName && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chuồng trại tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chuồng trại với tên '{request.BarnName}' đã tồn tại.");

            try
            {
                existing.BarnName = request.BarnName;
                existing.Address = request.Address;
                existing.Image = request.Image;
                existing.WorkerId = request.WorkerId;

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
        public async Task<(List<BarnResponse> Barns, string ErrorMessage)> GetAllAsync(
            string barnName = null,
            Guid? workerId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _barnRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(barnName))
                    query = query.Where(x => x.BarnName.Contains(barnName));

                if (workerId.HasValue)
                    query = query.Where(x => x.WorkerId == workerId.Value);

                var barns = await query.ToListAsync(cancellationToken);
                var responses = barns.Select(b => new BarnResponse
                {
                    Id = b.Id,
                    BarnName = b.BarnName,
                    Address = b.Address,
                    Image = b.Image,
                    WorkerId = b.WorkerId,
                    IsActive = b.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách chuồng trại: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của công nhân.
        /// </summary>
        public async Task<(List<BarnResponse> Barns, string ErrorMessage)> GetByWorkerAsync(
            Guid workerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var barns = await _barnRepository
                    .GetQueryable(x => x.WorkerId == workerId && x.IsActive)
                    .ToListAsync(cancellationToken);
                var responses = barns.Select(b => new BarnResponse
                {
                    Id = b.Id,
                    BarnName = b.BarnName,
                    Address = b.Address,
                    Image = b.Image,
                    WorkerId = b.WorkerId,
                    IsActive = b.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách chuồng trại: {ex.Message}");
            }
        }
    }
}