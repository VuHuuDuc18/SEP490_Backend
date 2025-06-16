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
    public class DailyReportService : IDailyReportService
    {
        private readonly IRepository<DailyReport> _dailyReportRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<FoodReport> _foodReportRepository;
        private readonly IRepository<MedicineReport> _medicineReportRepository;

        /// <summary>
        /// Khởi tạo service với các repository cần thiết.
        /// </summary>
        public DailyReportService(IRepository<DailyReport> dailyReportRepository, IRepository<LivestockCircle> livestockCircleRepository, IRepository<FoodReport> foodReportRepository, IRepository<MedicineReport> medicineReportRepository)
        {
            _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _foodReportRepository = foodReportRepository ?? throw new ArgumentNullException(nameof(foodReportRepository));
            _medicineReportRepository = medicineReportRepository ?? throw new ArgumentNullException(nameof(medicineReportRepository));
        }

        /// <summary>
        /// Tạo một báo cáo hàng ngày mới với kiểm tra hợp lệ.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateDailyReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo hàng ngày không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(requestDto.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null)
                return (false, "Vòng chăn nuôi không tồn tại.");

            var dailyReport = new DailyReport
            {
                LivestockCircleId = requestDto.LivestockCircleId,
                DeadUnit = requestDto.DeadUnit,
                GoodUnit = requestDto.GoodUnit,
                BadUnit = requestDto.BadUnit,
                Note = requestDto.Note
            };

            try
            {
                _dailyReportRepository.Insert(dailyReport);
                await _dailyReportRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo báo cáo hàng ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo hàng ngày.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateDailyReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo hàng ngày không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _dailyReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (existing == null)
                return (false, "Không tìm thấy báo cáo hàng ngày.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
            {
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));
            }

            var livestockCircle = await _livestockCircleRepository.GetById(requestDto.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null)
                return (false, "Vòng chăn nuôi không tồn tại.");

            try
            {
                existing.LivestockCircleId = requestDto.LivestockCircleId;
                existing.DeadUnit = requestDto.DeadUnit;
                existing.GoodUnit = requestDto.GoodUnit;
                existing.BadUnit = requestDto.BadUnit;
                existing.Note = requestDto.Note;

                _dailyReportRepository.Update(existing);
                await _dailyReportRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật báo cáo hàng ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một báo cáo hàng ngày bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (false, "Không tìm thấy báo cáo hàng ngày.");

            try
            {
                dailyReport.IsActive = false;
                _dailyReportRepository.Update(dailyReport);
                await _dailyReportRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa báo cáo hàng ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một báo cáo hàng ngày theo ID, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        public async Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (null, "Không tìm thấy báo cáo hàng ngày.");

            var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);
            var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);

            var response = new DailyReportResponse
            {
                Id = dailyReport.Id,
                LivestockCircleId = dailyReport.LivestockCircleId,
                DeadUnit = dailyReport.DeadUnit,
                GoodUnit = dailyReport.GoodUnit,
                BadUnit = dailyReport.BadUnit,
                Note = dailyReport.Note,
                IsActive = dailyReport.IsActive,
                FoodReports = foodReports.Select(fr => new FoodReportResponse
                {
                    Id = fr.Id,
                    FoodId = fr.FoodId,
                    ReportId = fr.ReportId,
                    Quantity = fr.Quantity,
                    IsActive = fr.IsActive
                }).ToList(),
                MedicineReports = medicineReports.Select(mr => new MedicineReportResponse
                {
                    Id = mr.Id,
                    MedicineId = mr.MedicineId,
                    ReportId = mr.ReportId,
                    Quantity = mr.Quantity,
                    IsActive = mr.IsActive
                }).ToList()
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo hàng ngày đang hoạt động với bộ lọc tùy chọn, bao gồm danh sách FoodReport và MedicineReport.
        /// </summary>
        public async Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetAllAsync(
            Guid? livestockCircleId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dailyReportRepository.GetQueryable(x => x.IsActive);

                if (livestockCircleId.HasValue)
                    query = query.Where(x => x.LivestockCircleId == livestockCircleId.Value);

                var dailyReports = await query.ToListAsync(cancellationToken);
                var responses = new List<DailyReportResponse>();

                foreach (var report in dailyReports)
                {
                    var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == report.Id && x.IsActive).ToListAsync(cancellationToken);
                    var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == report.Id && x.IsActive).ToListAsync(cancellationToken);

                    responses.Add(new DailyReportResponse
                    {
                        Id = report.Id,
                        LivestockCircleId = report.LivestockCircleId,
                        DeadUnit = report.DeadUnit,
                        GoodUnit = report.GoodUnit,
                        BadUnit = report.BadUnit,
                        Note = report.Note,
                        IsActive = report.IsActive,
                        FoodReports = foodReports.Select(fr => new FoodReportResponse
                        {
                            Id = fr.Id,
                            FoodId = fr.FoodId,
                            ReportId = fr.ReportId,
                            Quantity = fr.Quantity,
                            IsActive = fr.IsActive
                        }).ToList(),
                        MedicineReports = medicineReports.Select(mr => new MedicineReportResponse
                        {
                            Id = mr.Id,
                            MedicineId = mr.MedicineId,
                            ReportId = mr.ReportId,
                            Quantity = mr.Quantity,
                            IsActive = mr.IsActive
                        }).ToList()
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách báo cáo hàng ngày: {ex.Message}");
            }
        }
    }
}