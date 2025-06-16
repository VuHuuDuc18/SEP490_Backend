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
    public class FoodReportService : IFoodReportService
    {
        private readonly IRepository<FoodReport> _foodReportRepository;
        private readonly IRepository<LivestockCircleFood> _livestockCircleFoodRepository;
        private readonly IRepository<DailyReport> _dailyReportRepository;

        /// <summary>
        /// Khởi tạo service với các repository cần thiết.
        /// </summary>
        public FoodReportService(IRepository<FoodReport> foodReportRepository, IRepository<LivestockCircleFood> livestockCircleFoodRepository, IRepository<DailyReport> dailyReportRepository)
        {
            _foodReportRepository = foodReportRepository ?? throw new ArgumentNullException(nameof(foodReportRepository));
            _livestockCircleFoodRepository = livestockCircleFoodRepository ?? throw new ArgumentNullException(nameof(livestockCircleFoodRepository));
            _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
        }

        /// <summary>
        /// Tạo một báo cáo thức ăn mới và trừ lượng còn lại trong LivestockCircleFood.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateFoodReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo thức ăn không được null.");

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

            var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.FoodId == requestDto.FoodId && x.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (livestockCircleFood == null)
                return (false, "Thông tin thức ăn trong vòng chăn nuôi không tồn tại.");
            if (livestockCircleFood.Remaining < requestDto.Quantity)
                return (false, "Lượng thức ăn yêu cầu vượt quá lượng còn lại.");

            var foodReport = new FoodReport
            {
                FoodId = requestDto.FoodId,
                ReportId = requestDto.ReportId,
                Quantity = requestDto.Quantity
            };

            try
            {
                livestockCircleFood.Remaining -= requestDto.Quantity;
                _livestockCircleFoodRepository.Update(livestockCircleFood);

                _foodReportRepository.Insert(foodReport);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo báo cáo thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một báo cáo thức ăn và điều chỉnh lượng còn lại trong LivestockCircleFood.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateFoodReportRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo thức ăn không được null.");

            var checkError = new Ref<CheckError>();
            var existing = await _foodReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo thức ăn: {checkError.Value.Message}");
            if (existing == null)
                return (false, "Không tìm thấy báo cáo thức ăn.");

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

            var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.FoodId == requestDto.FoodId && x.IsActive).FirstOrDefaultAsync(cancellationToken);
            if (livestockCircleFood == null)
                return (false, "Thông tin thức ăn trong vòng chăn nuôi không tồn tại.");

            var diff = requestDto.Quantity - existing.Quantity;
            if (livestockCircleFood.Remaining + diff < 0)
                return (false, "Lượng thức ăn yêu cầu vượt quá lượng còn lại sau cập nhật.");

            try
            {
                existing.FoodId = requestDto.FoodId;
                existing.ReportId = requestDto.ReportId;
                existing.Quantity = requestDto.Quantity;

                livestockCircleFood.Remaining += diff;
                _livestockCircleFoodRepository.Update(livestockCircleFood);

                _foodReportRepository.Update(existing);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật báo cáo thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa mềm một báo cáo thức ăn bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodReport = await _foodReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo thức ăn: {checkError.Value.Message}");
            if (foodReport == null)
                return (false, "Không tìm thấy báo cáo thức ăn.");

            try
            {
                foodReport.IsActive = false;
                _foodReportRepository.Update(foodReport);
                await _foodReportRepository.CommitAsync(cancellationToken);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa báo cáo thức ăn: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin một báo cáo thức ăn theo ID.
        /// </summary>
        public async Task<(FoodReportResponse FoodReport, string ErrorMessage)> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var foodReport = await _foodReportRepository.GetById(id, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo thức ăn: {checkError.Value.Message}");
            if (foodReport == null)
                return (null, "Không tìm thấy báo cáo thức ăn.");

            var response = new FoodReportResponse
            {
                Id = foodReport.Id,
                FoodId = foodReport.FoodId,
                ReportId = foodReport.ReportId,
                Quantity = foodReport.Quantity,
                IsActive = foodReport.IsActive
            };
            return (response, null);
        }

        /// <summary>
        /// Lấy danh sách tất cả báo cáo thức ăn đang hoạt động với bộ lọc tùy chọn.
        /// </summary>
        public async Task<(List<FoodReportResponse> FoodReports, string ErrorMessage)> GetAllAsync(
            Guid? foodId = null,
            Guid? reportId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _foodReportRepository.GetQueryable(x => x.IsActive);

                if (foodId.HasValue)
                    query = query.Where(x => x.FoodId == foodId.Value);
                if (reportId.HasValue)
                    query = query.Where(x => x.ReportId == reportId.Value);

                var foodReports = await query.ToListAsync(cancellationToken);
                var responses = new List<FoodReportResponse>();
                foreach (var report in foodReports)
                {
                    responses.Add(new FoodReportResponse
                    {
                        Id = report.Id,
                        FoodId = report.FoodId,
                        ReportId = report.ReportId,
                        Quantity = report.Quantity,
                        IsActive = report.IsActive
                    });
                }
                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách báo cáo thức ăn: {ex.Message}");
            }
        }
    }
}