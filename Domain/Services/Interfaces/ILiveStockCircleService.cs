using Domain.Dto.Request;
using Domain.Dto.Request.LivestockCircle;
using Domain.Dto.Response;
using Domain.Dto.Response.LivestockCircle;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ILivestockCircleService
    {
        /// <summary>
        /// Tạo một chu kỳ chăn nuôi mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateLivestockCircleRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateLivestockCircleRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một chu kỳ chăn nuôi bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một chu kỳ chăn nuôi theo ID.
        /// </summary>
        Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả chu kỳ chăn nuôi đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetAllAsync(string status = null, Guid? barnId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetByTechnicalStaffAsync(Guid technicalStaffId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo status.
        /// </summary>
        Task<(PaginationSet<LivestockCircleResponse> Result, string ErrorMessage)> GetPaginatedListByStatusAsync(
            string status,
            ListingRequest request,
            CancellationToken cancellationToken = default);
    }
}