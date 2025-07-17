using Application.Wrappers;
using CloudinaryDotNet.Actions;
using Domain.Dto.Request;
using Domain.Dto.Request.Barn;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.LivestockCircle;
using Domain.Helper;
using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Dto.Response.User;
using System.Net.WebSockets;
using Domain.Dto.Response.Bill;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Domain.DTOs.Request.Order;



namespace Infrastructure.Services.Implements
{
    public class BarnService : IBarnService
    {
        private readonly IRepository<Barn> _barnRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<ImageLivestockCircle> _imageLiveStockCircleRepository;
        private readonly IRepository<ImageBreed> _imageBreedeRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly CloudinaryCloudService _cloudinaryCloudService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentUserId;

        public BarnService(
            IRepository<Barn> barnRepository,
            IRepository<User> userRepository,
            IRepository<LivestockCircle> livestockCircleRepository,
            IRepository<ImageLivestockCircle> imageLiveStockCircleRepository,
            IRepository<ImageBreed> imageBreedeRepository,
            IRepository<Breed> breedRepository,
            IHttpContextAccessor httpContextAccessor,
            CloudinaryCloudService cloudinaryCloudService)
        {
            _barnRepository = barnRepository ;
            _userRepository = userRepository ;
            _livestockCircleRepository = livestockCircleRepository ;
            _cloudinaryCloudService = cloudinaryCloudService ;
            _imageLiveStockCircleRepository = imageLiveStockCircleRepository;
            _imageBreedeRepository = imageBreedeRepository;
            _breedRepository = breedRepository;
            _httpContextAccessor = httpContextAccessor;

            _currentUserId = Guid.Empty;
            // Lấy current user từ JWT token claims
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        public async Task<Response<string>> CreateBarn(CreateBarnRequest requestDto, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu chuồng trại không được null",
                        Errors = new List<string> { "Dữ liệu chuồng trại không được null" }
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

                var exists = await _barnRepository.GetQueryable(x =>
                    x.BarnName == requestDto.BarnName && x.WorkerId == requestDto.WorkerId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Chuồng trại với tên '{requestDto.BarnName}' của '{requestDto.WorkerId}' đã tồn tại",
                        Errors = new List<string> { $"Chuồng trại với tên '{requestDto.BarnName}' và của '{requestDto.WorkerId}' đã tồn tại" }
                    };
                }

                var worker = await _userRepository.GetByIdAsync(requestDto.WorkerId);
                if (worker == null || !worker.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Người gia công không tồn tại hoặc đã bị xóa" }
                    };
                }

                var barn = new Barn
                {
                    BarnName = requestDto.BarnName,
                    Address = requestDto.Address,
                    WorkerId = requestDto.WorkerId,
                    IsActive = true,
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(requestDto.Image))
                {
                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        requestDto.Image, "barn", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        barn.Image = imageLink;
                    }
                }

