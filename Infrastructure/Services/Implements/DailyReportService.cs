
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
using Infrastructure.Extensions;
using Infrastructure.Services;
using Domain.Helper.Constants;
using Domain.Dto.Response.Bill;
using Domain.IServices;
using Microsoft.AspNetCore.Http;
using Application.Wrappers;
using Domain.DTOs.Response.LivestockCircle;
using Domain.Dto.Response.Food;
using Domain.Dto.Response.Medicine;

namespace Infrastructure.Services.Implements
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
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<Medicine> _medicineRepository;
        private readonly IRepository<ImageFood> _foodImageRepository;
        private readonly IRepository<ImageMedicine> _medicineImageRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;
        private readonly Guid _currentUserId;

        public DailyReportService(
            IRepository<DailyReport> dailyReportRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<FoodReport> foodReportRepository,
            IRepository<LivestockCircleFood> livestockCircleFoodRepository,
            IRepository<MedicineReport> medicineReportRepository,
            IRepository<LivestockCircleMedicine> livestockCircleMedicineRepository,
            IRepository<ImageDailyReport> imageDailyReportRepository,
            IRepository<Food> foodRepository,
            IRepository<Medicine> medicineRepository,
            IRepository<ImageFood> foodImageRepository,
            IRepository<ImageMedicine> medicineImageRepository,
            CloudinaryCloudService cloudinaryCloudService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dailyReportRepository = dailyReportRepository;
            _livestockCircleRepository = livestockCircleRepository;
            _foodReportRepository = foodReportRepository;
            _livestockCircleFoodRepository = livestockCircleFoodRepository;
            _medicineReportRepository = medicineReportRepository;
            _livestockCircleMedicineRepository = livestockCircleMedicineRepository;
            _imageDailyReportRepository = imageDailyReportRepository;
            _cloudinaryCloudService = cloudinaryCloudService;
            _foodRepository = foodRepository;
            _foodImageRepository = foodImageRepository;
            _medicineImageRepository = medicineImageRepository;
            _medicineRepository = medicineRepository;

            _currentUserId = Guid.Empty;
            var currentUser = httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        public async Task<Response<string>> CreateDailyReport(CreateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (requestDto == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu báo cáo hàng ngày không được null",
                        Errors = new List<string> { "Dữ liệu báo cáo hàng ngày không được null" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(requestDto);
                if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(requestDto.LivestockCircleId);
                if (livestockCircle == null || !livestockCircle.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Vòng chăn nuôi không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Vòng chăn nuôi không tồn tại hoặc đã bị xóa" }
                    };
                }
                var ageInDays = (DateTime.UtcNow.Date - ((DateTime)livestockCircle.StartDate).Date).Days;

                if (ageInDays < 0)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Ngày tạo vòng chăn nuôi không hợp lệ",
                        Errors = new List<string> { "Ngày tạo vòng chăn nuôi không hợp lệ" }
                    };
                }

                var goodUnit = livestockCircle.GoodUnitNumber - requestDto.DeadUnit - requestDto.BadUnit;
                if (goodUnit < 0)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Số lượng đơn vị tốt không hợp lệ sau khi trừ đơn vị chết và xấu",
                        Errors = new List<string> { "Số lượng đơn vị tốt không hợp lệ" }
                    };
                }

                var dailyReport = new DailyReport
                {
                    LivestockCircleId = requestDto.LivestockCircleId,
                    DeadUnit = requestDto.DeadUnit,
                    GoodUnit = goodUnit,
                    AgeInDays = ageInDays,
                    Status = requestDto.Status ?? DailyReportStatus.TODAY,
                    BadUnit = requestDto.BadUnit,
                    Note = requestDto.Note ?? "Không có ghi chú",
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = _currentUserId,
                    UpdatedDate = DateTime.UtcNow
                };

                _dailyReportRepository.Insert(dailyReport);
                await _dailyReportRepository.CommitAsync(cancellationToken);

                livestockCircle.DeadUnit += requestDto.DeadUnit;
                livestockCircle.BadUnitNumber += requestDto.BadUnit;
                livestockCircle.GoodUnitNumber = goodUnit;
                livestockCircle.UpdatedBy = _currentUserId;
                livestockCircle.UpdatedDate = DateTime.UtcNow;
                _livestockCircleRepository.Update(livestockCircle);
                await _livestockCircleRepository.CommitAsync(cancellationToken);

                var reportId = dailyReport.Id;

                // Tạo FoodReports
                if (requestDto.FoodReports?.Any() == true)
                {
                    var groupedFoods = requestDto.FoodReports
               .GroupBy(f => f.FoodId)
               .Select(g => new { FoodId = g.Key, Quantity = g.Sum(x => x.Quantity) });


                    foreach (var food in groupedFoods)
                    {
                        var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                            x => x.LivestockCircleId == requestDto.LivestockCircleId && x.FoodId == food.FoodId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleFood == null)
                        {
                            return new Response<string>()
                            {
                                Succeeded = false,
                                Message = $"Thông tin thức ăn {food.FoodId} trong vòng chăn nuôi không tồn tại",
                                Errors = new List<string> { $"Thông tin thức ăn {food.FoodId} không tồn tại" }
                            };
                        }
                        if (livestockCircleFood.Remaining < food.Quantity)
                        {
                            return new Response<string>()
                            {
                                Succeeded = false,
                                Message = $"Lượng thức ăn {food.FoodId} yêu cầu vượt quá lượng còn lại",
                                Errors = new List<string> { $"Lượng thức ăn {food.FoodId} vượt quá lượng còn lại" }
                            };
                        }

                        var foodReport = new FoodReport
                        {
                            FoodId = food.FoodId,
                            ReportId = reportId,
                            Quantity = food.Quantity,
                            IsActive = true,
                            CreatedBy = _currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = _currentUserId,
                            UpdatedDate = DateTime.UtcNow
                        };
                        livestockCircleFood.Remaining -= food.Quantity;
                        livestockCircleFood.UpdatedBy = _currentUserId;
                        livestockCircleFood.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                        _foodReportRepository.Insert(foodReport);
                    }
                    await _foodReportRepository.CommitAsync(cancellationToken);
                    await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                }

                // Tạo MedicineReports
                if (requestDto.MedicineReports?.Any() == true)
                {
                    var groupedMeds = requestDto.MedicineReports
               .GroupBy(m => m.MedicineId)
               .Select(g => new { MedicineId = g.Key, Quantity = g.Sum(x => x.Quantity) });
                    foreach (var medicine in groupedMeds)
                    {
                        var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                            x => x.LivestockCircleId == requestDto.LivestockCircleId && x.MedicineId == medicine.MedicineId && x.IsActive)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (livestockCircleMedicine == null)
                        {
                            return new Response<string>()
                            {
                                Succeeded = false,
                                Message = $"Thông tin thuốc {medicine.MedicineId} trong vòng chăn nuôi không tồn tại",
                                Errors = new List<string> { $"Thông tin thuốc {medicine.MedicineId} không tồn tại" }
                            };
                        }
                        if (livestockCircleMedicine.Remaining < medicine.Quantity)
                        {
                            return new Response<string>()
                            {
                                Succeeded = false,
                                Message = $"Lượng thuốc {medicine.MedicineId} yêu cầu vượt quá lượng còn lại",
                                Errors = new List<string> { $"Lượng thuốc {medicine.MedicineId} vượt quá lượng còn lại" }
                            };
                        }

                        var medicineReport = new MedicineReport
                        {
                            MedicineId = medicine.MedicineId,
                            ReportId = reportId,
                            Quantity = medicine.Quantity,
                            IsActive = true,
                            CreatedBy = _currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = _currentUserId,
                            UpdatedDate = DateTime.UtcNow
                        };
                        livestockCircleMedicine.Remaining -= medicine.Quantity;
                        livestockCircleMedicine.UpdatedBy = _currentUserId;
                        livestockCircleMedicine.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                        _medicineReportRepository.Insert(medicineReport);
                    }
                    await _medicineReportRepository.CommitAsync(cancellationToken);
                    await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);
                }

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
                            IsActive = true,
                            CreatedBy = _currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = _currentUserId,
                            UpdatedDate = DateTime.UtcNow
                        };
                        _imageDailyReportRepository.Insert(imageDailyReport);
                    }
                }

                if (requestDto.ImageLinks?.Any() == true)
                {
                    foreach (var imageLink in requestDto.ImageLinks)
                    {
                        if (!string.IsNullOrEmpty(imageLink))
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
                                    IsActive = true,
                                    CreatedBy = _currentUserId,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedBy = _currentUserId,
                                    UpdatedDate = DateTime.UtcNow
                                };
                                _imageDailyReportRepository.Insert(imageDailyReport);
                            }
                        }
                    }
                }

                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo báo cáo hàng ngày thành công",
                    Data = $"Báo cáo hàng ngày đã được tạo thành công. ID: {reportId}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo báo cáo hàng ngày",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<Response<string>> UpdateDailyReport(UpdateDailyReportWithDetailsRequest requestDto, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (requestDto == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu báo cáo hàng ngày không được null",
                        Errors = new List<string> { "Dữ liệu báo cáo hàng ngày không được null" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(requestDto);
                if (!Validator.TryValidateObject(requestDto, validationContext, validationResults, true))
                {
                    return new Response<string>
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                var existing = await _dailyReportRepository.GetByIdAsync(requestDto.DailyReportId);
                if (existing == null || !existing.IsActive)
                {
                    return new Response<string>
                    {
                        Succeeded = false,
                        Message = "Báo cáo hàng ngày không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Báo cáo hàng ngày không tồn tại hoặc đã bị xóa" }
                    };
                }

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(requestDto.LivestockCircleId);
                if (livestockCircle == null || !livestockCircle.IsActive)
                {
                    return new Response<string>
                    {
                        Succeeded = false,
                        Message = "Vòng chăn nuôi không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Vòng chăn nuôi không tồn tại hoặc đã bị xóa" }
                    };
                }

                var oldGoodUnit = existing.GoodUnit + existing.BadUnit + existing.DeadUnit;
                var newGoodUnit = oldGoodUnit - requestDto.DeadUnit - requestDto.BadUnit;
                if (newGoodUnit < 0)
                {
                    return new Response<string>
                    {
                        Succeeded = false,
                        Message = "Số lượng đơn vị tốt không hợp lệ sau khi trừ đơn vị chết và xấu",
                        Errors = new List<string> { "Số lượng đơn vị tốt không hợp lệ" }
                    };
                }

                // Cập nhật LivestockCircle
                livestockCircle.DeadUnit = livestockCircle.DeadUnit - existing.DeadUnit + requestDto.DeadUnit;
                livestockCircle.BadUnitNumber = livestockCircle.BadUnitNumber - existing.BadUnit + requestDto.BadUnit;
                livestockCircle.GoodUnitNumber = newGoodUnit;
                livestockCircle.UpdatedBy = _currentUserId;
                livestockCircle.UpdatedDate = DateTime.UtcNow;
                _livestockCircleRepository.Update(livestockCircle);

                // Cập nhật DailyReport
                existing.LivestockCircleId = requestDto.LivestockCircleId;
                existing.DeadUnit = requestDto.DeadUnit;
                existing.GoodUnit = newGoodUnit;
                existing.BadUnit = requestDto.BadUnit;
                existing.Note = requestDto.Note ?? "Không có ghi chú";
                existing.UpdatedBy = _currentUserId;
                existing.UpdatedDate = DateTime.UtcNow;
                _dailyReportRepository.Update(existing);

                // Xử lý FoodReports
                var currentFoodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == requestDto.DailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);

                // Nhóm FoodReports từ request theo FoodId và cộng dồn Quantity
                var groupedFoodReports = requestDto.FoodReports?.GroupBy(f => f.FoodId)
                    .Select(g => new
                    {
                        FoodId = g.Key,
                        TotalQuantity = g.Sum(f => f.Quantity),
                        Ids = g.Select(f => f.Id).Where(id => id.HasValue).Select(id => id.Value).ToList()
                    })
                    .ToList();

                // Cập nhật hoặc vô hiệu hóa FoodReports hiện tại
                foreach (var existingFood in currentFoodReports)
                {
                    var matchingFood = groupedFoodReports.FirstOrDefault(f => f.FoodId == existingFood.FoodId);
                    var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.FoodId == existingFood.FoodId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (livestockCircleFood != null)
                    {
                        livestockCircleFood.Remaining += existingFood.Quantity;
                        livestockCircleFood.UpdatedBy = _currentUserId;
                        livestockCircleFood.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                    }

                    if (matchingFood != null)
                    {
                        if (livestockCircleFood == null)
                        {
                            return new Response<string>
                            {
                                Succeeded = false,
                                Message = $"Thông tin thức ăn {matchingFood.FoodId} không tồn tại",
                                Errors = new List<string> { $"Thông tin thức ăn {matchingFood.FoodId} không tồn tại" }
                            };
                        }

                        if (livestockCircleFood.Remaining < matchingFood.TotalQuantity)
                        {
                            return new Response<string>
                            {
                                Succeeded = false,
                                Message = $"Lượng thức ăn {matchingFood.FoodId} vượt quá lượng còn lại",
                                Errors = new List<string> { $"Lượng thức ăn {matchingFood.FoodId} vượt quá lượng còn lại" }
                            };
                        }

                        existingFood.Quantity = matchingFood.TotalQuantity;
                        existingFood.UpdatedBy = _currentUserId;
                        existingFood.UpdatedDate = DateTime.UtcNow;
                        livestockCircleFood.Remaining -= matchingFood.TotalQuantity;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                        _foodReportRepository.Update(existingFood);
                    }
                    else
                    {
                        existingFood.IsActive = false;
                        existingFood.UpdatedBy = _currentUserId;
                        existingFood.UpdatedDate = DateTime.UtcNow;
                        _foodReportRepository.Update(existingFood);
                    }
                }

                // Thêm FoodReports mới
                foreach (var newFood in groupedFoodReports.Where(f => !currentFoodReports.Any(cf => cf.FoodId == f.FoodId)))
                {
                    var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.FoodId == newFood.FoodId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (livestockCircleFood == null)
                    {
                        return new Response<string>
                        {
                            Succeeded = false,
                            Message = $"Thông tin thức ăn {newFood.FoodId} không tồn tại",
                            Errors = new List<string> { $"Thông tin thức ăn {newFood.FoodId} không tồn tại" }
                        };
                    }

                    if (livestockCircleFood.Remaining < newFood.TotalQuantity)
                    {
                        return new Response<string>
                        {
                            Succeeded = false,
                            Message = $"Lượng thức ăn {newFood.FoodId} vượt quá lượng còn lại",
                            Errors = new List<string> { $"Lượng thức ăn {newFood.FoodId} vượt quá lượng còn lại" }
                        };
                    }

                    var foodReport = new FoodReport
                    {
                        Id = Guid.NewGuid(),
                        FoodId = newFood.FoodId,
                        ReportId = requestDto.DailyReportId,
                        Quantity = newFood.TotalQuantity,
                        IsActive = true,
                        CreatedBy = _currentUserId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = _currentUserId,
                        UpdatedDate = DateTime.UtcNow
                    };
                    livestockCircleFood.Remaining -= newFood.TotalQuantity;
                    livestockCircleFood.UpdatedBy = _currentUserId;
                    livestockCircleFood.UpdatedDate = DateTime.UtcNow;
                    _livestockCircleFoodRepository.Update(livestockCircleFood);
                    _foodReportRepository.Insert(foodReport);
                }

                // Xử lý MedicineReports
                var currentMedicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == requestDto.DailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);

                // Nhóm MedicineReports từ request theo MedicineId và cộng dồn Quantity
                var groupedMedicineReports = requestDto.MedicineReports?.GroupBy(m => m.MedicineId)
                    .Select(g => new
                    {
                        MedicineId = g.Key,
                        TotalQuantity = g.Sum(m => m.Quantity),
                        Ids = g.Select(m => m.Id).Where(id => id.HasValue).Select(id => id.Value).ToList()
                    })
                    .ToList();

                // Cập nhật hoặc vô hiệu hóa MedicineReports hiện tại
                foreach (var existingMedicine in currentMedicineReports)
                {
                    var matchingMedicine = groupedMedicineReports.FirstOrDefault(m => m.MedicineId == existingMedicine.MedicineId);
                    var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.MedicineId == existingMedicine.MedicineId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (livestockCircleMedicine != null)
                    {
                        livestockCircleMedicine.Remaining += existingMedicine.Quantity;
                        livestockCircleMedicine.UpdatedBy = _currentUserId;
                        livestockCircleMedicine.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                    }

                    if (matchingMedicine != null)
                    {
                        if (livestockCircleMedicine == null)
                        {
                            return new Response<string>
                            {
                                Succeeded = false,
                                Message = $"Thông tin thuốc {matchingMedicine.MedicineId} không tồn tại",
                                Errors = new List<string> { $"Thông tin thuốc {matchingMedicine.MedicineId} không tồn tại" }
                            };
                        }

                        if (livestockCircleMedicine.Remaining < matchingMedicine.TotalQuantity)
                        {
                            return new Response<string>
                            {
                                Succeeded = false,
                                Message = $"Lượng thuốc {matchingMedicine.MedicineId} vượt quá lượng còn lại",
                                Errors = new List<string> { $"Lượng thuốc {matchingMedicine.MedicineId} vượt quá lượng còn lại" }
                            };
                        }

                        existingMedicine.Quantity = matchingMedicine.TotalQuantity;
                        existingMedicine.UpdatedBy = _currentUserId;
                        existingMedicine.UpdatedDate = DateTime.UtcNow;
                        livestockCircleMedicine.Remaining -= matchingMedicine.TotalQuantity;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                        _medicineReportRepository.Update(existingMedicine);
                    }
                    else
                    {
                        existingMedicine.IsActive = false;
                        existingMedicine.UpdatedBy = _currentUserId;
                        existingMedicine.UpdatedDate = DateTime.UtcNow;
                        _medicineReportRepository.Update(existingMedicine);
                    }
                }

                // Thêm MedicineReports mới
                foreach (var newMedicine in groupedMedicineReports.Where(m => !currentMedicineReports.Any(cm => cm.MedicineId == m.MedicineId)))
                {
                    var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                        x => x.LivestockCircleId == requestDto.LivestockCircleId && x.MedicineId == newMedicine.MedicineId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (livestockCircleMedicine == null)
                    {
                        return new Response<string>
                        {
                            Succeeded = false,
                            Message = $"Thông tin thuốc {newMedicine.MedicineId} không tồn tại",
                            Errors = new List<string> { $"Thông tin thuốc {newMedicine.MedicineId} không tồn tại" }
                        };
                    }

                    if (livestockCircleMedicine.Remaining < newMedicine.TotalQuantity)
                    {
                        return new Response<string>
                        {
                            Succeeded = false,
                            Message = $"Lượng thuốc {newMedicine.MedicineId} vượt quá lượng còn lại",
                            Errors = new List<string> { $"Lượng thuốc {newMedicine.MedicineId} vượt quá lượng còn lại" }
                        };
                    }

                    var medicineReport = new MedicineReport
                    {
                        Id = Guid.NewGuid(),
                        MedicineId = newMedicine.MedicineId,
                        ReportId = requestDto.DailyReportId,
                        Quantity = newMedicine.TotalQuantity,
                        IsActive = true,
                        CreatedBy = _currentUserId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = _currentUserId,
                        UpdatedDate = DateTime.UtcNow
                    };
                    livestockCircleMedicine.Remaining -= newMedicine.TotalQuantity;
                    livestockCircleMedicine.UpdatedBy = _currentUserId;
                    livestockCircleMedicine.UpdatedDate = DateTime.UtcNow;
                    _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                    _medicineReportRepository.Insert(medicineReport);
                }

                // Xử lý ImageDailyReports
                var currentImageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == requestDto.DailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                if (requestDto.Thumbnail != null || requestDto.ImageLinks?.Any() == true)
                {
                    foreach (var existingImage in currentImageReports)
                    {
                        if (!string.IsNullOrEmpty(existingImage.ImageLink))
                        {
                            await _cloudinaryCloudService.DeleteImage(existingImage.ImageLink, cancellationToken);
                        }
                        existingImage.IsActive = false;
                        existingImage.UpdatedBy = _currentUserId;
                        existingImage.UpdatedDate = DateTime.UtcNow;
                        _imageDailyReportRepository.Update(existingImage);
                    }
                }

                if (!string.IsNullOrEmpty(requestDto.Thumbnail))
                {
                    var thumbnailUrl = await UploadImageExtension.UploadBase64ImageAsync(
                        requestDto.Thumbnail, "daily-reports", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(thumbnailUrl))
                    {
                        var imageDailyReport = new ImageDailyReport
                        {
                            DailyReportId = requestDto.DailyReportId,
                            ImageLink = thumbnailUrl,
                            Thumnail = "true",
                            IsActive = true,
                            CreatedBy = _currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = _currentUserId,
                            UpdatedDate = DateTime.UtcNow
                        };
                        _imageDailyReportRepository.Insert(imageDailyReport);
                    }
                }

                if (requestDto.ImageLinks?.Any() == true)
                {
                    foreach (var imageLink in requestDto.ImageLinks)
                    {
                        if (!string.IsNullOrEmpty(imageLink))
                        {
                            var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                                imageLink, "daily-reports", _cloudinaryCloudService, cancellationToken);

                            if (!string.IsNullOrEmpty(uploadedLink))
                            {
                                var imageDailyReport = new ImageDailyReport
                                {
                                    DailyReportId = requestDto.DailyReportId,
                                    ImageLink = uploadedLink,
                                    Thumnail = "false",
                                    IsActive = true,
                                    CreatedBy = _currentUserId,
                                    CreatedDate = DateTime.UtcNow,
                                    UpdatedBy = _currentUserId,
                                    UpdatedDate = DateTime.UtcNow
                                };
                                _imageDailyReportRepository.Insert(imageDailyReport);
                            }
                        }
                    }
                }

                await _dailyReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                return new Response<string>
                {
                    Succeeded = true,
                    Message = "Cập nhật báo cáo hàng ngày thành công",
                    Data = $"Báo cáo hàng ngày đã được cập nhật thành công. ID: {requestDto.DailyReportId}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật báo cáo hàng ngày",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

            public async Task<Response<string>> DisableDailyReport(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<string>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var dailyReport = await _dailyReportRepository.GetByIdAsync(dailyReportId);
                if (dailyReport == null || !dailyReport.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Báo cáo hàng ngày không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Báo cáo hàng ngày không tồn tại hoặc đã bị xóa" }
                    };
                }

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(dailyReport.LivestockCircleId);
                if (livestockCircle == null || !livestockCircle.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Vòng chăn nuôi không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Vòng chăn nuôi không tồn tại hoặc đã bị xóa" }
                    };
                }
                var oldGoodUnit =  dailyReport.GoodUnit + dailyReport.BadUnit + dailyReport.DeadUnit;
                livestockCircle.BadUnitNumber -= dailyReport.BadUnit;
                livestockCircle.GoodUnitNumber = oldGoodUnit;
                livestockCircle.DeadUnit -= dailyReport.DeadUnit;
                livestockCircle.UpdatedBy = _currentUserId;
                livestockCircle.UpdatedDate = DateTime.UtcNow;
                _livestockCircleRepository.Update(livestockCircle);

                dailyReport.IsActive = false;
                dailyReport.UpdatedBy = _currentUserId;
                dailyReport.UpdatedDate = DateTime.UtcNow;
                _dailyReportRepository.Update(dailyReport);

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                foreach (var foodReport in foodReports)
                {
                    var livestockCircleFood = await _livestockCircleFoodRepository.GetQueryable(
                        x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.FoodId == foodReport.FoodId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleFood != null)
                    {
                        livestockCircleFood.Remaining += foodReport.Quantity;
                        livestockCircleFood.UpdatedBy = _currentUserId;
                        livestockCircleFood.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleFoodRepository.Update(livestockCircleFood);
                    }
                    foodReport.IsActive = false;
                    foodReport.UpdatedBy = _currentUserId;
                    foodReport.UpdatedDate = DateTime.UtcNow;
                    _foodReportRepository.Update(foodReport);
                }

                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                foreach (var medicineReport in medicineReports)
                {
                    var livestockCircleMedicine = await _livestockCircleMedicineRepository.GetQueryable(
                        x => x.LivestockCircleId == dailyReport.LivestockCircleId && x.MedicineId == medicineReport.MedicineId && x.IsActive)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (livestockCircleMedicine != null)
                    {
                        livestockCircleMedicine.Remaining += medicineReport.Quantity;
                        livestockCircleMedicine.UpdatedBy = _currentUserId;
                        livestockCircleMedicine.UpdatedDate = DateTime.UtcNow;
                        _livestockCircleMedicineRepository.Update(livestockCircleMedicine);
                    }
                    medicineReport.IsActive = false;
                    medicineReport.UpdatedBy = _currentUserId;
                    medicineReport.UpdatedDate = DateTime.UtcNow;
                    _medicineReportRepository.Update(medicineReport);
                }

                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                foreach (var imageReport in imageReports)
                {
                    if (!string.IsNullOrEmpty(imageReport.ImageLink))
                    {
                        await _cloudinaryCloudService.DeleteImage(imageReport.ImageLink, cancellationToken);
                    }
                    imageReport.IsActive = false;
                    imageReport.UpdatedBy = _currentUserId;
                    imageReport.UpdatedDate = DateTime.UtcNow;
                    _imageDailyReportRepository.Update(imageReport);
                }

                await _dailyReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleRepository.CommitAsync(cancellationToken);
                await _foodReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleFoodRepository.CommitAsync(cancellationToken);
                await _medicineReportRepository.CommitAsync(cancellationToken);
                await _livestockCircleMedicineRepository.CommitAsync(cancellationToken);
                await _imageDailyReportRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Vô hiệu hóa báo cáo hàng ngày thành công",
                    Data = $"Báo cáo hàng ngày đã được vô hiệu hóa. ID: {dailyReportId}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa báo cáo hàng ngày",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<Response<DailyReportResponse>> GetDailyReportById(Guid dailyReportId, CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<DailyReportResponse>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var dailyReport = await _dailyReportRepository.GetByIdAsync(dailyReportId);
                if (dailyReport == null || !dailyReport.IsActive)
                {
                    return new Response<DailyReportResponse>()
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy báo cáo hàng ngày",
                        Errors = new List<string> { "Không tìm thấy báo cáo hàng ngày" }
                    };
                }

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);
                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReportId && x.IsActive)
                    .ToListAsync(cancellationToken);

                var foodReportResponses = new List<FoodReportResponse>();
                foreach (var foodReport in foodReports)
                {
                    var foodDetails = await _foodRepository.GetByIdAsync(foodReport.FoodId);
                    var foodImages = foodDetails != null
                        ? await _foodImageRepository.GetQueryable(x => x.FoodId == foodReport.FoodId).ToListAsync(cancellationToken)
                        : new List<ImageFood>();

                    foodReportResponses.Add(new FoodReportResponse
                    {
                        Id = foodReport.Id,
                        ReportId = foodReport.ReportId,
                        Quantity = foodReport.Quantity,
                        IsActive = foodReport.IsActive,
                        Food = new FoodBillResponse
                        {
                            Id = foodDetails?.Id ?? Guid.Empty,
                            FoodName = foodDetails?.FoodName,
                            Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        }
                    });
                }

                var medicineReportResponses = new List<MedicineReportResponse>();
                foreach (var medicineReport in medicineReports)
                {
                    var medicineDetails = await _medicineRepository.GetByIdAsync(medicineReport.MedicineId);
                    var medicineImages = medicineDetails != null
                        ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicineReport.MedicineId).ToListAsync(cancellationToken)
                        : new List<ImageMedicine>();

                    medicineReportResponses.Add(new MedicineReportResponse
                    {
                        Id = medicineReport.Id,
                        ReportId = medicineReport.ReportId,
                        Quantity = medicineReport.Quantity,
                        IsActive = medicineReport.IsActive,
                        Medicine = new MedicineBillResponse
                        {
                            Id = medicineDetails?.Id ?? Guid.Empty,
                            MedicineName = medicineDetails?.MedicineName,
                            Thumbnail = medicineImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        }
                    });
                }

                var response = new DailyReportResponse
                {
                    Id = dailyReport.Id,
                    LivestockCircleId = dailyReport.LivestockCircleId,
                    DeadUnit = dailyReport.DeadUnit,
                    GoodUnit = dailyReport.GoodUnit,
                    BadUnit = dailyReport.BadUnit,
                    AgeInDays = dailyReport.AgeInDays,
                    CreatedDate = dailyReport.CreatedDate,
                    Note = dailyReport.Note,
                    IsActive = dailyReport.IsActive,
                    ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                    FoodReports = foodReportResponses,
                    MedicineReports = medicineReportResponses
                };

                return new Response<DailyReportResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin báo cáo hàng ngày thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<DailyReportResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin báo cáo hàng ngày",
                    Errors = new List<string> { ex.Message }
                };
            }
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

                    var foodReportResponses = new List<FoodReportResponse>();
                    foreach (var foodReport in foodReports)
                    {
                        var foodDetails = await _foodRepository.GetByIdAsync(foodReport.FoodId);
                        var foodImages = foodDetails != null
                            ? await _foodImageRepository.GetQueryable(x => x.FoodId == foodReport.FoodId).ToListAsync(cancellationToken)
                            : new List<ImageFood>();

                        foodReportResponses.Add(new FoodReportResponse
                        {
                            Id = foodReport.Id,
                            ReportId = foodReport.ReportId,
                            Quantity = foodReport.Quantity,
                            IsActive = foodReport.IsActive,
                            Food = new FoodBillResponse
                            {
                                Id = foodDetails?.Id ?? Guid.Empty,
                                FoodName = foodDetails?.FoodName,
                                Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                            }
                        });
                    }

                    var medicineReportResponses = new List<MedicineReportResponse>();
                    foreach (var medicineReport in medicineReports)
                    {
                        var medicineDetails = await _medicineRepository.GetByIdAsync(medicineReport.MedicineId);
                        var medicineImages = medicineDetails != null
                            ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicineReport.MedicineId).ToListAsync(cancellationToken)
                            : new List<ImageMedicine>();

                        medicineReportResponses.Add(new MedicineReportResponse
                        {
                            Id = medicineReport.Id,
                            ReportId = medicineReport.ReportId,
                            Quantity = medicineReport.Quantity,
                            IsActive = medicineReport.IsActive,
                            Medicine = new MedicineBillResponse
                            {
                                Id = medicineDetails?.Id ?? Guid.Empty,
                                MedicineName = medicineDetails?.MedicineName,
                                Thumbnail = medicineImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                            }
                        });
                    }

                    responses.Add(new DailyReportResponse
                    {
                        Id = report.Id,
                        LivestockCircleId = report.LivestockCircleId,
                        DeadUnit = report.DeadUnit,
                        GoodUnit = report.GoodUnit,
                        BadUnit = report.BadUnit,
                        Note = report.Note,
                        AgeInDays = report.AgeInDays,
                        //Status = report.Status,
                        CreatedDate = report.CreatedDate,
                        IsActive = report.IsActive,
                        ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                        FoodReports = foodReportResponses,
                        MedicineReports = medicineReportResponses
                    });
                }

                return (responses, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi khi lấy danh sách báo cáo hàng ngày: {ex.Message}");
            }
        }

        public async Task<Response<PaginationSet<FoodReportResponse>>> GetFoodReportDetails(
             Guid reportId,
             ListingRequest request,
             CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<FoodReportResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<FoodReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<FoodReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var dailyReport = await _dailyReportRepository.GetByIdAsync(reportId);
                if (dailyReport == null || !dailyReport.IsActive)
                {
                    return new Response<PaginationSet<FoodReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy báo cáo hàng ngày",
                        Errors = new List<string> { "Không tìm thấy báo cáo hàng ngày" }
                    };
                }

                var validFields = typeof(FoodReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<FoodReportResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                var query = _foodReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive);

                if (request.SearchString?.Any() == true)
                {
                    query = query.SearchString(request.SearchString);
                }

                if (request.Filter?.Any() == true)
                {
                    query = query.Filter(request.Filter);
                }

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<FoodReportResponse>();
                foreach (var report in paginationResult.Items)
                {
                    var foodDetails = await _foodRepository.GetByIdAsync(report.FoodId);
                    var images = foodDetails != null
                        ? await _foodImageRepository.GetQueryable(x => x.FoodId == report.FoodId).ToListAsync(cancellationToken)
                        : new List<ImageFood>();

                    var foodResponse = new FoodBillResponse
                    {
                        Id = foodDetails?.Id ?? Guid.Empty,
                        FoodName = foodDetails?.FoodName,
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    };

                    responses.Add(new FoodReportResponse
                    {
                        Id = report.Id,
                        ReportId = report.ReportId,
                        Quantity = report.Quantity,
                        IsActive = report.IsActive,
                        Food = foodResponse
                    });
                }

                var result = new PaginationSet<FoodReportResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<FoodReportResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy chi tiết báo cáo thức ăn thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<FoodReportResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy chi tiết báo cáo thức ăn",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<Response<PaginationSet<MedicineReportResponse>>> GetMedicineReportDetails(
                 Guid reportId,
                 ListingRequest request,
                 CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<MedicineReportResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<MedicineReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<MedicineReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var dailyReport = await _dailyReportRepository.GetByIdAsync(reportId);
                if (dailyReport == null || !dailyReport.IsActive)
                {
                    return new Response<PaginationSet<MedicineReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy báo cáo hàng ngày",
                        Errors = new List<string> { "Không tìm thấy báo cáo hàng ngày" }
                    };
                }

                var validFields = typeof(MedicineReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<MedicineReportResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                var query = _medicineReportRepository.GetQueryable(x => x.ReportId == reportId && x.IsActive);

                if (request.SearchString?.Any() == true)
                {
                    query = query.SearchString(request.SearchString);
                }

                if (request.Filter?.Any() == true)
                {
                    query = query.Filter(request.Filter);
                }

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = new List<MedicineReportResponse>();
                foreach (var report in paginationResult.Items)
                {
                    var medicineDetails = await _medicineRepository.GetByIdAsync(report.MedicineId);
                    var images = medicineDetails != null
                        ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == report.MedicineId).ToListAsync(cancellationToken)
                        : new List<ImageMedicine>();

                    var medicineResponse = new MedicineBillResponse
                    {
                        Id = medicineDetails?.Id ?? Guid.Empty,
                        MedicineName = medicineDetails?.MedicineName,
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    };

                    responses.Add(new MedicineReportResponse
                    {
                        Id = report.Id,
                        ReportId = report.ReportId,
                        Quantity = report.Quantity,
                        IsActive = report.IsActive,
                        Medicine = medicineResponse
                    });
                }

                var result = new PaginationSet<MedicineReportResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<MedicineReportResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy chi tiết báo cáo thuốc thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<MedicineReportResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy chi tiết báo cáo thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<DailyReportResponse>>> GetPaginatedDailyReportList(
           ListingRequest request,
           Guid? livestockCircleId = null,
           CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<DailyReportResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<DailyReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<DailyReportResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(DailyReport).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<DailyReportResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}" }
                    };
                }

                var query = _dailyReportRepository.GetQueryable(x => x.IsActive);
                if (livestockCircleId.HasValue)
                {
                    query = query.Where(x => x.LivestockCircleId == livestockCircleId.Value);
                }

                if (request.SearchString?.Any() == true)
                {
                    query = query.SearchString(request.SearchString);
                }

                if (request.Filter?.Any() == true)
                {
                    query = query.Filter(request.Filter);
                }

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var reportIds = paginationResult.Items.Select(r => r.Id).ToList();

                var foodReports = await _foodReportRepository.GetQueryable(x => reportIds.Contains(x.ReportId) && x.IsActive)
                    .ToListAsync(cancellationToken);
                var medicineReports = await _medicineReportRepository.GetQueryable(x => reportIds.Contains(x.ReportId) && x.IsActive)
                    .ToListAsync(cancellationToken);
                var imageReports = await _imageDailyReportRepository.GetQueryable(x => reportIds.Contains(x.DailyReportId) && x.IsActive)
                    .ToListAsync(cancellationToken);

                var foodIds = foodReports.Select(fr => fr.FoodId).Distinct().ToList();
                var medicineIds = medicineReports.Select(mr => mr.MedicineId).Distinct().ToList();

                var foodDetails = await _foodRepository.GetQueryable(x => foodIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
                var medicineDetails = await _medicineRepository.GetQueryable(x => medicineIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

                var foodImageQuery = _foodImageRepository.GetQueryable(x => foodIds.Contains(x.FoodId));
                var medicineImageQuery = _medicineImageRepository.GetQueryable(x => medicineIds.Contains(x.MedicineId));
                var foodImages = (await foodImageQuery.ToListAsync(cancellationToken)).GroupBy(x => x.FoodId).ToDictionary(g => g.Key, g => g.ToList());
                var medicineImages = (await medicineImageQuery.ToListAsync(cancellationToken)).GroupBy(x => x.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

                var foodReportGroups = foodReports.GroupBy(x => x.ReportId).ToDictionary(g => g.Key, g => g.ToList());
                var medicineReportGroups = medicineReports.GroupBy(x => x.ReportId).ToDictionary(g => g.Key, g => g.ToList());
                var imageReportGroups = imageReports.GroupBy(x => x.DailyReportId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<DailyReportResponse>();
                foreach (var report in paginationResult.Items)
                {
                    var reportFoodReports = foodReportGroups.GetValueOrDefault(report.Id, new List<FoodReport>());
                    var reportMedicineReports = medicineReportGroups.GetValueOrDefault(report.Id, new List<MedicineReport>());
                    var reportImages = imageReportGroups.GetValueOrDefault(report.Id, new List<ImageDailyReport>());

                    var foodReportResponses = new List<FoodReportResponse>();
                    foreach (var foodReport in reportFoodReports)
                    {
                        var foodDetail = foodDetails.GetValueOrDefault(foodReport.FoodId);
                        var foodImageList = foodImages.GetValueOrDefault(foodReport.FoodId, new List<ImageFood>());

                        foodReportResponses.Add(new FoodReportResponse
                        {
                            Id = foodReport.Id,
                            ReportId = foodReport.ReportId,
                            Quantity = foodReport.Quantity,
                            IsActive = foodReport.IsActive,
                            Food = new FoodBillResponse
                            {
                                Id = foodDetail?.Id ?? Guid.Empty,
                                FoodName = foodDetail?.FoodName,
                                Thumbnail = foodImageList.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                            }
                        });
                    }

                    var medicineReportResponses = new List<MedicineReportResponse>();
                    foreach (var medicineReport in reportMedicineReports)
                    {
                        var medicineDetail = medicineDetails.GetValueOrDefault(medicineReport.MedicineId);
                        var medicineImageList = medicineImages.GetValueOrDefault(medicineReport.MedicineId, new List<ImageMedicine>());

                        medicineReportResponses.Add(new MedicineReportResponse
                        {
                            Id = medicineReport.Id,
                            ReportId = medicineReport.ReportId,
                            Quantity = medicineReport.Quantity,
                            IsActive = medicineReport.IsActive,
                            Medicine = new MedicineBillResponse
                            {
                                Id = medicineDetail?.Id ?? Guid.Empty,
                                MedicineName = medicineDetail?.MedicineName,
                                Thumbnail = medicineImageList.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                            }
                        });
                    }

                    responses.Add(new DailyReportResponse
                    {
                        Id = report.Id,
                        LivestockCircleId = report.LivestockCircleId,
                        DeadUnit = report.DeadUnit,
                        GoodUnit = report.GoodUnit,
                        BadUnit = report.BadUnit,
                        Note = report.Note,
                        AgeInDays = report.AgeInDays,
                        CreatedDate = report.CreatedDate,
                        IsActive = report.IsActive,
                        ImageLinks = reportImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = reportImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                        FoodReports = foodReportResponses,
                        MedicineReports = medicineReportResponses
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

                return new Response<PaginationSet<DailyReportResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách báo cáo hàng ngày phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<DailyReportResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<Response<bool>> HasDailyReportToday(
                 Guid livestockCircleId,
                 CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<bool>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId);
                if (livestockCircle == null || !livestockCircle.IsActive)
                {
                    return new Response<bool>()
                    {
                        Succeeded = false,
                        Message = "Vòng chăn nuôi không tồn tại",
                        Errors = new List<string> { "Vòng chăn nuôi không tồn tại" }
                    };
                }

                var today = DateTime.UtcNow.Date;
                var hasReport = await _dailyReportRepository.GetQueryable(x =>
                    x.LivestockCircleId == livestockCircleId &&
                    x.IsActive &&
                    x.CreatedDate.Date == today)
                    .AnyAsync(cancellationToken);

                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Kiểm tra báo cáo thành công",
                    Data = hasReport
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi kiểm tra báo cáo hàng ngày",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<DailyReportResponse>> GetTodayDailyReport(
            Guid livestockCircleId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<DailyReportResponse>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(livestockCircleId);
                if (livestockCircle == null || !livestockCircle.IsActive)
                {
                    return new Response<DailyReportResponse>()
                    {
                        Succeeded = false,
                        Message = "Vòng chăn nuôi không tồn tại",
                        Errors = new List<string> { "Vòng chăn nuôi không tồn tại" }
                    };
                }

                var today = DateTime.UtcNow.Date;
                var dailyReport = await _dailyReportRepository.GetQueryable(x =>
                    x.LivestockCircleId == livestockCircleId &&
                    x.IsActive &&
                    x.CreatedDate.Date == today)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dailyReport == null)
                {
                    return new Response<DailyReportResponse>()
                    {
                        Succeeded = false,
                        Message = "Không tìm thấy báo cáo hàng ngày cho hôm nay",
                        Errors = new List<string> { "Không tìm thấy báo cáo hàng ngày cho hôm nay" }
                    };
                }

                var foodReports = await _foodReportRepository.GetQueryable(x => x.ReportId == dailyReport.Id && x.IsActive)
                    .ToListAsync(cancellationToken);
                var medicineReports = await _medicineReportRepository.GetQueryable(x => x.ReportId == dailyReport.Id && x.IsActive)
                    .ToListAsync(cancellationToken);
                var imageReports = await _imageDailyReportRepository.GetQueryable(x => x.DailyReportId == dailyReport.Id && x.IsActive)
                    .ToListAsync(cancellationToken);

                var foodReportResponses = new List<FoodReportResponse>();
                foreach (var foodReport in foodReports)
                {
                    var foodDetails = await _foodRepository.GetByIdAsync(foodReport.FoodId);
                    var foodImages = foodDetails != null
                        ? await _foodImageRepository.GetQueryable(x => x.FoodId == foodReport.FoodId).ToListAsync(cancellationToken)
                        : new List<ImageFood>();

                    foodReportResponses.Add(new FoodReportResponse
                    {
                        Id = foodReport.Id,
                        ReportId = foodReport.ReportId,
                        Quantity = foodReport.Quantity,
                        IsActive = foodReport.IsActive,
                        Food = new FoodBillResponse
                        {
                            Id = foodDetails?.Id ?? Guid.Empty,
                            FoodName = foodDetails?.FoodName,
                            Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        }
                    });
                }

                var medicineReportResponses = new List<MedicineReportResponse>();
                foreach (var medicineReport in medicineReports)
                {
                    var medicineDetails = await _medicineRepository.GetByIdAsync(medicineReport.MedicineId);
                    var medicineImages = medicineDetails != null
                        ? await _medicineImageRepository.GetQueryable(x => x.MedicineId == medicineReport.MedicineId).ToListAsync(cancellationToken)
                        : new List<ImageMedicine>();

                    medicineReportResponses.Add(new MedicineReportResponse
                    {
                        Id = medicineReport.Id,
                        ReportId = medicineReport.ReportId,
                        Quantity = medicineReport.Quantity,
                        IsActive = medicineReport.IsActive,
                        Medicine = new MedicineBillResponse
                        {
                            Id = medicineDetails?.Id ?? Guid.Empty,
                            MedicineName = medicineDetails?.MedicineName,
                            Thumbnail = medicineImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                        }
                    });
                }

                var response = new DailyReportResponse
                {
                    Id = dailyReport.Id,
                    LivestockCircleId = dailyReport.LivestockCircleId,
                    DeadUnit = dailyReport.DeadUnit,
                    GoodUnit = dailyReport.GoodUnit,
                    BadUnit = dailyReport.BadUnit,
                    AgeInDays = dailyReport.AgeInDays,
                    CreatedDate = dailyReport.CreatedDate,
                    Note = dailyReport.Note,
                    IsActive = dailyReport.IsActive,
                    ImageLinks = imageReports.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = imageReports.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink,
                    FoodReports = foodReportResponses,
                    MedicineReports = medicineReportResponses
                };

                return new Response<DailyReportResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy báo cáo hàng ngày hôm nay thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<DailyReportResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy báo cáo hàng ngày hôm nay",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<List<FoodResponse>> GetAllFoodRemainingOfLivestockCircle(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var livestockCircleFoods = await _livestockCircleFoodRepository.GetQueryable(x => x.LivestockCircleId == livestockCircleId && x.IsActive).ToListAsync(cancellationToken);

            // Lấy danh sách FoodId từ LivestockCircleFoods
            var foodIds = livestockCircleFoods.Select(f => f.FoodId).ToList();

            // Lấy thông tin chi tiết của các food từ FoodRepository
            var foods = await _foodRepository.GetQueryable(x => x.IsActive && foodIds.Contains(x.Id))
                .Include(x => x.FoodCategory)
                .ToListAsync(cancellationToken);

            // Lấy danh sách ảnh liên quan đến các food
            var images = await _foodImageRepository.GetQueryable(x => foodIds.Contains(x.FoodId)).ToListAsync(cancellationToken);
            var imageGroups = images.GroupBy(x => x.FoodId).ToDictionary(g => g.Key, g => g.ToList());

            // Tạo danh sách phản hồi
            return foods.Select(food =>
            {
                var livestockCircleFood = livestockCircleFoods.FirstOrDefault(lcf => lcf.FoodId == food.Id);
                var foodCategoryResponse = new FoodCategoryResponse
                {
                    Id = food.FoodCategory.Id,
                    Name = food.FoodCategory.Name,
                    Description = food.FoodCategory.Description
                };
                var foodImages = imageGroups.GetValueOrDefault(food.Id, new List<ImageFood>());
                return new FoodResponse
                {
                    Id = food.Id,
                    FoodName = food.FoodName,
                    FoodCategory = foodCategoryResponse,
                    Stock = food.Stock,
                    WeighPerUnit = food.WeighPerUnit,
                    IsActive = food.IsActive,
                    ImageLinks = foodImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = foodImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };
            }).ToList();
        }


        public async Task<List<MedicineResponse>> GetAllMedicineRemainingOfLivestockCircle(Guid livestockCircleId, CancellationToken cancellationToken = default)
        {
            var livestockCircleMedicines = await _livestockCircleMedicineRepository.GetQueryable(x => x.LivestockCircleId == livestockCircleId && x.IsActive).ToListAsync(cancellationToken);

            // Lấy danh sách MedicineId từ LivestockCircleMedicines
            var medicineIds = livestockCircleMedicines.Select(m => m.MedicineId).ToList();

            // Lấy thông tin chi tiết của các medicine từ MedicineRepository
            var medicines = await _medicineRepository.GetQueryable(x => x.IsActive && medicineIds.Contains(x.Id))
                .Include(x => x.MedicineCategory)
                .ToListAsync(cancellationToken);

            // Lấy danh sách ảnh liên quan đến các medicine
            var images = await _medicineImageRepository.GetQueryable(x => medicineIds.Contains(x.MedicineId)).ToListAsync(cancellationToken);
            var imageGroups = images.GroupBy(x => x.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

            // Tạo danh sách phản hồi
            return medicines.Select(medicine =>
            {
                var livestockCircleMedicine = livestockCircleMedicines.FirstOrDefault(lcm => lcm.MedicineId == medicine.Id);
                var medicineCategoryResponse = new MedicineCategoryResponse
                {
                    Id = medicine.MedicineCategory.Id,
                    Name = medicine.MedicineCategory.Name,
                    Description = medicine.MedicineCategory.Description
                };
                var medicineImages = imageGroups.GetValueOrDefault(medicine.Id, new List<ImageMedicine>());
                return new MedicineResponse
                {
                    Id = medicine.Id,
                    MedicineName = medicine.MedicineName,
                    MedicineCategory = medicineCategoryResponse,
                    Stock = medicine.Stock,
                    IsActive = medicine.IsActive,
                    ImageLinks = medicineImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = medicineImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };
            }).ToList();
        }
    }
}
