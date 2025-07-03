using Application.Exceptions;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Domain.Extensions;
using Domain.Helper.Constants;
using Domain.Services.Interfaces;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Identity.Helpers;
using Infrastructure.Identity.Models;
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
        }
        #region Old Code
        

        //public async Task<bool> CreateAccount(CreateAccountRequest req)
        //{
        //    // bien
        //    var userPassword = PasswordGenerate.GenerateRandomCode();
        //    string roleName = (await _rolerepo.GetById(req.RoleId)).Name;

        //    // ínert
        //    _userrepo.Insert(new User()
        //    {
        //        Email = req.Email,
        //        Password = userPassword,
        //        RoleId = req.RoleId,
        //        UserName = req.UserName
        //    });


        //    // send mail
        //    if (await _userrepo.CommitAsync() > 0)
        //    {
        //        string Body = MailBodyGenerate.BodyCreateAccount(req.Email, userPassword);
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
        //}
        #endregion


        public async Task<Response<AuthenticationResponse>> LoginAsync(AuthenticationRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new Response<AuthenticationResponse>($"No Accounts Registered with {request.Email}.");
                //throw new ApiException($"No Accounts Registered with {request.Email}.");
            }
            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                //throw new ApiException($"Invalid Credentials for '{request.Email}'.");
                return new Response<AuthenticationResponse>($"Invalid Credentials for '{request.Email}'.");
            }
            if (!user.EmailConfirmed)
            {
                return new Response<AuthenticationResponse>($"Account Not Confirmed for '{request.Email}'.");
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
                return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
            }
            catch (Exception ex)
            {
                return new Response<AuthenticationResponse>(ex.Message) { Errors = new List<string>() { ex.Message } };
            }


        }

        public async Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin)
        {
            //Check username exists
            //var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            //if (userWithSameUserName != null)
            //{
            //    return new Response<string>($"Username '{request.UserName}' is already taken.");
            //}
            //check email used
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null) return new Response<string>($"Email {request.Email} is already registered.");

            //Create new user
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };
            user.CreatedBy = user.Id;
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, request.RoleName);
                var verificationUri = await SendVerificationEmail(user, origin);
                return new Response<string>(user.Id.ToString(), message: $"User Registered. An email has been sent to {user.Email} to confirm your account.");
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

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return new Response<string>(user.Id.ToString(), message: $"Account Confirmed for {user.Email}.");
            }
            else
            {
                throw new ApiException($"An error occured while confirming {user.Email}.");
            }
        }

        public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return;

            var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            var route = "api/account/reset-password/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            await _emailService.SendEmailAsync(model.Email, EmailConstant.EMAILSUBJECTFORGOTPASSWORD, MailBodyGenerate.BodyCreateForgotPassword(code, _enpointUri.ToString()));
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if (result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Password Resetted.");
            }
            else
            {
                throw new ApiException($"Error occured while reseting the password.");
            }
        }

        public async Task<Response<string>> ChangePassword(ChangePasswordRequest req)
        {
            User user = await _userManager.FindByIdAsync(req.UserId.ToString());
            if (user == null)
            {
                return new Response<string>("Tài khoản không tồn tại");
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
                return new Response<AuthenticationResponse>("Refresh token không hợp lệ");

            if (!refreshToken.IsActive)
                return new Response<AuthenticationResponse>("Refresh token đã hết hạn");

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
                return new Response<AuthenticationResponse>("Không tìm thấy tài khoản");

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

            return new Response<AuthenticationResponse>(response, $"Token refreshed for {user.UserName}");
        }

        public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<string>("Refresh token không hợp lệ");

            if (!refreshToken.IsActive)
                return new Response<string>("Refresh token đã bị hủy");

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();

            return new Response<string>("Hủy refresh token thành công");
        }

        public async Task<Response<string>> UpdateAccountAsync(UserUpdateAccountRequest request)
        {
            // Lấy current user từ JWT token claims
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
            {
                return new Response<string>("Không thể xác định người dùng hiện tại.");
            }

            // Lấy userId từ claims
            var userIdClaim = currentUser.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new Response<string>("Không tìm thấy thông tin người dùng trong token.");
            }

            // Tìm user theo userId
            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return new Response<string>($"Không tìm thấy tài khoản với ID {userIdClaim}.");
            }
            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = Guid.Parse(userIdClaim);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<string>()
                {
                    Errors = result.Errors.Select(x=>x.Description).ToList(),
                    Succeeded = result.Succeeded,
                    Message = "Cập nhập không thành công."
                };
            }

            return new Response<string>(null, message: $"Thông tin đã được cập nhập.");
        }

        public async Task<Response<User>> GetUserProfile()
        {
            // Lấy current user từ JWT token claims
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
            {
                return new Response<User>("Không thể xác định người dùng hiện tại.");
            }

            // Lấy userId từ claims
            var userIdClaim = currentUser.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new Response<User>("Không tìm thấy thông tin người dùng trong token.");
            }

            // Tìm user theo userId
            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return new Response<User>($"Không tìm thấy tài khoản với ID {userIdClaim}.");
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
