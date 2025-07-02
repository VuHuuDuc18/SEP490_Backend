using Domain.Dto.Request;
using Domain.Dto.Request.Breed;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.Medicine;
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
        Task<(bool Success, string ErrorMessage)> CreateBreed(CreateBreedRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật thông tin một giống loài, bao gồm upload ảnh và thumbnail lên Cloudinary trong folder được chỉ định.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> UpdateBreed(Guid BreedId, UpdateBreedRequest request,CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa mềm một giống loài bằng cách đặt IsActive thành false.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> DisableBreed(Guid BreedId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin một giống loài theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(BreedResponse Breed, string ErrorMessage)> GetBreedById(Guid BreedId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả giống loài đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(List<BreedResponse> Breeds, string ErrorMessage)> GetBreedByCategory(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách phân trang tìm kiếm lọc tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        Task<(PaginationSet<BreedResponse> Result, string ErrorMessage)> GetPaginatedBreedList(
           ListingRequest request,
           CancellationToken cancellationToken = default);
        public Task<bool> ExcelDataHandle(List<CellBreedItem> data);
    }
}