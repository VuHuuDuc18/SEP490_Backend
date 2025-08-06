using Application.Exceptions;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Infrastructure.Extensions;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Identity.Helpers;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

using Domain.Dto.Request.User;
using Domain.Dto.Response.User;
using Domain.Dto.Response.BarnPlan;
using Domain.IServices;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userrepo;
        private readonly IEmailService _emailService;
        private readonly IRepository<Role> _rolerepo;
        private readonly SignInManager<User> _signInManager;
        private readonly IdentityContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Guid _currentUserId;

        public UserService(UserManager<User> userManager,
           RoleManager<Role> roleManager,
           IOptions<JWTSettings> jwtSettings,
           SignInManager<User> signInManager,
           IEmailService emailService,
           IdentityContext context,
           IHttpContextAccessor httpContextAccessor
       )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            this._emailService = emailService;
            _context = context;
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
        


        public async Task<Response<AuthenticationResponse>> LoginAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new Response<AuthenticationResponse>($"Không tìm thấy tài khoản với email {request.Email}.");
            }
            if(!user.IsActive)
            {
                return new Response<AuthenticationResponse>($"Tài khoản với email {request.Email} đã bị khóa.");
            }
            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                //throw new ApiException($"Invalid Credentials for '{request.Email}'.");
                return new Response<AuthenticationResponse>($"Mật khẩu không chính xác.");
            }
            if (!user.EmailConfirmed)
            {
                return new Response<AuthenticationResponse>($"Tài khoản chưa được xác thực.");
                //throw new ApiException($"Account Not Confirmed for '{request.Email}'.");
            }
            try
            {
                var refreshToken = GenerateRefreshToken(user.Id, ipAddress);
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
                JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
                AuthenticationResponse response = new AuthenticationResponse();
                response.Id = user.Id;
                response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                response.Email = user.Email;
                response.UserName = user.UserName;
                var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                response.Roles = rolesList.ToList();
                response.IsVerified = user.EmailConfirmed;
                response.RefreshToken = refreshToken.Token;
                return new Response<AuthenticationResponse>(response, $"Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                return new Response<AuthenticationResponse>(ex.Message) { Errors = new List<string>() { ex.Message } };
                //throw new ApiException(ex.Message);
            }


        }

        public async Task<Response<string>> CreateCustomerAccountAsync(CreateNewAccountRequest request, string origin)
        {

            //check email used
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null) return new Response<string>($"Email {request.Email} đã được đăng ký.");

            var userId = Guid.NewGuid();
            //Create new user
            var user = new User
            {
                Id = userId,
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                //Test - ko cần confirm email
                //EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleConstant.Customer);
                var verificationUri = await SendVerificationEmail(user, origin);
                return new Response<string>(user.Id.ToString(), message: $"Đã tạo tài khoản. Một email đã được gửi đến {user.Email} để xác thực tài khoản.");
            }
            else
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Tạo tài khoản không thành công.",
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
        }

        //    // send mail
        //    if (await _userrepo.CommitAsync() > 0)
        //    {
        //        string Body = Extensions.MailBodyGenerate.BodyCreateAccount(req.Email, userPassword, roleName);
        //        await _emailservice.SendEmailAsync(req.Email, EmailConstant.EMAILSUBJECTCREATEACCOUNT, Body);
        //    }
        //    else
        //    {
        //        throw new Exception("Không thể tạo tài khoản");
        //    }
        //    return true;
        //}


        //public async Task<PaginationSet<AccountResponse>> GetListAccount(ListingRequest req)
        //{
        //    var AccountItems = _userrepo.GetQueryable()
        //        .Select(it => new AccountResponse()
        //        {
        //            Id = it.Id,
        //            UserName = it.UserName,
        //            IsActive = it.IsActive,
        //            RoleName = it.Role.RoleName,
        //        });
        //    if (req.Filter != null)
        //    {
        //        AccountItems = AccountItems.Filter(req.Filter);
        //    }

        //    if (req.SearchString != null)
        //    {
        //        AccountItems = AccountItems.SearchString(req.SearchString);
        //    }


        //    var result = await AccountItems.Pagination(req.PageIndex, req.PageSize, req.Sort);
        //    return result;
        //}

        //public async Task<bool> ResetPassword(Guid id)
        //{
        //    var userPassword = Extensions.PasswordGenerate.GenerateRandomCode();
        //    // update
        //    var user = await _userrepo.GetById(id);
        //    user.Password = userPassword;
        //    //send mail
        //    if (await _userrepo.CommitAsync() > 0)
        //    {
        //        string Body = Extensions.MailBodyGenerate.BodyResetPassword(user.Email, userPassword);
        //        await _emailservice.SendEmailAsync(user.Email, EmailConstant.EMAILSUBJECTRESETPASSWORD, Body);
        //    }
        //    else
        //    {
        //        throw new Exception("Không thể đổi mật khẩu");
        //    }
        //    return await _userrepo.CommitAsync() > 0;

        //        var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        //        var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
        //            .Select(f => f.Field).ToList() ?? new List<string>();
        //        if (invalidFields.Any())
        //            throw new Exception($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");



        //        var query = _userrepo.GetQueryable(x => x.IsActive).Where(it => it.Role == livestockCircleId);

        //        if (request.SearchString?.Any() == true)
        //            query = query.SearchString(request.SearchString);

        //        if (request.Filter?.Any() == true)
        //            query = query.Filter(request.Filter);

        //        var result = await query.Select(i => new ViewBarnPlanResponse
        //        {
        //            Id = i.Id,
        //            EndDate = i.EndDate,
        //            foodPlans = null,
        //            medicinePlans = null,
        //            Note = i.Note,
        //            StartDate = i.StartDate
        //        }).Pagination(request.PageIndex, request.PageSize, request.Sort);

        //        return (result);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Lỗi khi lấy danh sách: {ex.Message}");
        //    }}

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId)) return new Response<string>("The UserId field is a require.");
            if (string.IsNullOrEmpty(code)) return new Response<string>("The Code field is a require.");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new Response<string>("Không tìm thấy tài khoản.");
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return new Response<string>(user.Id.ToString(), message: $"Tài khoản đã được xác thực thành công.");
            }
            else
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Xác thực tài khoản không thành công.",
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
        }

        public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return;
            if (!account.IsActive) return;

            var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            var route = "api/account/reset-password/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            await _emailService.SendEmailAsync(model.Email, EmailConstant.EMAILSUBJECTFORGOTPASSWORD, MailBodyGenerate.BodyCreateForgotPassword(code, _enpointUri.ToString()));
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true))
            {
                return new Response<string>
                {
                    Succeeded = false,
                    Message = "Dữ liệu không hợp lệ.",
                    Errors = validationResults.Select(r => r.ErrorMessage).ToList()
                };
            }
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) return new Response<string>($"Không tìm thấy tài khoản với email {model.Email}.");
            if (!account.IsActive) return new Response<string>($"Tài khoản với email {model.Email} đã bị khóa.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if (result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Đã đặt lại mật khẩu.");
            }
            else
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Đặt lại mật khẩu không thành công.",
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
        }

        public async Task<Response<string>> ChangePassword(ChangePasswordRequest req)
        {
            if (req == null) return new Response<string>("Yêu cầu không được để trống.");           
            var validationContext = new ValidationContext(req);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(req, validationContext, validationResults, validateAllProperties: true))
            {
                return new Response<string>
                {
                    Succeeded = false,
                    Message = "Dữ liệu không hợp lệ.",
                    Errors = validationResults.Select(r => r.ErrorMessage).ToList()
                };
            }
            User user = await _userManager.FindByIdAsync(req.UserId.ToString());
            if (user == null)
            {
                return new Response<string>("Không tìm thấy tài khoản");
            }
            var result = await _userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);
            if (!result.Succeeded)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Đổi mật khẩu không thành công.",
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }
            return new Response<string>("", "Đổi mật khẩu thành công!");
        }

        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<AuthenticationResponse>("Token không hợp lệ");

            if (!refreshToken.IsActive)
                return new Response<AuthenticationResponse>("Token đã hết hạn");

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
                return new Response<AuthenticationResponse>("Không tìm thấy tài khoản.");

            // Generate new JWT token
            var jwtToken = await GenerateJWToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id, ipAddress);

            // Revoke old refresh token
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            // Save new refresh token
            newRefreshToken.UserId = user.Id;
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            var response = new AuthenticationResponse
            {
                Id = user.Id,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Email = user.Email,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsVerified = user.EmailConfirmed,
                RefreshToken = newRefreshToken.Token
            };

            return new Response<AuthenticationResponse>(response, $"Token đã được cập nhật.");
        }

        public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<string>("Token không hợp lệ");

            if (!refreshToken.IsActive)
                return new Response<string>("Token đã bị hủy");

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();

            return new Response<string>("","Hủy token thành công.");
        }

        public async Task<Response<string>> UpdateAccountAsync(UserUpdateAccountRequest request)
        {
            if (_currentUserId == Guid.Empty)
            {
                return new Response<string>("Không thể xác định người dùng hiện tại. Đăng nhập lại.");
            }

            // Tìm user theo userId
            var user = await _userManager.FindByIdAsync(_currentUserId.ToString());
            if (user == null)
            {
                return new Response<string>($"Không tìm thấy tài khoản."){
                    Errors = new List<string>(){
                        $"Không tìm thấy tài khoản với ID:{_currentUserId}"
                    }
                };
            }
            bool isChanged = false;
            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
            {
                user.Email = request.Email;
                isChanged = true;
            }
            
            if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
            {
                user.PhoneNumber = request.PhoneNumber;
                isChanged = true;
            }
            if (!string.IsNullOrEmpty(request.FullName) && user.FullName != request.FullName)
            {
                user.FullName = request.FullName;
                isChanged = true;
            }
            if (!string.IsNullOrEmpty(request.Address) && user.Address != request.Address)
            {
                user.Address = request.Address;
                isChanged = true;
            }
            if (!isChanged)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Không có thông tin thay đổi."
                };
            }
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = _currentUserId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<string>()
                {
                    Errors = result.Errors.Select(x => x.Description).ToList(),
                    Succeeded = result.Succeeded,
                    Message = "Cập nhập không thành công."
                };
            }

            return new Response<string>(null, message: $"Thông tin đã được cập nhập.");
        }

        public async Task<Response<User>> GetUserProfile()
        {
            if (_currentUserId == Guid.Empty)
            {
                return new Response<User>("Không thể xác định người dùng hiện tại. Đăng nhập lại.");
            }
            var userIdClaim = _currentUserId.ToString();
            // Tìm user theo userId
            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return new Response<User>($"Không tìm thấy tài khoản."){
                    Errors = new List<string>(){
                        $"Không tìm thấy tài khoản với ID:{_currentUserId}"
                    }
                };
            }

            return new Response<User>(user, message: "Lấy thông tin tài khoản thành công.");
        }

        private async Task<JwtSecurityToken> GenerateJWToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString()),
                new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.LifeTime),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        private string RandomTokenString()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(40));
        }
        private async Task<string> SendVerificationEmail(User user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/account/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            _emailService.SendEmailAsync(user.Email, EmailConstant.EMAILSUBJECTCONFIRMEMAIL, MailBodyGenerate.BodyCreateConfirmEmail(user.Email, verificationUri));
            return verificationUri;
        }
        private RefreshToken GenerateRefreshToken(Guid userId, string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = userId
            };
        }


    }
}
