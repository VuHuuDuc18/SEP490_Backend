using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Implements
{
    public class FoodService : IFoodService
    {
        private readonly IRepository<Food> _foodRepository;

        /// <summary>
        /// Khởi tạo service với repository của Food.
        /// </summary>
        public FoodService(IRepository<Food> foodRepository)
        {
            _foodRepository = foodRepository ?? throw new ArgumentNullException(nameof(foodRepository));
        }

        /// <summary>
        /// Tạo một loại thức ăn mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateFoodRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thức ăn không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem thức ăn với tên này đã tồn tại chưa trong cùng danh mục
            var checkError = new Ref<CheckError>();
            var exists = await _foodRepository.CheckExist(
                x => x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thức ăn tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại.");

            var food = new Food
            {
                FoodName = request.FoodName,
                FoodCategoryId = request.FoodCategoryId,
                Stock = request.Stock,
                WeighPerUnit = request.WeighPerUnit
            };

            try
            {
                _foodRepository.Insert(food);
                await _foodRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thức ăn.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateFoodRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thức ăn không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _foodRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy thức ăn.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các thức ăn khác trong cùng danh mục
            var exists = await _foodRepository.CheckExist(
                x => x.FoodName == request.FoodName && x.FoodCategoryId == request.FoodCategoryId && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thức ăn tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thức ăn với tên '{request.FoodName}' trong danh mục này đã tồn tại.");

            try
            {
                existing.FoodName = request.FoodName;
                existing.FoodCategoryId = request.FoodCategoryId;
                existing.Stock = request.Stock;
                existing.WeighPerUnit = request.WeighPerUnit;

                _foodRepository.Update(existing);
                await _foodRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một loại thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var food = await _foodRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (food == null)
                return (false, "Không tìm thấy thức ăn.");

            try
            {
                food.IsActive = false;
                _foodRepository.Update(food);
                await _foodRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thức ăn theo ID.
        /// </summary>
        public async Task<(FoodResponse Food, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var food = await _foodRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin thức ăn: {checkError.Value.Message}");

            if (food == null)
                return (null, "Không tìm thấy thức ăn.");

            var response = new FoodResponse
            {
                Id = food.Id,
                FoodName = food.FoodName,
                FoodCategoryId = food.FoodCategoryId,
                Stock = food.Stock,
                WeighPerUnit = food.WeighPerUnit,
                IsActive = food.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<FoodResponse> Foods, string ErrorMessage)> GetAllAsync(
            string foodName = null,
            Guid? foodCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _foodRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(foodName))
                    query = query.Where(x => x.FoodName.Contains(foodName));

                if (foodCategoryId.HasValue)
                    query = query.Where(x => x.FoodCategoryId == foodCategoryId.Value);

                var foods = await query.ToListAsync(cancellationToken);
                var responses = foods.Select(f => new FoodResponse
                {
                    Id = f.Id,
                    FoodName = f.FoodName,
                    FoodCategoryId = f.FoodCategoryId,
                    Stock = f.Stock,
                    WeighPerUnit = f.WeighPerUnit,
                    IsActive = f.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách thức ăn: {ex.Message}");
            }
        }
    }
}