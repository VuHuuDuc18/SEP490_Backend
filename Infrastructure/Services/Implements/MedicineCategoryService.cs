
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
using Domain.Dto.Request;
using Domain.Dto.Response;

namespace Infrastructure.Services.Implements
{
    public class MedicineCategoryService : IMedicineCategoryService
    {
        private readonly IRepository<MedicineCategory> _medicineCategoryRepository;

        /// <summary>
        /// Khởi tạo service với repository của MedicineCategory.
        /// </summary>
        public MedicineCategoryService(IRepository<MedicineCategory> medicineCategoryRepository)
        {
            _medicineCategoryRepository = medicineCategoryRepository ?? throw new ArgumentNullException(nameof(medicineCategoryRepository));
        }

        /// <summary>
        /// Tạo một danh mục thuốc mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục thuốc không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem danh mục với tên này đã tồn tại chưa
            var checkError = new Ref<CheckError>();
            var exists = await _medicineCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục thuốc với tên '{request.Name}' đã tồn tại.");

            var medicineCategory = new MedicineCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            try
            {
                _medicineCategoryRepository.Insert(medicineCategory);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo danh mục thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một danh mục thuốc.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu danh mục thuốc không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _medicineCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục thuốc: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy danh mục thuốc.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các danh mục đang hoạt động khác
            var exists = await _medicineCategoryRepository.CheckExist(
                x => x.Name == request.Name && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra danh mục tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Danh mục thuốc với tên '{request.Name}' đã tồn tại.");

            try
            {
                existing.Name = request.Name;
                existing.Description = request.Description;

                _medicineCategoryRepository.Update(existing);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật danh mục thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một danh mục thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicineCategory = await _medicineCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin danh mục thuốc: {checkError.Value.Message}");

            if (medicineCategory == null)
                return (false, "Không tìm thấy danh mục thuốc.");

            try
            {
                medicineCategory.IsActive = false;
                _medicineCategoryRepository.Update(medicineCategory);
                await _medicineCategoryRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa danh mục thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một danh mục thuốc theo ID.
        /// </summary>
        public async Task<(CategoryResponse Category, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicineCategory = await _medicineCategoryRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin danh mục thuốc: {checkError.Value.Message}");

            if (medicineCategory == null)
                return (null, "Không tìm thấy danh mục thuốc.");

            var response = new CategoryResponse
            {
                Id = medicineCategory.Id,
                Name = medicineCategory.Name,
                Description = medicineCategory.Description,
                IsActive = medicineCategory.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<CategoryResponse> Categories, string ErrorMessage)> GetAllAsync(
            string name = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineCategoryRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(x => x.Name.Contains(name));

                var categories = await query.ToListAsync(cancellationToken);
                var responses = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách danh mục thuốc: {ex.Message}");
            }
        }
    }
}