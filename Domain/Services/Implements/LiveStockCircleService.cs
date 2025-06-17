
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
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response.LivestockCircle;

namespace Domain.Services.Implements
{
    public class LivestockCircleService : ILivestockCircleService
    {
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;

        /// <summary>
        /// Khởi tạo service với repository của LivestockCircle.
        /// </summary>
        public LivestockCircleService(IRepository<LivestockCircle> livestockCircleRepository)
        {
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
        }

        /// <summary>
        /// Tạo một chu kỳ chăn nuôi mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu chu kỳ chăn nuôi không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            //// Kiểm tra ngày hợp lệ
            //if (request.StartDate > request.EndDate)
            //    return (false, "Ngày bắt đầu không thể muộn hơn ngày kết thúc.");

            // Kiểm tra xem chu kỳ với tên này đã tồn tại chưa
            var checkError = new Ref<CheckError>();
            var exists = await _livestockCircleRepository.CheckExist(
                x => x.LivestockCircleName == request.LivestockCircleName && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra chu kỳ tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Chu kỳ chăn nuôi với tên '{request.LivestockCircleName}' đã tồn tại.");

            var livestockCircle = new LivestockCircle
            {
                LivestockCircleName = request.LivestockCircleName,
                Status = request.Status,
                StartDate = request.StartDate,
                TotalUnit = request.TotalUnit,
                DeadUnit = 0,
                AverageWeight = 0,
                GoodUnitNumber = request.TotalUnit,
                BadUnitNumber = 0,
                BreedId = request.BreedId,
                BarnId = request.BarnId,
                TechicalStaffId = request.TechicalStaffId
            };

            try
            {
                _livestockCircleRepository.Insert(livestockCircle);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo chu kỳ chăn nuôi: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu chu kỳ chăn nuôi không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _livestockCircleRepository.GetById(id, checkError);
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
                x => x.LivestockCircleName == request.LivestockCircleName && x.Id != id && x.IsActive,
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
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(id, checkError);
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
        public async Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(id, checkError);
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
        public async Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetAllAsync(
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

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        public async Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetByTechnicalStaffAsync(
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
    }
}