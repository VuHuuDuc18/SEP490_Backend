using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Dto.Request.DailyReport;
using CloudinaryDotNet.Actions;
using Org.BouncyCastle.Asn1.X509;
using Domain.Dto.Response.DailyReport;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Domain.Helper.Constants;

namespace Domain.Services.Implements
{
    public class DailyReportService : IDailyReportService
    {
        private readonly IRepository<DailyReport> _dailyReportRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<FoodReport> _foodReportRepository;
        private readonly IRepository<LivestockCircleFood> _livestockCircleFoodRepository;
        private readonly IRepository<MedicineReport> _medicineReportRepository;
        private readonly IRepository<LivestockCircleMedicine> _livestockCircleMedicineRepository;
        private readonly IRepository<ImageDailyReport> _imageDailyReportRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;

        public DailyReportService(
            IRepository<DailyReport> dailyReportRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<FoodReport> foodReportRepository,
            IRepository<LivestockCircleFood> livestockCircleFoodRepository,
            IRepository<MedicineReport> medicineReportRepository,
            IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository,
            IRepository<ImageDailyReport> imageDailyReportRepository,
            CloudinaryCloudService cloudinaryCloudService)
        {
            _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
            _livestockCircleRepository = livestockCircleRepository ?? throw new ArgumentNullException(nameof(livestockCircleRepository));
            _foodReportRepository = foodReportRepository ?? throw new ArgumentNullException(nameof(foodReportRepository));
            _livestockCircleFoodRepository = livestockCircleFoodRepository ?? throw new ArgumentNullException(nameof(livestockCircleFoodRepository));
            _medicineReportRepository = medicineReportRepository ?? throw new ArgumentNullException(nameof(medicineReportRepository));
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository ?? throw new ArgumentNullException(nameof(livestockCircleMedicineRepository));
            _imageDailyReportRepository = imageDailyReportRepository ?? throw new ArgumentNullException(nameof(imageDailyReportRepository));
            _cloudinaryCloudService = cloudinaryCloudService ?? throw new ArgumentNullException(nameof(cloudinaryCloudService));
        }

        public async Task<(bool Success, string ErrorMessage)> CreateDailyReport(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo hàng ngày không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            var livestockCircle = await _livestockCircleRepository.GetById(requestDto.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null)
                return (false, "Vòng chăn nuôi không tồn tại.");

            // Tính số ngày tuổi
            var ageInDays = (DateTime.UtcNow.Date - ((DateTime)livestockCircle.StartDate).Date).Days;
            if (ageInDays < 0)
                return (false, "Ngày tạo vòng chăn nuôi không hợp lệ.");

            var dailyReport = new DailyReport
            {
                LivestockCircleId = requestDto.LivestockCircleId,
                DeadUnit = requestDto.DeadUnit,
                GoodUnit = livestockCircle.GoodUnitNumber - requestDto.DeadUnit - requestDto.BadUnit,
                AgeInDays = ageInDays,
                Status = DailyReportStatus.TODAY,
                BadUnit = requestDto.BadUnit,
                Note = requestDto.Note,
                IsActive = true
            };

            try
            {
                _dailyReportRepository.Insert(dailyReport);
                await _dailyReportRepository.CommitAsync(cancellationToken);

                livestockCircle.DeadUnit += requestDto.DeadUnit;
                livestockCircle.BadUnitNumber += requestDto.BadUnit;
                livestockCircle.GoodUnitNumber -= livestockCircle.GoodUnitNumber - requestDto.DeadUnit - requestDto.BadUnit;
                _livestockCircleRepository.Update(livestockCircle);
                await _livestockCircleRepository.CommitAsync(cancellationToken);

                var reportId = dailyReport.Id;

                // Tạo FoodReports
                foreach (var food in requestDto.FoodReports ?? new List<CreateFoodReportRequest>())
                {
                    var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.FoodId == food.FoodId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleFood == null)
                        return (false, $"Thông tin thức ăn {food.FoodId} trong vòng chăn nuôi không tồn tại.");
                    if (livestockCircleFood.Remaining < food.Quantity)
                        return (false, $"Lượng thức ăn {food.FoodId} yêu cầu vượt quá lượng còn lại.");

                    var foodReport = new FoodReport
                    {
                        FoodId = food.FoodId,
                        ReportId = reportId,
                        Quantity = food.Quantity,
                        IsActive = true
                    };
                    livestockCircleFood.Remaining -= food.Quantity;
                    _livestockCircleFoodRepository.Update(livestockCircleFood);
                    _foodReportRepository.Insert(foodReport);
                }
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);

