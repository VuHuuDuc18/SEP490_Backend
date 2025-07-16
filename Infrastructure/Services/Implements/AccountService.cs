using Application.Exceptions;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Request.User;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Domain.Helper.Constants;
using Domain.IServices;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Domain.Dto.Response.User;
using Domain.DTOs.Response.Role;
using Domain.Helper;
namespace Infrastructure.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IdentityContext _context;
        private readonly IRepository<User> _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentUserId;
        public AccountService(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<JWTSettings> jwtSettings,
            SignInManager<User> signInManager,
            IEmailService emailService,
            IdentityContext context,
            IRepository<User> userRepository,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            this._emailService = emailService;
            _context = context;
            _userRepository = userRepository;
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


        public async Task<Response<List<(AccountResponse, RoleResponse)>>> GetAllAccountsAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userNRole = new List<(AccountResponse user, RoleResponse role)>();

                foreach (var user in users)
                {
                    var roleNames = await _userManager.GetRolesAsync(user);
                    var roleName = roleNames.FirstOrDefault();
                    var roles = await _roleManager.Roles.ToListAsync();
                    var role = roles.FirstOrDefault(r => r.Name == roleName);
                    
                    var userResponse = new AccountResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        RoleName = roleName,
                        CreatedDate = user.CreatedDate,
                        CreatedBy = user.CreatedBy,
                        UpdatedDate = user.UpdatedDate,
                        UpdatedBy = user.UpdatedBy
                    };
                    
                    var roleResponse = new RoleResponse
                    {   
                        Id = role?.Id,
                        Name = role?.Name
                    };
                    
                    userNRole.Add((userResponse, roleResponse));
                }

                return new Response<List<(AccountResponse, RoleResponse)>>(userNRole, message: $"Lấy danh sách tài khoản thành công.");
            }
            catch (Exception ex)
            {
                return new Response<List<(AccountResponse, RoleResponse)>>($"Lỗi: {ex.Message}");
            }
        }
        public async Task<Response<User>> GetAccountByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<User>($"Không tìm thấy tài khoản với email {email}.");
            return new Response<User>(user, message: $"Lấy tài khoản thành công.");
        }
        public async Task<Response<string>> CreateAccountAsync(CreateAccountRequest request, string origin)
        {
            
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                IsActive = true,
                CreatedBy = _currentUserId,
                CreatedDate = DateTime.UtcNow,
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, request.RoleName);
                    var verificationUri = await SendVerificationEmail(user, origin);
                    return new Response<string>(user.Id.ToString(), message: $"Đã tạo tài khoản. Một email đã được gửi đến {user.Email} để xác thực tài khoản.");
                }
                else
                {
                    return new Response<string>($"Lỗi khi tạo tài khoản.")
                    {
                        Errors = result.Errors.Select(x => x.Description).ToList()
                    };
                }
            }
            else
            {
                return new Response<string>($"Email {request.Email} đã được đăng ký.");
            }
        }
        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if (result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Đã đặt lại mật khẩu.");
            }
            else
            {
                return new Response<string>($"Lỗi khi đặt lại mật khẩu.")
                {
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
        }
        public async Task<Response<string>> DeleteAccount(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"Không tìm thấy tài khoản với email {email}.");
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new Response<string>(email, message: $"Đã xóa tài khoản.");
            }
            else
            {
                return new Response<string>($"Lỗi khi xóa tài khoản.")
                {
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
        }
        public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<string>("Token không hợp lệ.");

            if (!refreshToken.IsActive)
                return new Response<string>("Token đã bị hủy.");

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();

            return new Response<string>("","Token đã bị hủy.");
        }
        public async Task<Response<string>> DisableAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"Không tìm thấy tài khoản với email {email}.");
            if (!user.IsActive)
            {
                return new Response<string>($"Tài khoản đã bị vô hiệu hóa - {email}.");
            }
            user.IsActive = false;
            user.UpdatedBy = _currentUserId;
            user.UpdatedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return new Response<string>(email, message: $"Tài khoản đã bị vô hiệu hóa.");
        }
        public async Task<Response<string>> EnableAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"Không tìm thấy tài khoản với email {email}.");
            if (user.IsActive)
            {
                return new Response<string>($"Tài khoản đã được kích hoạt - {email}.");
            }
            user.IsActive = true;
            user.UpdatedBy = _currentUserId;
            user.UpdatedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return new Response<string>(email, message: $"Tài khoản đã được kích hoạt.");
        }
        public async Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new Response<string>($"Không tìm thấy tài khoản với ID:{request.UserId}.");
            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            user.UpdatedBy = _currentUserId;
            user.UpdatedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return new Response<string>(user.Id.ToString(), message: $"Tài khoản đã được cập nhật.");
        }
        public async Task<Response<PaginationSet<AccountResponse>>> GetListAccount(ListingRequest req)
        {
            try
            {
                // Materialize accounts trước để tránh DataReader conflict
                var accounts = await _userRepository.GetQueryable().ToListAsync();
                
                // Tạo list AccountResponse với roles
                var accountItems = new List<AccountResponse>();
                
                foreach (User user in accounts) 
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    string role = roles.FirstOrDefault() ?? "";
                    
                    accountItems.Add(new AccountResponse()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        IsActive = user.IsActive,
                        RoleName = role,
                        CreatedDate = user.CreatedDate,
                        CreatedBy = user.CreatedBy,
                        UpdatedDate = user.UpdatedDate,
                        UpdatedBy = user.UpdatedBy
                        
                    });
                }

                // Thực hiện filtering trên List
                if (req.Filter != null && req.Filter.Any())
                {
                    foreach (var filter in req.Filter)
                    {
                        if (string.IsNullOrEmpty(filter.Field) || filter.Value == null)
                            continue;

                        var property = typeof(AccountResponse).GetProperty(filter.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                        if (property == null)
                            continue;

                        accountItems = accountItems.Where(item =>
                        {
                            var value = property.GetValue(item);
                            return value != null && value.ToString().Equals(filter.Value, StringComparison.OrdinalIgnoreCase);
                        }).ToList();
                    }
                }

                // Thực hiện searching trên List
                if (req.SearchString != null && req.SearchString.Any())
                {
                    foreach (var search in req.SearchString)
                    {
                        if (string.IsNullOrEmpty(search.Field) || string.IsNullOrEmpty(search.Value))
                            continue;

                        var property = typeof(AccountResponse).GetProperty(search.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                        if (property == null || property.PropertyType != typeof(string))
                            continue;

                        accountItems = accountItems.Where(item =>
                        {
                            var value = property.GetValue(item) as string;
                            return value != null && value.Contains(search.Value, StringComparison.OrdinalIgnoreCase);
                        }).ToList();
                    }
                }

                // Thực hiện sorting trên List
                if (req.Sort != null && !string.IsNullOrEmpty(req.Sort.Field))
                {
                    var property = typeof(AccountResponse).GetProperty(req.Sort.Field, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (property != null)
                    {
                        if (req.Sort.Value == "asc")
                        {
                            accountItems = accountItems.OrderBy(item => property.GetValue(item)).ToList();
                        }
                        else
                        {
                            accountItems = accountItems.OrderByDescending(item => property.GetValue(item)).ToList();
                        }
                    }
                }

                // Thực hiện pagination trên List
                var totalCount = accountItems.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)req.PageSize);
                var pagedItems = accountItems.Skip((req.PageIndex - 1) * req.PageSize).Take(req.PageSize).ToList();

                var result = new PaginationSet<AccountResponse>
                {
                    PageIndex = req.PageIndex,
                    Count = pagedItems.Count,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Items = pagedItems
                };

                return new Response<PaginationSet<AccountResponse>>()
                {
                    Succeeded = true,
                    Message = "",
                    Data = result
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Response<PaginationSet<AccountResponse>>("Xảy ra lỗi khi lấy thông tin.")
                {
                    Errors = new List<string>() { e.Message }
                };
            }
        }
        private async Task<string> SendVerificationEmail(User user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/user/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            await _emailService.SendEmailAsync(user.Email, EmailConstant.EMAILSUBJECTCONFIRMEMAIL, MailBodyGenerate.BodyCreateConfirmEmail(user.Email, verificationUri));
            return verificationUri;
        }
    }
}
