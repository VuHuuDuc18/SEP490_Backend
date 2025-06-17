using Domain.Dto.Request.Food;
using Domain.Dto.Response.Food;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IFoodService
    {
        /// <summary>
        /// Tạo một loại thức ăn mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateAsync(CreateFoodRequest request, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateFoodRequest request, string folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một loại thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(FoodResponse Food, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(List<FoodResponse> Foods, string ErrorMessage)> GetAllAsync(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default);
    }
}