                _barnRepository.Insert(barn);
                await _barnRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo chuồng trại thành công",
                    Data = $"Chuồng trại đã được tạo thành công. ID: {barn.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<string>> UpdateBarn(UpdateBarnRequest requestDto, CancellationToken cancellationToken = default)
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
                        Message = "Dữ liệu chuồng trại không được null",
                        Errors = new List<string> { "Dữ liệu chuồng trại không được null" }
                    };
                }

                var existing = await _barnRepository.GetByIdAsync(requestDto.BarnId);
                if (existing == null || !existing.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Chuồng trại không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Chuồng trại không tồn tại hoặc đã bị xóa" }
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

                var exists = await _barnRepository.GetQueryable(x =>
                    x.BarnName == requestDto.BarnName && x.WorkerId == requestDto.WorkerId && x.Id != requestDto.BarnId && x.IsActive)
                    .AnyAsync(cancellationToken);

                if (exists)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = $"Chuồng trại với tên '{requestDto.BarnName}' và của '{requestDto.WorkerId}' đã tồn tại",
                        Errors = new List<string> { $"Chuồng trại với tên '{requestDto.BarnName}' và của '{requestDto.WorkerId}' đã tồn tại" }
                    };
                }

                var worker = await _userRepository.GetByIdAsync(requestDto.WorkerId);
                if (worker == null || !worker.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Người gia công không tồn tại hoặc đã bị xóa" }
                    };
                }

                existing.BarnName = requestDto.BarnName;
                existing.Address = requestDto.Address;
                existing.WorkerId = requestDto.WorkerId;
                existing.UpdatedBy = _currentUserId;
                existing.UpdatedDate = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(requestDto.Image))
                {
                    if (!string.IsNullOrEmpty(existing.Image))
                    {
                        await _cloudinaryCloudService.DeleteImage(existing.Image, cancellationToken);
                    }

                    var imageLink = await UploadImageExtension.UploadBase64ImageAsync(
                        requestDto.Image, "barn", _cloudinaryCloudService, cancellationToken);

                    if (!string.IsNullOrEmpty(imageLink))
                    {
                        existing.Image = imageLink;
                    }
                }

                _barnRepository.Update(existing);
                await _barnRepository.CommitAsync(cancellationToken);

                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật chuồng trại thành công",
                    Data = $"Chuồng trại đã được cập nhật thành công. ID: {existing.Id}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<string>> DisableBarn(Guid barnId, CancellationToken cancellationToken = default)
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

                var barn = await _barnRepository.GetByIdAsync(barnId);
                if (barn == null)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Chuồng trại không tồn tại",
                        Errors = new List<string> { "Chuồng trại không tồn tại" }
                    };
                }

                barn.IsActive = !barn.IsActive;
                barn.UpdatedBy = _currentUserId;
                barn.UpdatedDate = DateTime.UtcNow;

                //if (!string.IsNullOrEmpty(barn.Image))
                //{
                //    await _cloudinaryCloudService.DeleteImage(barn.Image, cancellationToken);
                //    barn.Image = null;
                //}

                _barnRepository.Update(barn);
                await _barnRepository.CommitAsync(cancellationToken);

                if (barn.IsActive)
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Khôi phục chuồng trại thành công",
                        Data = $"Chuồng trại đã được khôi phục thành công. ID: {barn.Id}"
                    };
                }
                else
                {
                    return new Response<string>()
                    {
                        Succeeded = true,
                        Message = "Xóa chuồng trại thành công",
                        Data = $"Chuồng trại đã được xóa thành công. ID: {barn.Id}"
                    };

                }
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xóa chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<BarnResponse>> GetBarnById(Guid barnId, CancellationToken cancellationToken = default)
        {
            try
            {

                var barn = await _barnRepository.GetByIdAsync(barnId);
                if (barn == null || !barn.IsActive)
                {
                    return new Response<BarnResponse>()
                    {
                        Succeeded = false,
                        Message = "Chuồng trại không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Chuồng trại không tồn tại hoặc đã bị xóa" }
                    };
                }

                if (barn.Worker == null || !barn.Worker.IsActive)
                {
                    return new Response<BarnResponse>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Người gia công không tồn tại hoặc đã bị xóa" }
                    };
                }

                var workerResponse = new WokerResponse
                {
                    Id = barn.Worker.Id,
                    FullName = barn.Worker.FullName,
                    Email = barn.Worker.Email
                };

                var response = new BarnResponse
                {
                    Id = barn.Id,
                    BarnName = barn.BarnName,
                    Address = barn.Address,
                    Image = barn.Image,
                    Worker = workerResponse,
                    IsActive = barn.IsActive
                };

                return new Response<BarnResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy thông tin chuồng trại thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<BarnResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy thông tin chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<BarnResponse>>> GetBarnByWorker(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var worker = await _userRepository.GetByIdAsync(_currentUserId);
                if (worker == null || !worker.IsActive)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Người gia công không tồn tại hoặc đã bị xóa" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                var query = _barnRepository.GetQueryable(x => x.WorkerId == _currentUserId && x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items
                    .Where(barn => barn.Worker != null && barn.Worker.IsActive)
                    .Select(barn => new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = new WokerResponse
                        {
                            Id = barn.Worker.Id,
                            FullName = barn.Worker.FullName,
                            Email = barn.Worker.Email
                        },
                        IsActive = barn.IsActive
                    }).ToList();

                var result = new PaginationSet<BarnResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<BarnResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang chuồng trại thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BarnResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<PaginationSet<BarnResponse>>> GetPaginatedBarnList(ListingRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                //if (_currentUserId == Guid.Empty)
                //{
                //    return new Response<PaginationSet<BarnResponse>>()
                //    {
                //        Succeeded = false,
                //        Message = "Hãy đăng nhập và thử lại",
                //        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                //    };
                //}

                if (request == null)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (!string.IsNullOrEmpty(request.Sort?.Field) && !validFields.Contains(request.Sort.Field))
                {
                    return new Response<PaginationSet<BarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                var query = _barnRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var responses = paginationResult.Items
                    .Where(barn => barn.Worker != null && barn.Worker.IsActive)
                    .Select(barn => new BarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = new WokerResponse
                        {
                            Id = barn.Worker.Id,
                            FullName = barn.Worker.FullName,
                            Email = barn.Worker.Email
                        },
                        IsActive = barn.IsActive
                    }).ToList();

                var result = new PaginationSet<BarnResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<BarnResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang chuồng trại thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BarnResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách phân trang chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<AdminBarnResponse>>> GetPaginatedAdminBarnListAsync(ListingRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<PaginationSet<AdminBarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                if (request == null)
                {
                    return new Response<PaginationSet<AdminBarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được null",
                        Errors = new List<string> { "Yêu cầu không được null" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<AdminBarnResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(AdminBarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<AdminBarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (!string.IsNullOrEmpty(request.Sort?.Field) && !validFields.Contains(request.Sort.Field))
                {
                    return new Response<PaginationSet<AdminBarnResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                var query = _barnRepository.GetQueryable();

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                var activeLivestockCircles = await _livestockCircleRepository
                    .GetQueryable(x => x.IsActive && x.Status != StatusConstant.CANCELSTAT && x.Status != StatusConstant.DONESTAT)
                    .Select(x => x.BarnId)
                    .ToListAsync(cancellationToken);

                var responses = paginationResult.Items
                    .Where(barn => barn.Worker != null && barn.Worker.IsActive)
                    .Select(barn => new AdminBarnResponse
                    {
                        Id = barn.Id,
                        BarnName = barn.BarnName,
                        Address = barn.Address,
                        Image = barn.Image,
                        Worker = new WokerResponse
                        {
                            Id = barn.Worker.Id,
                            FullName = barn.Worker.FullName,
                            Email = barn.Worker.Email
                        },
                        IsActive = barn.IsActive,
                        HasActiveLivestockCircle = activeLivestockCircles.Contains(barn.Id)
                    }).ToList();

                var result = new PaginationSet<AdminBarnResponse>
                {
                    PageIndex = paginationResult.PageIndex,
                    Count = responses.Count,
                    TotalCount = paginationResult.TotalCount,
                    TotalPages = paginationResult.TotalPages,
                    Items = responses
                };

                return new Response<PaginationSet<AdminBarnResponse>>()
                {
                    Succeeded = true,
                    Message = "Lấy danh sách phân trang chuồng trại cho admin thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<AdminBarnResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách chuồng trại cho admin",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        public async Task<Response<AdminBarnDetailResponse>> GetAdminBarnDetailAsync(Guid barnId,CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<AdminBarnDetailResponse>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string> { "Hãy đăng nhập và thử lại" }
                    };
                }

                var barn = await _barnRepository.GetQueryable(x => x.Id == barnId && x.IsActive)
                    .Include(x => x.Worker)
                    .FirstOrDefaultAsync(cancellationToken);

                if (barn == null)
                {
                    return new Response<AdminBarnDetailResponse>()
                    {
                        Succeeded = false,
                        Message = "Chuồng trại không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Chuồng trại không tồn tại hoặc đã bị xóa" }
                    };
                }

                if (barn.Worker == null || !barn.Worker.IsActive)
                {
                    return new Response<AdminBarnDetailResponse>()
                    {
                        Succeeded = false,
                        Message = "Người gia công không tồn tại hoặc đã bị xóa",
                        Errors = new List<string> { "Người gia công không tồn tại hoặc đã bị xóa" }
                    };
                }

                var workerResponse = new WokerResponse
                {
                    Id = barn.Worker.Id,
                    FullName = barn.Worker.FullName,
                    Email = barn.Worker.Email
                };

                ActiveLivestockCircleResponse? activeLivestockCircleResponse = null;
                var activeLivestockCircle = await _livestockCircleRepository
                    .GetQueryable(x => x.BarnId == barnId && x.IsActive && x.Status != StatusConstant.CANCELSTAT && x.Status != StatusConstant.DONESTAT)
                    .FirstOrDefaultAsync(cancellationToken);

                if (activeLivestockCircle != null)
                {
                    var technicalStaff = await _userRepository.GetByIdAsync(activeLivestockCircle.TechicalStaffId);
                    if (technicalStaff == null || !technicalStaff.IsActive)
                    {
                        return new Response<AdminBarnDetailResponse>()
                        {
                            Succeeded = false,
                            Message = "Nhân viên kỹ thuật không tồn tại hoặc đã bị xóa",
                            Errors = new List<string> { "Nhân viên kỹ thuật không tồn tại hoặc đã bị xóa" }
                        };
                    }

                    var breed = await _breedRepository.GetByIdAsync(activeLivestockCircle.BreedId);
                    if (breed == null || !breed.IsActive)
                    {
                        return new Response<AdminBarnDetailResponse>()
                        {
                            Succeeded = false,
                            Message = "Giống không tồn tại hoặc đã bị xóa",
                            Errors = new List<string> { "Giống không tồn tại hoặc đã bị xóa" }
                        };
                    }

                    var images = await _imageBreedeRepository.GetQueryable(x => x.BreedId == breed.Id && x.IsActive)
                        .ToListAsync(cancellationToken);

                    var breedResponse = new BreedBillResponse
                    {
                        Id = activeLivestockCircle.BreedId,
                        BreedName = breed.BreedName,
                        Thumbnail = images.FirstOrDefault(x => x.Thumnail == "true")?.ImageLink
                    };

                    var technicalStaffResponse = new UserItemResponse
                    {
                        Id = technicalStaff.Id,
                        Email = technicalStaff.Email,
                        Fullname = technicalStaff.FullName,
                        PhoneNumber = technicalStaff.PhoneNumber
                    };

                    activeLivestockCircleResponse = new ActiveLivestockCircleResponse
                    {
                        Id = activeLivestockCircle.Id,
                        LivestockCircleName = activeLivestockCircle.LivestockCircleName,
                        Status = activeLivestockCircle.Status,
                        StartDate = activeLivestockCircle.StartDate,
                        TotalUnit = activeLivestockCircle.TotalUnit,
                        DeadUnit = activeLivestockCircle.DeadUnit,
                        AverageWeight = activeLivestockCircle.AverageWeight,
                        GoodUnitNumber = activeLivestockCircle.GoodUnitNumber,
                        BadUnitNumber = activeLivestockCircle.BadUnitNumber,
                        Breed = breedResponse,
                        TechicalStaff = technicalStaffResponse
                    };
                }

                var response = new AdminBarnDetailResponse
                {
                    Id = barn.Id,
                    BarnName = barn.BarnName,
                    Address = barn.Address,
                    Image = barn.Image,
                    Worker = workerResponse,
                    IsActive = barn.IsActive,
                    ActiveLivestockCircle = activeLivestockCircleResponse
                };

                return new Response<AdminBarnDetailResponse>()
                {
                    Succeeded = true,
                    Message = "Lấy chi tiết chuồng trại thành công",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new Response<AdminBarnDetailResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy chi tiết chuồng trại",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<ReleaseBarnResponse>>> GetPaginatedReleaseBarnListAsync(ListingRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<ReleaseBarnResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<ReleaseBarnResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(ReleaseBarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<ReleaseBarnResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<ReleaseBarnResponse>>($"Trường sắp xếp không hợp lệ: {request.Sort?.Field}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                }

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive)
                    .Include(x => x.Barn)
                    .Include(x => x.Breed)
                    .ThenInclude(x => x.BreedCategory)
                    .Select(x => new ReleaseBarnResponse()
                    {
                        Id = x.Barn.Id,
                        BarnName = x.Barn.BarnName,
                        Address = x.Barn.Address,
                        Image = x.Barn.Image,

                        TotalUnit = x.TotalUnit,
                        DeadUnit = x.DeadUnit,
                        GoodUnitNumber = x.GoodUnitNumber,
                        BadUnitNumber = x.BadUnitNumber,
                        AverageWeight = x.AverageWeight,
                        BreedCategory = x.Breed.BreedCategory.Name,
                        Breed = x.Breed.BreedName,
                        StartDate = x.StartDate,
                        ReleaseDate = x.ReleaseDate,
                        Age = (DateTime.UtcNow - (DateTime)x.StartDate).Days < 0 ? 0 : (DateTime.UtcNow - (DateTime)x.StartDate).Days
                    });

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);
                
                return new Response<PaginationSet<ReleaseBarnResponse>>(paginationResult, "Lấy dữ liệu thành công.");
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<ReleaseBarnResponse>>($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }

        public async Task<Response<ReleaseBarnDetailResponse>> GetReleaseBarnDetail(Guid BarnId,CancellationToken cancellationToken = default)
        {
            try
            {
                //get current live stock circle
                var liveStockCircle = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.Barn.Id == BarnId && x.Status == StatusConstant.RELEASESTAT).FirstOrDefault();
                if (liveStockCircle == null)
                {
                    return new Response<ReleaseBarnDetailResponse>("Không tìm thấy thông tin chuồng nuôi.");
                }
                //get livestock circle images
                var circleImages =await  _imageLiveStockCircleRepository.GetQueryable(x=>x.IsActive && x.LivestockCircleId == liveStockCircle.Id)
                    .Select(x=> AutoMapperHelper.AutoMap<ImageLivestockCircle, ImageLivestockCircleResponse>(x))
                    .ToListAsync();
                //get breed images
                var breedImages = _imageBreedeRepository.GetQueryable(x => x.IsActive && x.BreedId == liveStockCircle.BreedId).Select(x=> x.ImageLink).ToList();
                //map data to response object
                var result = AutoMapperHelper.AutoMap<Barn,ReleaseBarnDetailResponse>(liveStockCircle.Barn);
                result.LiveStockCircle = AutoMapperHelper.AutoMap<LivestockCircle, LivestockCircleResponse>(liveStockCircle);
                result.Breed = AutoMapperHelper.AutoMap<Breed, BreedResponse>(liveStockCircle.Breed);
                result.Breed.Thumbnail = breedImages.FirstOrDefault();
                result.Breed.BreedCategory = AutoMapperHelper.AutoMap<BreedCategory, BreedCategoryResponse>(liveStockCircle.Breed.BreedCategory);
                result.LiveStockCircle.Images = circleImages;
                result.Breed.ImageLinks = breedImages;

                return new Response<ReleaseBarnDetailResponse>(result, "Lấy thông tin thành công.");
            }
            catch (Exception e)
            {
                return new Response<ReleaseBarnDetailResponse>("Không thế lấy thông tin chuồng nuôi.")
                {
                    Errors = new List<string>(){
                        e.Message
                    }
                };

            }
            
        }

        public async Task<Response<PaginationSet<BarnResponse>>> GetAssignedBarn(Guid tsid, ListingRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<BarnResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<BarnResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(ReleaseBarnResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<BarnResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<BarnResponse>>($"Trường sắp xếp không hợp lệ: {request.Sort?.Field}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                }

                var query = _livestockCircleRepository.GetQueryable(x => x.IsActive && x.TechicalStaffId==tsid)
                    .Include(x => x.Barn)
                    .ThenInclude(x=>x.Worker)
                    .Select(x => new BarnResponse()
                    {
                        Id = x.Barn.Id,
                        BarnName = x.Barn.BarnName,
                        Address = x.Barn.Address,
                        Image = x.Barn.Image,
                        IsActive = x.Barn.IsActive,
                        Worker = new WokerResponse()
                        {
                            Id= x.Barn.Worker.Id,
                            Email = x.Barn.Worker.Email,
                            FullName = x.Barn.Worker.FullName,
                        }
                        
                    });

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var paginationResult = await query.Pagination(request.PageIndex, request.PageSize, request.Sort);

                return new Response<PaginationSet<BarnResponse>>(paginationResult, "Lấy dữ liệu thành công.");
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<BarnResponse>>($"Lỗi khi lấy danh sách phân trang: {ex.Message}");
            }
        }
    }
}