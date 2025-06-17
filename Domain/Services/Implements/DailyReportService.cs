using Domain.Dto.Request;
using Domain.Dto.Response;
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

        public async Task<(bool Success, string ErrorMessage)> CreateAsync(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
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

            var dailyReport = new DailyReport
            {
                LivestockCircleId = requestDto.LivestockCircleId,
                DeadUnit = requestDto.DeadUnit,
                GoodUnit = requestDto.GoodUnit,
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
                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(requestDto.Thumbnail.Split(',')[1]));
                    var thumbnailUrl = await _cloudinaryCloudService.UploadImage(requestDto.Thumbnail, "daily-reports/thumbnails", cancellationToken);
                    File.Delete(tempFilePath);

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
                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, Convert.FromBase64String(imageLink.Split(',')[1]));
                        var uploadedLink = await _cloudinaryCloudService.UploadImage(imageLink, "daily-reports", cancellationToken);
                        File.Delete(tempFilePath);

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

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(Guid id, UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            if (requestDto == null)
                return (false, "Dữ liệu báo cáo hàng ngày không được null.");

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(requestDto);
            if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
                return (false, string.Join("; ", validationResults.Select(v => v.ErrorMessage)));

            var checkError = new Ref<CheckError>();
            var existing = await _dailyReportRepository.GetById(id, checkError);
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
                _livestockCircleRepository.Update(livestockCircle);

                // Cập nhật DailyReport
                existing.LivestockCircleId = requestDto.LivestockCircleId;
                existing.DeadUnit = requestDto.DeadUnit;
                existing.GoodUnit = requestDto.GoodUnit;
                existing.BadUnit = requestDto.BadUnit;
                existing.Note = requestDto.Note;
                _dailyReportRepository.Update(existing);

                // Lấy danh sách hiện tại
                var currentFoodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);
                var currentMedicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);
                var currentImageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == id).ToListAsync(cancellationToken);

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
                                ReportId = id,
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
                                ReportId = id,
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
                   // _imageDailyReportRepository.Delete(existingImage);
                }
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                // Upload thumbnail mới
                if (!string.IsNullOrEmpty(requestDto.Thumbnail))
                {
                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, Convert.FromBase64String(requestDto.Thumbnail.Split(',')[1]));
                    var thumbnailUrl = await _cloudinaryCloudService.UploadImage(requestDto.Thumbnail, "daily-reports/thumbnails", cancellationToken);
                    File.Delete(tempFilePath);

                    if (!string.IsNullOrEmpty(thumbnailUrl))
                    {
                        var imageDailyReport = new ImageDailyReport
                        {
                            DailyReportId = id,
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
                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, Convert.FromBase64String(imageLink.Split(',')[1]));
                        var uploadedLink = await _cloudinaryCloudService.UploadImage(imageLink, "daily-reports", cancellationToken);
                        File.Delete(tempFilePath);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageDailyReport = new ImageDailyReport
                            {
                                DailyReportId = id,
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

        public async Task<(bool Success, string ErrorMessage)> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(id, checkError);
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

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);
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

                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == id && x.IsActive).ToListAsync(cancellationToken);
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

                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == id).ToListAsync(cancellationToken);
                foreach (var imageReport in imageReports)
                {
                    await _cloudinaryCloudService.DeleteImage(imageReport.ImageLink, cancellationToken);
                    //_imageDailyReportRepository.Delete(imageReport);
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
            var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == id && x.IsActive).ToListAsync(cancellationToken);

            var response = new DailyReportResponse
            {
                Id = dailyReport.Id,
                LivestockCircleId = dailyReport.LivestockCircleId,
                DeadUnit = dailyReport.DeadUnit,
                GoodUnit = dailyReport.GoodUnit,
                BadUnit = dailyReport.BadUnit,
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

        public async Task<(List<DailyReportResponse> DailyReports, string ErrorMessage)> GetAllAsync(
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

        public async Task<(List<FoodReportResponse> FoodReports, string ErrorMessage)> GetFoodReportDetailsAsync(Guid reportId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(reportId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (null, "Không tìm thấy báo cáo hàng ngày.");

            try
            {
                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive)
                    .Select(fr => new FoodReportResponse
                    {
                        Id = fr.Id,
                        FoodId = fr.FoodId,
                        ReportId = fr.ReportId,
                        Quantity = fr.Quantity,
                        IsActive = fr.IsActive
                    }).ToListAsync(cancellationToken);

                return (foodReports, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chi tiết báo cáo thức ăn: {ex.Message}");
            }
        }

        public async Task<(List<MedicineReportResponse> MedicineReports, string ErrorMessage)> GetMedicineReportDetailsAsync(Guid reportId, CancellationToken cancellationToken = default)
        {
            var checkError = new Ref<CheckError>();
            var dailyReport = await _dailyReportRepository.GetById(reportId, checkError);
            if (checkError.Value?.IsError == true)
                return (null, $"Lỗi khi lấy thông tin báo cáo hàng ngày: {checkError.Value.Message}");
            if (dailyReport == null)
                return (null, "Không tìm thấy báo cáo hàng ngày.");

            try
            {
                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive)
                    .Select(mr => new MedicineReportResponse
                    {
                        Id = mr.Id,
                        MedicineId = mr.MedicineId,
                        ReportId = mr.ReportId,
                        Quantity = mr.Quantity,
                        IsActive = mr.IsActive
                    }).ToListAsync(cancellationToken);

                return (medicineReports, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy chi tiết báo cáo thuốc: {ex.Message}");
            }
        }
    }
}