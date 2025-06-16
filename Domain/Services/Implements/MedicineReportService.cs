using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Implements
{
    public class MedicineReportService : IMedicineReportService
    {
        private readonly IRepository<MedicineReport> _medicineReportRepository;
        private readonly IRepository<LivestockCircleMedicine> _livestockCircleMedicineRepository;
        private readonly IRepository<DailyReport> _dailyReportRepository;

        /// <summary>
        /// Khởi tạo service với các repository cần thiết.
        /// </summary>
        public MedicineReportService(IRepository<MedicineReport> medicineReportRepository, IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository, IRepository<DailyReport> dailyReportRepository)
        {
            _medicineReportRepository = medicineReportRepository ?? throw new ArgumentNullException(nameof(medicineReportRepository));
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository ?? throw new ArgumentNullException(nameof(livestockCircleMedicineRepository));
            _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
        }

        /// <summary>
        /// Tạo một báo cáo thuốc mới và trừ lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateMedicineReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo thuốc không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(requestDto.ReportId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (false, "Báo cáo hàng ngày không tồn tại.");

            var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.MedicineId == requestDto.MedicineId && x.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (livestockCircleMedicine == null)
                return (false, "Thông tin thuốc trong vòng chăn nuôi không tồn tại.");
            if (livestockCircleMedicine.Remaining < requestDto.Quantity)
                return (false, "Lượng thuốc yêu cầu vượt quá lượng còn lại.");

            var medicineReport = new MedicineReport
            {
                MedicineId = requestDto.MedicineId,
                ReportId = requestDto.ReportId,
                Quantity = requestDto.Quantity
            };

            try
            {
                livestockCircleMedicine.Remaining -= requestDto.Quantity;
                _livestockCircleMedicineRepository.Update(livestockCircleMedicine);

                _medicineReportRepository.Insert(medicineReport);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo báo cáo thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo thuốc và điều chỉnh lượng còn lại trong LivestockCircleMedicine.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateMedicineReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo thuốc không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _medicineReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo thuốc: {checkError.Value.Message}");
            if (existing == null)
                return (false, "Không tìm thấy báo cáo thuốc.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var dailyReport = await _dailyReportRepository.GetById(requestDto.ReportId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (false, "Báo cáo hàng ngày không tồn tại.");

            var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.MedicineId == requestDto.MedicineId && x.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (livestockCircleMedicine == null)
                return (false, "Thông tin thuốc trong vòng chăn nuôi không tồn tại.");

            var diff = requestDto.Quantity - existing.Quantity;
            if (livestockCircleMedicine.Remaining + diff < 0)
                return (false, "Lượng thuốc yêu cầu vượt quá lượng còn lại sau cập nhật.");

            try
            {
                existing.MedicineId = requestDto.MedicineId;
                existing.ReportId = requestDto.ReportId;
                existing.Quantity = requestDto.Quantity;

                livestockCircleMedicine.Remaining += diff;
                _livestockCircleMedicineRepository.Update(livestockCircleMedicine);

                _medicineReportRepository.Update(existing);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật báo cáo thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một báo cáo thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicineReport = await _medicineReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo thuốc: {checkError.Value.Message}");
            if (medicineReport == null)
                return (false, "Không tìm thấy báo cáo thuốc.");

            try
            {
                medicineReport.IsActive = false;
                _medicineReportRepository.Update(medicineReport);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa báo cáo thuốc: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một báo cáo thuốc theo ID.
        /// </summary>
        public async Task<(MedicineReportResponse MedicineReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var medicineReport = await _medicineReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo thuốc: {checkError.Value.Message}");
            if (medicineReport == null)
                return (null, "Không tìm thấy báo cáo thuốc.");

            var response = new MedicineReportResponse
            {
                Id = medicineReport.Id,
                MedicineId = medicineReport.MedicineId,
                ReportId = medicineReport.ReportId,
                Quantity = medicineReport.Quantity,
                IsActive = medicineReport.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thuốc đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<MedicineReportResponse> MedicineReports, string ErrorMessage)> GetAllAsync(
            Guid? medicineId = null,
            Guid? reportId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineReportRepository.GetQueryable(x => x.IsActive);

                if (medicineId.HasValue)
                    query = query.Where(x => x.MedicineId == medicineId.Value);
                if (reportId.HasValue)
                    query = query.Where(x => x.ReportId == reportId.Value);

                var medicineReports = await query.ToListAsync(cancellationToken);
                var responses = new List<MedicineReportResponse>();
                foreach (var report in medicineReports)
                {
                    responses.Add(new MedicineReportResponse
                    {
                        Id = report.Id,
                        MedicineId = report.MedicineId,
                        ReportId = report.ReportId,
                        Quantity = report.Quantity,
                        IsActive = report.IsActive
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách báo cáo thuốc: {ex.Message}");
            }
        }
    }
}