                // Tạo MedicineReports
                foreach (var medicine in requestDto.MedicineReports ?? new List<CreateMedicineReportRequest>())
                {
                    var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.MedicineId == medicine.MedicineId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleMedicine == null)
                        return (false, $"Thông tin thuốc {medicine.MedicineId} trong vòng chăn nuôi không tồn tại.");
                    if (livestockCircleMedicine.Remaining < medicine.Quantity)
                        return (false, $"Lượng thuốc {medicine.MedicineId} yêu cầu vượt quá lượng còn lại.");

                    var medicineReport = new MedicineReport
                    {
                        MedicineId = medicine.MedicineId,
                        ReportId = reportId,
                        Quantity = medicine.Quantity,
                        IsActive = true
                    };
                    livestockCircleMedicine.Remaining -= medicine.Quantity;
                    _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                    _medicineReportRepository.Insert(medicineReport);
                }
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);

                // Tạo ImageDailyReports
                if (!string.IsNullOrEmpty(requestDto.Thumbnail))
                {
                    var thumbnailUrl = await UploadImageExtension.UploadBase64ImageAsync(
    requestDto.Thumbnail, "daily-reports", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(thumbnailUrl))
                    {
                        var imageDailyReport = new ImageDailyReport
                        {
                            DailyReportId = reportId,
                            ImageLink = thumbnailUrl,
                            Thumnail = "true",
                            IsActive = true
                        };
                        _imageDailyReportRepository.Insert(imageDailyReport);
                    }
                }

                if (requestDto.ImageLinks != null && requestDto.ImageLinks.Any())
                {
                    foreach (var imageLink in requestDto.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
         imageLink, "daily-reports", _cloudinaryCloudService, cancellationToken);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageDailyReport = new ImageDailyReport
                            {
                                DailyReportId = reportId,
                                ImageLink = uploadedLink,
                                Thumnail = "false",
                                IsActive = true
                            };
                            _imageDailyReportRepository.Insert(imageDailyReport);
                        }
                    }
                }
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi tạo báo cáo hàng ngày: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateDailyReport(Guid dailyReportId, UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo hàng ngày không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            var existing = await _dailyReportRepository.GetById(dailyReportId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (existing == null)
                return (false, "Không tìm thấy báo cáo hàng ngày.");

            var livestockCircle = await _livestockCircleRepository.GetById(requestDto.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null)
                return (false, "Vòng chăn nuôi không tồn tại.");

            try
            {
                // Cập nhật LivestockCircle
                livestockCircle.DeadUnit = livestockCircle.DeadUnit - existing.DeadUnit + requestDto.DeadUnit;
                livestockCircle.BadUnitNumber = livestockCircle.BadUnitNumber - existing.BadUnit + requestDto.BadUnit;
                livestockCircle.GoodUnitNumber = livestockCircle.GoodUnitNumber + existing.DeadUnit + existing.BadUnit - requestDto.BadUnit - requestDto.DeadUnit;
                _livestockCircleRepository.Update(livestockCircle);

                // Cập nhật DailyReport
                existing.LivestockCircleId = requestDto.LivestockCircleId;
                existing.DeadUnit = requestDto.DeadUnit;
                existing.GoodUnit = livestockCircle.GoodUnitNumber + existing.DeadUnit + existing.BadUnit - requestDto.BadUnit - requestDto.DeadUnit;
                existing.BadUnit = requestDto.BadUnit;
                existing.Note = requestDto.Note;
                _dailyReportRepository.Update(existing);

                // Lấy danh sách hiện tại
                var currentFoodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
                var currentMedicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
                var currentImageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReportId).ToListAsync(cancellationToken);

                // Xử lý FoodReports
                foreach (var existingFood in currentFoodReports)
                {
                    var matchingFood = requestDto.FoodReports?.FirstOrDefault(f => f.Id == existingFood.Id);
                    if (matchingFood != null)
                    {
                        var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                            x => x.LivestockCircleId == existing.LivestockCircleId && x.FoodId == matchingFood.FoodId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleFood == null)
                            return (false, $"Thông tin thức ăn {matchingFood.FoodId} không tồn tại.");
                        var diff = matchingFood.Quantity - existingFood.Quantity;
                        if (livestockCircleFood.Remaining + existingFood.Quantity - matchingFood.Quantity < 0)
                            return (false, $"Lượng thức ăn {matchingFood.FoodId} vượt quá lượng còn lại.");

                        livestockCircleFood.Remaining += existingFood.Quantity;
                        existingFood.FoodId = matchingFood.FoodId;
                        existingFood.Quantity = matchingFood.Quantity;
                        livestockCircleFood.Remaining -= matchingFood.Quantity;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                        _foodReportRepository.Update(existingFood);
                    }
                    else
                    {
                        var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                            x => x.LivestockCircleId == existing.LivestockCircleId && x.FoodId == existingFood.FoodId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleFood != null)
                        {
                            livestockCircleFood.Remaining += existingFood.Quantity;
                            _livestockCircleFoodRepository.Update(livestockCircleFood);
                        }
                        existingFood.IsActive = false;
                        _foodReportRepository.Update(existingFood);
                    }
                }

                // Thêm mới FoodReports
                if (requestDto.FoodReports != null)
                {
                    foreach (var newFood in requestDto.FoodReports)
                    {
                        if (!currentFoodReports.Any(f => f.Id == newFood.Id))
                        {
                            var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                                x => x.LivestockCircleId == existing.LivestockCircleId && x.FoodId == newFood.FoodId && x.IsActive)
                                .FirstOrDefaultAsync(cancellationToken);
                            if (livestockCircleFood == null)
                                return (false, $"Thông tin thức ăn {newFood.FoodId} không tồn tại.");
                            if (livestockCircleFood.Remaining < newFood.Quantity)
                                return (false, $"Lượng thức ăn {newFood.FoodId} vượt quá lượng còn lại.");

                            var foodReport = new FoodReport
                            {
                                Id = newFood.Id,
                                FoodId = newFood.FoodId,
                                ReportId = dailyReportId,
                                Quantity = newFood.Quantity,
                                IsActive = true
                            };
                            livestockCircleFood.Remaining -= newFood.Quantity;
                            _livestockCircleFoodRepository.Update(livestockCircleFood);
                            _foodReportRepository.Insert(foodReport);
                        }
                    }
                }

                // Xử lý MedicineReports
                foreach (var existingMedicine in currentMedicineReports)
                {
                    var matchingMedicine = requestDto.MedicineReports?.FirstOrDefault(m => m.Id == existingMedicine.Id);
                    if (matchingMedicine != null)
                    {
                        var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                            x => x.LivestockCircleId == existing.LivestockCircleId && x.MedicineId == matchingMedicine.MedicineId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleMedicine == null)
                            return (false, $"Thông tin thuốc {matchingMedicine.MedicineId} không tồn tại.");
                        var diff = matchingMedicine.Quantity - existingMedicine.Quantity;
                        if (livestockCircleMedicine.Remaining + existingMedicine.Quantity - matchingMedicine.Quantity < 0)
                            return (false, $"Lượng thuốc {matchingMedicine.MedicineId} vượt quá lượng còn lại.");

                        livestockCircleMedicine.Remaining += existingMedicine.Quantity;
                        existingMedicine.MedicineId = matchingMedicine.MedicineId;
                        existingMedicine.Quantity = matchingMedicine.Quantity;
                        livestockCircleMedicine.Remaining -= matchingMedicine.Quantity;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                        _medicineReportRepository.Update(existingMedicine);
                    }
                    else
                    {
                        var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                            x => x.LivestockCircleId == existing.LivestockCircleId && x.MedicineId == existingMedicine.MedicineId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleMedicine != null)
                        {
                            livestockCircleMedicine.Remaining += existingMedicine.Quantity;
                            _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                        }
                        existingMedicine.IsActive = false;
                        _medicineReportRepository.Update(existingMedicine);
                    }
                }

                // Thêm mới MedicineReports
                if (requestDto.MedicineReports != null)
                {
                    foreach (var newMedicine in requestDto.MedicineReports)
                    {
                        if (!currentMedicineReports.Any(m => m.Id == newMedicine.Id))
                        {
                            var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                                x => x.LivestockCircleId == existing.LivestockCircleId && x.MedicineId == newMedicine.MedicineId && x.IsActive)
                                .FirstOrDefaultAsync(cancellationToken);
                            if (livestockCircleMedicine == null)
                                return (false, $"Thông tin thuốc {newMedicine.MedicineId} không tồn tại.");
                            if (livestockCircleMedicine.Remaining < newMedicine.Quantity)
                                return (false, $"Lượng thuốc {newMedicine.MedicineId} vượt quá lượng còn lại.");

                            var medicineReport = new MedicineReport
                            {
                                Id = newMedicine.Id,
                                MedicineId = newMedicine.MedicineId,
                                ReportId = dailyReportId,
                                Quantity = newMedicine.Quantity,
                                IsActive = true
                            };
                            livestockCircleMedicine.Remaining -= newMedicine.Quantity;
                            _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                            _medicineReportRepository.Insert(medicineReport);
                        }
                    }
                }

                // Xử lý ImageDailyReports
                // Xóa ảnh và thumbnail cũ
                foreach (var existingImage in currentImageReports)
                {
                    await _cloudinaryCloudService.DeleteImage(existingImage.ImageLink, cancellationToken);
                     _imageDailyReportRepository.Remove(existingImage);
                }
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                // Upload thumbnail mới
                if (!string.IsNullOrEmpty(requestDto.Thumbnail))
                {
                    var thumbnailUrl = await UploadImageExtension.UploadBase64ImageAsync(
requestDto.Thumbnail, "daily-reports", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(thumbnailUrl))
                    {
                        var imageDailyReport = new ImageDailyReport
                        {
                            DailyReportId = dailyReportId,
                            ImageLink = thumbnailUrl,
                            Thumnail = "true",
                            IsActive = true
                        };
                        _imageDailyReportRepository.Insert(imageDailyReport);
                    }
                }

                // Upload ảnh khác
                if (requestDto.ImageLinks != null && requestDto.ImageLinks.Any())
                {
                    foreach (var imageLink in requestDto.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
        imageLink, "daily-reports", _cloudinaryCloudService, cancellationToken);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageDailyReport = new ImageDailyReport
                            {
                                DailyReportId = dailyReportId,
                                ImageLink = uploadedLink,
                                Thumnail = "false",
                                IsActive = true
                            };
                            _imageDailyReportRepository.Insert(imageDailyReport);
                        }
                    }
                }
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                await _dailyReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật báo cáo hàng ngày: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DisableDailyReport(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(dailyReportId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (false, "Không tìm thấy báo cáo hàng ngày.");

            var livestockCircle = await _livestockCircleRepository.GetById(dailyReport.LivestockCircleId, checkError);
            if (checkError.Value?.IsError == true)
                return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
            if (livestockCircle == null)
                return (false, "Vòng chăn nuôi không tồn tại.");

            try
            {
                dailyReport.IsActive = false;
                _dailyReportRepository.Update(dailyReport);

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
                foreach (var foodReport in foodReports)
                {
                    var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                        x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.FoodId == foodReport.FoodId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleFood != null)
                    {
                        livestockCircleFood.Remaining += foodReport.Quantity;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                    }
                    foodReport.IsActive = false;
                    _foodReportRepository.Update(foodReport);
                }

                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
                foreach (var medicineReport in medicineReports)
                {
                    var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                        x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.MedicineId == medicineReport.MedicineId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleMedicine != null)
                    {
                        livestockCircleMedicine.Remaining += medicineReport.Quantity;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                    }
                    medicineReport.IsActive = false;
                    _medicineReportRepository.Update(medicineReport);
                }

                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReportId).ToListAsync(cancellationToken);
                foreach (var imageReport in imageReports)
                {
                    await _cloudinaryCloudService.DeleteImage(imageReport.ImageLink, cancellationToken);
                    _imageDailyReportRepository.Remove(imageReport);
                }
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                await _dailyReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xóa báo cáo hàng ngày: {ex.Message}");
            }
        }

        public async Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetDailyReportById(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(dailyReportId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (null, "Không tìm thấy báo cáo hàng ngày.");

            var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
            var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);
            var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReportId && x.IsActive).ToListAsync(cancellationToken);

            var response = new DailyReportResponse
            {
                Id = dailyReport.Id,
                LivestockCircleId = dailyReport.LivestockCircleId,
                DeadUnit = dailyReport.DeadUnit,
                GoodUnit = dailyReport.GoodUnit,
                BadUnit = dailyReport.BadUnit,
                AgeInDays = dailyReport.AgeInDays,
                Status = dailyReport.Status,
                Note = dailyReport.Note,
                IsActive = dailyReport.IsActive,
                ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
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

        public async Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetDailyReportByLiveStockCircle(
            Guid? livestockCircleId = null, CancellationToken cancellationToken = default)
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
                    var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == report.Id && x.IsActive).ToListAsync(cancellationToken);

                    responses.Add(new DailyReportResponse
                    {
                        Id = report.Id,
                        LivestockCircleId = report.LivestockCircleId,
                        DeadUnit = report.DeadUnit,
                        GoodUnit = report.GoodUnit,
                        BadUnit = report.BadUnit,
                        Note = report.Note,
                        AgeInDays = report.AgeInDays,
                        Status = report.Status,
                        IsActive = report.IsActive,
                        ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
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

        public async Task<(PaginationSet<FoodReportResponse> Result, string ErrorMessage)> GetFoodReportDetails(
             Guid reportId,
             ListingRequest request,
             CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var checkError = new Ref<CheckError>();
                var dailyReport = await _dailyReportRepository.GetById(reportId, checkError);
                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
                if (dailyReport == null)
                    return (null, "Không tìm thấy báo cáo hàng ngày.");

                var validFields = typeof(FoodReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _foodReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(fr => new FoodReportResponse
                {
                    Id = fr.Id,
                    FoodId = fr.FoodId,
                    ReportId = fr.ReportId,
                    Quantity = fr.Quantity,
                    IsActive = fr.IsActive
                }).ToList();

                var result = new PaginationSet<FoodReportResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chi tiết báo cáo thức ăn: {ex.Message}");
            }
        }

        public async Task<(PaginationSet<MedicineReportResponse> Result, string ErrorMessage)> GetMedicineReportDetails(
            Guid reportId,
            ListingRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var checkError = new Ref<CheckError>();
                var dailyReport = await _dailyReportRepository.GetById(reportId, checkError);
                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
                if (dailyReport == null)
                    return (null, "Không tìm thấy báo cáo hàng ngày.");

                var validFields = typeof(MedicineReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _medicineReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items.Select(mr => new MedicineReportResponse
                {
                    Id = mr.Id,
                    MedicineId = mr.MedicineId,
                    ReportId = mr.ReportId,
                    Quantity = mr.Quantity,
                    IsActive = mr.IsActive
                }).ToList();

                var result = new PaginationSet<MedicineReportResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chi tiết báo cáo thuốc: {ex.Message}");
            }
        }

    public async Task<(PaginationSet<DailyReportResponse> Result, string ErrorMessage)> GetPaginatedDailyReportList(
                ListingRequest request,
                CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return (null, "Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return (null, "PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(DailyReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return (null, $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");

                var query = _dailyReportRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var reportIds = paginationResult.Items.Select(r => r.Id).ToList();

                // Lấy  dữ liệu liên quan
                var foodReports = await _foodReportRepository.GetQueryable(x => reportIds.Contains(x.ReportId) && x.IsActive).ToListAsync(cancellationToken);
                var medicineReports = await _medicineReportRepository.GetQueryable(x => reportIds.Contains(x.ReportId) && x.IsActive).ToListAsync(cancellationToken);
                var imageReports = await _imageDailyReportRepository.GetQueryable(x => reportIds.Contains(x.DailyReportId) && x.IsActive).ToListAsync(cancellationToken);

                // Nhóm data
                var foodReportGroups = foodReports.GroupBy(x => x.ReportId).ToDictionary(g => g.Key, g => g.ToList());
                var medicineReportGroups = medicineReports.GroupBy(x => x.ReportId).ToDictionary(g => g.Key, g => g.ToList());
                var imageReportGroups = imageReports.GroupBy(x => x.DailyReportId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<DailyReportResponse>();
                foreach (var report in paginationResult.Items)
                {
                    var reportFoodReports = foodReportGroups.GetValueOrDefault(report.Id, new List<FoodReport>());
                    var reportMedicineReports = medicineReportGroups.GetValueOrDefault(report.Id, new List<MedicineReport>());
                    var reportImages = imageReportGroups.GetValueOrDefault(report.Id, new List<ImageDailyReport>());

                    responses.Add(new DailyReportResponse
                    {
                        Id = report.Id,
                        LivestockCircleId = report.LivestockCircleId,
                        DeadUnit = report.DeadUnit,
                        GoodUnit = report.GoodUnit,
                        BadUnit = report.BadUnit,
                        Note = report.Note,
                        AgeInDays = report.AgeInDays,
                        Status = report.Status,
                        IsActive = report.IsActive,
                        ImageLinks = reportImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = reportImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                        FoodReports = reportFoodReports.Select(fr => new FoodReportResponse
                        {
                            Id = fr.Id,
                            FoodId = fr.FoodId,
                            ReportId = fr.ReportId,
                            Quantity = fr.Quantity,
                            IsActive = fr.IsActive
                        }).ToList(),
                        MedicineReports = reportMedicineReports.Select(mr => new MedicineReportResponse
                        {
                            Id = mr.Id,
                            MedicineId = mr.MedicineId,
                            ReportId = mr.ReportId,
                            Quantity = mr.Quantity,
                            IsActive = mr.IsActive
                        }).ToList()
                    });
                }

                var result = new PaginationSet<DailyReportResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<(bool HasReport, string ErrorMessage)> HasDailyReportToday(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkError = new Ref<CheckError>();
                var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
                if (checkError.Value?.IsError == true)
                    return (false, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
                if (livestockCircle == null)
                    return (false, "Vòng chăn nuôi không tồn tại.");

                var today = DateTime.UtcNow.Date;
                var hasReport = await _dailyReportRepository.GetQueryable(x =>
                    x.LivestockCircleId == livestockCircleId &&
                    x.IsActive &&
                    x.CreatedDate.Date == today)
                    .AnyAsync(cancellationToken);

                return (hasReport, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi kiểm tra báo cáo hàng ngày: {ex.Message}");
            }
        }

        public async Task<(DailyReportResponse DailyReport, string ErrorMessage)> GetTodayDailyReport(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkError = new Ref<CheckError>();
                var livestockCircle = await _livestockCircleRepository.GetById(livestockCircleId, checkError);
                if (checkError.Value?.IsError == true)
                    return (null, $"Lỗi khi lấy thông tin vòng chăn nuôi: {checkError.Value.Message}");
                if (livestockCircle == null)
                    return (null, "Vòng chăn nuôi không tồn tại.");

                var today = DateTime.UtcNow.Date;
                var dailyReport = await _dailyReportRepository.GetQueryable(x =>
                    x.LivestockCircleId == livestockCircleId &&
                    x.IsActive &&
                    x.CreatedDate.Date == today)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dailyReport == null)
                    return (null, "Không tìm thấy báo cáo hàng ngày cho hôm nay.");

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReport.Id && x.IsActive).ToListAsync(cancellationToken);
                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReport.Id && x.IsActive).ToListAsync(cancellationToken);
                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReport.Id && x.IsActive).ToListAsync(cancellationToken);

                var response = new DailyReportResponse
                {
                    Id = dailyReport.Id,
                    LivestockCircleId = dailyReport.LivestockCircleId,
                    DeadUnit = dailyReport.DeadUnit,
                    GoodUnit = dailyReport.GoodUnit,
                    BadUnit = dailyReport.BadUnit,
                    AgeInDays = dailyReport.AgeInDays,
                    Status = dailyReport.Status,
                    Note = dailyReport.Note,
                    IsActive = dailyReport.IsActive,
                    ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
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
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy báo cáo hàng ngày hôm nay: {ex.Message}");
            }
        }
    }
}
