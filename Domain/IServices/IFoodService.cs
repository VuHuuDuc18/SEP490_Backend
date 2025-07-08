using Domain.Dto.Request;
using Domain.Dto.Request.Food;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IFoodService
    {
        /// <summary>
        /// Tạo một loại thức ăn mới với kiểm tra hợp lệ, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> CreateFood(CreateFoodRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateFood(Guid FoodId, UpdateFoodRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một loại thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableFood(Guid FoodId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(FoodResponse Food, string ErrorMessage)> GetFoodById(Guid FoodId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(List<FoodResponse> Foods, string ErrorMessage)> GetFoodByCategory(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Lấy danh sách phân trang tìm kiếm lọc tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(PaginationSet<FoodResponse> Result, string ErrorMessage)> GetPaginatedFoodList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        public Task<bool> ExcelDataHandle(List<CellFoodItem> data);
    }
}