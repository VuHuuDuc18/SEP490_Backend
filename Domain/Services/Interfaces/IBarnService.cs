﻿using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Request.Barn;
using Domain.Dto.Response.Barn;
using Domain.Dto.Request;
using Domain.Dto.Response.Medicine;
using Domain.Dto.Response;
namespace Domain.Services.Interfaces
{
    public interface IBarnService
    {
        /// <summary>
        /// Tạo một chuồng trại mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateBarn(CreateBarnRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một chuồng trại.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateBarn(Guid BarnId, UpdateBarnRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một chuồng trại bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableBarn(Guid BarnId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một chuồng trại theo ID.
        /// </summary>
        Task<(BarnResponse Barn, string ErrorMessage)> GetBarnById(Guid BarnId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của công nhân.
        /// </summary>
        Task<(List<BarnResponse> Barns, string ErrorMessage)> GetBarnByWorker(Guid workerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang tìm kiếm lọc tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(PaginationSet<BarnResponse> Result, string ErrorMessage)> GetPaginatedBarnList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        // Lấy danh sách chuồng trại cho admin với thông tin có LivestockCircle đang hoạt động hay không
        Task<(PaginationSet<AdminBarnResponse> Result, string ErrorMessage)> GetPaginatedAdminBarnListAsync(
            ListingRequest request, CancellationToken cancellationToken = default);

        // Lấy chi tiết chuồng trại cho admin, bao gồm thông tin LivestockCircle đang hoạt động (nếu có)
        Task<(AdminBarnDetailResponse Barn, string ErrorMessage)> GetAdminBarnDetailAsync(
            Guid barnId, CancellationToken cancellationToken = default);

    }
}