using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using


using Domain.Dto.Request;
using Domain.Dto.Response;
namespace Domain.Services.Interfaces
{
    public interface IBarnService
    {
        /// <summary>
        /// Tạo một chuồng trại mới với kiểm tra hợp lệ.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateBarnRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một chuồng trại.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateBarnRequest requestDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một chuồng trại bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một chuồng trại theo ID.
        /// </summary>
        Task<(BarnResponse Barn, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả chuồng trại đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        Task<(List<BarnResponse> Barns, string ErrorMessage)> GetAllAsync(string barnName = null, Guid? workerId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách chuồng trại theo ID của công nhân.
        /// </summary>
        Task<(List<BarnResponse> Barns, string ErrorMessage)> GetByWorkerAsync(Guid workerId, CancellationToken cancellationToken = default);
    }
}