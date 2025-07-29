using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Medicine;
using Domain.Helper;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class MedicineService : IMedicineService
    {
        private readonly IRepository<Medicine> _medicineRepository;
        private readonly IRepository<MedicineCategory> _medicineCategoryRepository;
        private readonly IRepository<ImageMedicine> _imageMedicineRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;
        private readonly Guid _currentUserId;

        public MedicineService(
            IRepository<Medicine> medicineRepository,
            IRepository<MedicineCategory> medicineCategoryRepository,
            IRepository<ImageMedicine> imageMedicineRepository,
            CloudinaryCloudService cloudinaryCloudService,
            IHttpContextAccessor httpContextAccessor)
        {
            _medicineRepository = medicineRepository;
            _medicineCategoryRepository = medicineCategoryRepository;
            _imageMedicineRepository = imageMedicineRepository;
            _cloudinaryCloudService = cloudinaryCloudService;

            // Lấy current user từ JWT token claims
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
        public async Task<Response<string>> CreateMedicine(CreateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu thuốc không được null",
                        Errors = new List<string> { "Dữ liệu thuốc không được null" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }


               

                string medicineName = $"{request.MedicineName}/{request.MedicineCode}";
                var exists = await _medicineRepository.GetQueryable(x =>
                    x.MedicineName == medicineName &&
                    x.MedicineCategoryId == request.MedicineCategoryId &&
                    x.IsActive).AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Thuốc với tên '{medicineName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Thuốc với tên '{medicineName}' trong danh mục này đã tồn tại" }
                    };
                }
                var medicineCategory = await _medicineCategoryRepository.GetByIdAsync(request.MedicineCategoryId);
                if (medicineCategory == null || !medicineCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }
                var medicine = new Medicine
                {
                    MedicineName = medicineName,
                    MedicineCategoryId = request.MedicineCategoryId,
                    Stock = request.Stock,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                _medicineRepository.Insert(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "medicine", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageMedicine = new ImageMedicine
                        {
                            MedicineId = medicine.Id,
                            ImageLink = imageLink,
                            Thumnail = "true",
                            IsActive = true
                        };
                        _imageMedicineRepository.Insert(imageMedicine);
                    }
                }

                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "medicine", _cloudinaryCloudService, cancellationToken);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageMedicine = new ImageMedicine
                            {
                                MedicineId = medicine.Id,
                                ImageLink = uploadedLink,
                                Thumnail = "false",
                                IsActive = true
                            };
                            _imageMedicineRepository.Insert(imageMedicine);
                        }
                    }
                }

                await _imageMedicineRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo thuốc thành công",
                    Data = $"Thuốc đã được tạo thành công. ID: {medicine.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc, bao gồm upload ảnh và thumbnail.
        /// </summary>
        public async Task<Response<string>> UpdateMedicine(UpdateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu thuốc không được null",
                        Errors = new List<string> { "Dữ liệu thuốc không được null" }
                    };
                }

                var medicine = await _medicineRepository.GetByIdAsync(request.MedicineId);
                if (medicine == null || !medicine.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(request);
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Dữ liệu không hợp lệ",
                        Errors = validationResults.Select(v => v.ErrorMessage).ToList()
                    };
                }

                string medicineName = $"{request.MedicineName}/{request.MedicineCode}";
                var exists = await _medicineRepository.GetQueryable(x =>
                    x.MedicineName == medicineName &&
                    x.MedicineCategoryId == request.MedicineCategoryId &&
                    x.Id != request.MedicineId &&
                    x.IsActive).AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Thuốc với tên '{medicineName}' trong danh mục này đã tồn tại",
                        Errors = new List<string> { $"Thuốc với tên '{medicineName}' trong danh mục này đã tồn tại" }
                    };
                }

                var medicineCategory = await _medicineCategoryRepository.GetByIdAsync(request.MedicineCategoryId);
                if (medicineCategory == null || !medicineCategory.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Danh mục thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Danh mục thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }

                medicine.MedicineName = medicineName;
                medicine.MedicineCategoryId = request.MedicineCategoryId;
                medicine.Stock = request.Stock;
                medicine.UpdatedBy = _currentUserId;
                medicine.UpdatedDate = DateTime.UtcNow;

                _medicineRepository.Update(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);

                var existingImages = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == request.MedicineId).ToListAsync(cancellationToken);
                foreach (var image in existingImages)
                {
                    _imageMedicineRepository.Remove(image);
                    await _cloudinaryCloudService.DeleteImage(image.ImageLink, cancellationToken);
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(request.Thumbnail))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        request.Thumbnail, "medicine", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        var imageMedicine = new ImageMedicine
                        {
                            MedicineId = request.MedicineId,
                            ImageLink = imageLink,
                            Thumnail = "true",
                            IsActive = true
                        };
                        _imageMedicineRepository.Insert(imageMedicine);
                    }
                }

                if (request.ImageLinks != null && request.ImageLinks.Any())
                {
                    foreach (var imageLink in request.ImageLinks)
                    {
                        var uploadedLink = await UploadImageExtension.UploadBase64ImageAsync(
                            imageLink, "medicine", _cloudinaryCloudService, cancellationToken);

                        if (!string.IsNullOrEmpty(uploadedLink))
                        {
                            var imageMedicine = new ImageMedicine
                            {
                                MedicineId = request.MedicineId,
                                ImageLink = uploadedLink,
                                Thumnail = "false",
                                IsActive = true
                            };
                            _imageMedicineRepository.Insert(imageMedicine);
                        }
                    }
                }
                await _imageMedicineRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật thuốc thành công",
                    Data = $"Thuốc đã được cập nhật thành công. ID: {medicine.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Xóa mềm một loại thuốc bằng cách đặt IsActive thành false.
        /// </summary>
        public async Task<Response<string>> DisableMedicine(Guid medicineId, CancellationToken cancellationToken = default)
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

                var medicine = await _medicineRepository.GetByIdAsync(medicineId);
                if (medicine == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Thuốc không tồn tại",
                        Errors = new List<string> { "Thuốc không tồn tại" }
                    };
                }

                medicine.IsActive = !medicine.IsActive;
                medicine.UpdatedBy = _currentUserId;
                medicine.UpdatedDate = DateTime.UtcNow;

                _medicineRepository.Update(medicine);
                await _medicineRepository.CommitAsync(cancellationToken);

                if (medicine.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục thuốc thành công",
                        Data = $"Thuốc đã được khôi phục thành công. ID: {medicine.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa thuốc thành công",
                        Data = $"Thuốc đã được xóa thành công. ID: {medicine.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Lấy thông tin một loại thuốc theo ID, bao gồm danh sách ảnh và thumbnail.
        /// </summary>
        public async Task<Response<MedicineResponse>> GetMedicineById(Guid medicineId, CancellationToken cancellationToken = default)
        {
            try
            {
                var medicine = await _medicineRepository.GetByIdAsync(medicineId);
                if (medicine == null || !medicine.IsActive)
                {
                    return new Response<MedicineResponse>()
                    {
                        Succeeded = false,
                        Message = "Thuốc không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Thuốc không tồn tại hoặc đã bị xóa" }
                    };
                }

                var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == medicineId && x.IsActive).ToListAsync(cancellationToken);
                var category = await _medicineCategoryRepository.GetByIdAsync(medicine.MedicineCategoryId);
                string[] medicine_sign_detail = medicine.MedicineName.Split('/');
                var response = new MedicineResponse
                {
                    Id = medicine.Id,
                    MedicineName = medicine_sign_detail[0],
                    MedicineCode = medicine_sign_detail[1],
                    MedicineCategory = AutoMapperHelper.AutoMap<MedicineCategory, MedicineCategoryResponse>(category),
                    Stock = medicine.Stock,
                    IsActive = medicine.IsActive,
                    ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                    Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                };

                return new Response<MedicineResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin thuốc thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<MedicineResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<List<MedicineResponse>>> GetMedicineByCategory(
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
                var responses = new List<MedicineResponse>();

                foreach (var medicine in medicines)
                {
                    var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == medicine.Id && x.IsActive).ToListAsync(cancellationToken);
                    var category = await _medicineCategoryRepository.GetByIdAsync(medicine.MedicineCategoryId);
                    string[] medicine_sign_detail = medicine.MedicineName.Split('/');
                    responses.Add(new MedicineResponse
                    {
                        Id = medicine.Id,
                        MedicineName = medicine_sign_detail[0],
                        MedicineCode = medicine_sign_detail[1],
                        MedicineCategory = AutoMapperHelper.AutoMap<MedicineCategory, MedicineCategoryResponse>(category),
                        Stock = medicine.Stock,
                        IsActive = medicine.IsActive,
                        ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                return new Response<List<MedicineResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách thuốc thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<MedicineResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<PaginationSet<MedicineResponse>>> GetPaginatedMedicineList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<MedicineResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<MedicineResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(MedicineResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<MedicineResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<MedicineResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(",", validFields)}" }
                    };
                }

                var query = _medicineRepository.GetQueryable();
                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);
                var medicineIds = paginationResult.Items.Select(f => f.Id).ToList();
                var images = await _imageMedicineRepository.GetQueryable(x => medicineIds.Contains(x.MedicineId) && x.IsActive).ToListAsync(cancellationToken);
                var imageGroups = images.GroupBy(x => x.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

                var responses = new List<MedicineResponse>();
                foreach (var medicine in paginationResult.Items)
                {
                    var category = await _medicineCategoryRepository.GetByIdAsync(medicine.MedicineCategoryId);
                    var medicineImages = imageGroups.GetValueOrDefault(medicine.Id, new List<ImageMedicine>());
                    string[] medicine_sign_detail = medicine.MedicineName.Split('/');
                    responses.Add(new MedicineResponse
                    {
                        Id = medicine.Id,
                        MedicineName = medicine_sign_detail[0] ?? medicine.MedicineName,
                        MedicineCode = medicine_sign_detail[1] ?? medicine.MedicineName,
                        MedicineCategory = AutoMapperHelper.AutoMap<MedicineCategory, MedicineCategoryResponse>(category),
                        Stock = medicine.Stock,
                        IsActive = medicine.IsActive,
                        ImageLinks = medicineImages.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = medicineImages.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                var result = new PaginationSet<MedicineResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<MedicineResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<MedicineResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<bool>> ExcelDataHandle(List<CellMedicineItem> data)
        {
            try
            {

                foreach (var it in data)
                {
                    var MedicineDetail = await _medicineRepository.GetQueryable(x => x.IsActive).FirstOrDefaultAsync(x => x.MedicineName.Contains(it.Ma_dang_ky));
                    var ListCategory = await _medicineCategoryRepository.GetQueryable(x => x.IsActive).ToListAsync();
                    if (MedicineDetail == null)
                    {
                        // add thuoc
                        var MedicineCategoryDetail = ListCategory.FirstOrDefault(x => StringKeyComparer.CompareStrings(x.Name, it.Phan_Loai_Thuoc));
                        if (MedicineCategoryDetail == null)
                        {
                            // add category
                            var MedicineCategoryToInsert = new MedicineCategory()
                            {
                                Name = it.Phan_Loai_Thuoc,
                                Description = it.Phan_Loai_Thuoc
                            };
                            // luu db
                            _medicineCategoryRepository.Insert(MedicineCategoryToInsert);
                            await _medicineCategoryRepository.CommitAsync();
                            // gan lai du lieu chung
                            MedicineCategoryDetail = MedicineCategoryToInsert;
                        }

                        Medicine MedicineToInsert = new Medicine()
                        {
                            MedicineName = it.Ten_Thuoc + "/" + it.Ma_dang_ky,
                            Stock = it.So_luong,
                            MedicineCategoryId = MedicineCategoryDetail.Id
                        };
                        _medicineRepository.Insert(MedicineToInsert);
                    }
                    else
                    {
                        // add sos luong
                        MedicineDetail.Stock += it.So_luong;

                    }
                }
                return new Application.Wrappers.Response<bool>()
                {
                    Succeeded = true,
                    Message = "Nhập dữ liệu thành công"
                };
            }
            catch (Exception ex)
            {
                return new Response<bool>("Lỗi dữ liệu");
            }
        }
        public async Task<Response<List<MedicineResponse>>> GetAllMedicine(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _medicineRepository.GetQueryable(x => x.IsActive);
                var medicines = await query.ToListAsync(cancellationToken);
                var responses = new List<MedicineResponse>();

                foreach (var medicine in medicines)
                {
                    var images = await _imageMedicineRepository.GetQueryable(x => x.MedicineId == medicine.Id && x.IsActive).ToListAsync(cancellationToken);
                    var category = await _medicineCategoryRepository.GetByIdAsync(medicine.MedicineCategoryId);
                    string[] medicine_sign_detail = medicine.MedicineName.Split('/');
                    responses.Add(new MedicineResponse
                    {
                        Id = medicine.Id,
                        MedicineName = medicine_sign_detail[0] ?? medicine.MedicineName,
                        MedicineCode = medicine_sign_detail[1] ?? medicine.MedicineName,
                        MedicineCategory = AutoMapperHelper.AutoMap<MedicineCategory, MedicineCategoryResponse>(category),
                        Stock = medicine.Stock,
                        IsActive = medicine.IsActive,
                        ImageLinks = images.Where(x => x.Thumnail == "false").Select(x => x.ImageLink).ToList(),
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    });
                }

                return new Response<List<MedicineResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách thuốc thành công",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new Response<List<MedicineResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách thuốc",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}