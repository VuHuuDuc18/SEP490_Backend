using Domain.Dto.Request.Breed;
using Domain.Dto.Response.Breed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBreedService
    {
        /// <summary>
        /// Tạo một giống loài mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateBreedRequest request, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateBreedRequest request, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một giống loài bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một giống loài theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(BreedResponse Breed, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(List<BreedResponse> Breeds, string ErrorMessage)> GetAllAsync(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default);
    }
}