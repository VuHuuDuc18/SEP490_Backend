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
        public Task<Guid> CreateLiveStockCircle(CreateLivestockCircleRequest request);

        /// <summary>
        /// Cập nhật thông tin một chu kỳ chăn nuôi.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateLiveStockCircle(
            Guid livestockCircleId, 
            UpdateLivestockCircleRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một chu kỳ chăn nuôi bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableLiveStockCircle(
            Guid livestockCircleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một chu kỳ chăn nuôi theo ID.
        /// </summary>
        Task<(LivestockCircleResponse Circle, string ErrorMessage)> GetLiveStockCircleById(
            Guid livestockCircleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả chu kỳ chăn nuôi đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByBarnIdAndStatus(
            string status = null, 
            Guid? barnId = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo ID của nhân viên kỹ thuật.
        /// </summary>
        Task<(List<LivestockCircleResponse> Circles, string ErrorMessage)> GetLiveStockCircleByTechnicalStaff(
            Guid technicalStaffId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chu kỳ chăn nuôi theo status.
        /// </summary>
        Task<(PaginationSet<LivestockCircleResponse> Result, string ErrorMessage)> GetPaginatedListByStatus(
            string status,
            ListingRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thay đổi trung bình cân của một chu kỳ chăn nuôi.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAverageWeight(
            Guid livestockCircleId,
            float averageWeight,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thay đổi trạng thái (Status) của một chu kỳ chăn nuôi.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> ChangeStatus(
            Guid livestockCircleId,
            string status,
            CancellationToken cancellationToken = default);

        // cap nhat trang thai xuat chuong cho 1 lua nuoi
        public Task<bool> ReleaseBarn(Guid id);
        //lay danh sach cac chuong duoc giao quan ly
        public Task<PaginationSet<LivestockCircleResponse>> GetAssignedBarn(Guid tsid,ListingRequest req);
        // danh sach lich su chan nuoi cua 1 chuong
        public Task<PaginationSet<LiveStockCircleHistoryItem>> GetLivestockCircleHistory(Guid barnId,ListingRequest req);

    }
}