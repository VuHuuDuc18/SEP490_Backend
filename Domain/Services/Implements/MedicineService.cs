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
    public class MedicineService : IMedicineService
    {
        private readonly IRepository<Medicine> _medicineRepository;

        /// <summary>
        /// Khởi tạo service với repository của Medicine.
        /// </summary>
        public MedicineService(IRepository<Medicine> medicineRepository)
        {
            _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        }

        /// <summary>
        /// Tạo một loại thuốc mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thuốc không được null.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xem thuốc với tên này đã tồn tại chưa trong cùng danh mục
            var checkError = new Ref<CheckError>();
            var exists = await _medicineRepository.CheckExist(
                x => x.MedicineName == request.MedicineName && x.MedicineCategoryId == request.MedicineCategoryId && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thuốc tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thuốc với tên '{request.MedicineName}' trong danh mục này đã tồn tại.");

            var medicine = new Medicine
            {
                MedicineName = request.MedicineName,
                MedicineCategoryId = request.MedicineCategoryId,
                Stock = request.Stock
            };

            try
            {
                _medicineRepository.Insert(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return (false, "Dữ liệu thuốc không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (existing == null)
                return (false, "Không tìm thấy thuốc.");

            // Kiểm tra các trường bắt buộc
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            // Kiểm tra xung đột tên với các thuốc khác trong cùng danh mục
            var exists = await _medicineRepository.CheckExist(
                x => x.MedicineName == request.MedicineName && x.MedicineCategoryId == request.MedicineCategoryId && x.Id != id && x.IsActive,
                checkError,
                cancellationToken);

            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi kiểm tra thuốc tồn tại: {checkError.Value.Message}");

            if (exists)
                return (false, $"Thuốc với tên '{request.MedicineName}' trong danh mục này đã tồn tại.");

            try
            {
                existing.MedicineName = request.MedicineName;
                existing.MedicineCategoryId = request.MedicineCategoryId;
                existing.Stock = request.Stock;

                _medicineRepository.Update(existing);
                await _medicineRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một loại thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicine = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (medicine == null)
                return (false, "Không tìm thấy thuốc.");

            try
            {
                medicine.IsActive = false;
                _medicineRepository.Update(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID.
        /// </summary>
        public async Task<(MedicineResponse Medicine, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicine = await _medicineRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin thuốc: {checkError.Value.Message}");

            if (medicine == null)
                return (null, "Không tìm thấy thuốc.");

            var response = new MedicineResponse
            {
                Id = medicine.Id,
                MedicineName = medicine.MedicineName,
                MedicineCategoryId = medicine.MedicineCategoryId,
                Stock = medicine.Stock,
                IsActive = medicine.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả loại thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<MedicineResponse> Medicines, string ErrorMessage)> GetAllAsync(
            string medicineName = null,
            Guid? medicineCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineRepository.GetQueryable(x => x.IsActive);

                if (!string.IsNullOrEmpty(medicineName))
                    query = query.Where(x => x.MedicineName.Contains(medicineName));

                if (medicineCategoryId.HasValue)
                    query = query.Where(x => x.MedicineCategoryId == medicineCategoryId.Value);

                var medicines = await query.ToListAsync(cancellationToken);
                var responses = medicines.Select(m => new MedicineResponse
                {
                    Id = m.Id,
                    MedicineName = m.MedicineName,
                    MedicineCategoryId = m.MedicineCategoryId,
                    Stock = m.Stock,
                    IsActive = m.IsActive
                }).ToList();
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách thuốc: {ex.Message}");
            }
        }
    }
}