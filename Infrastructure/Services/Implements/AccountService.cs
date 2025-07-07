using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Request.User;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Domain.Extensions;
using Domain.Helper.Constants;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Identity.Helpers;
using Infrastructure.Identity.Models;
using Infrastructure.Repository;
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
        public AccountService(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<JWTSettings> jwtSettings,
            SignInManager<User> signInManager,
            IEmailService emailService,
            IdentityContext context,
            IRepository<User> userRepository
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            this._emailService = emailService;
            _context = context;
            _userRepository = userRepository;
        }


        public async Task<Response<List<User>>> GetAllAccountsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return new Response<List<User>>(users, message: $"All Accounts Retrieved Successfully.");
        }
        public async Task<Response<User>> GetAccountByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<User>($"No Accounts Registered with {email}.");
            return new Response<User>(user, message: $"Account Retrieved Successfully.");
        }
        public async Task<Response<string>> CreateAccountAsync(CreateAccountRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.FullName);
            if (userWithSameUserName != null)
            {
                return new Response<string>($"Username '{request.FullName}' is already taken.");
            }
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, request.RoleName);
                    var verificationUri = await SendVerificationEmail(user, origin);
                    return new Response<string>(user.Id.ToString(), message: $"User Registered. An email has been sent to {user.Email} to confirm your account.");
                }
                else
                {
                    return new Response<string>($"{result.Errors}");
                }
            }
            else
            {
                return new Response<string>($"Email {request.Email} is already registered.");
            }
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
        public async Task<Response<string>> DeleteAccount(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"No Accounts Registered with {email}.");
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return new Response<string>(email, message: $"Account Deleted Successfully.");
            }
            else
            {
                throw new ApiException($"Error occured while deleting the account.");
            }
        }
        public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<string>("Invalid refresh token");

            if (!refreshToken.IsActive)
                return new Response<string>("Token already revoked");

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();

            return new Response<string>("Token revoked successfully");
        }
        public async Task<Response<string>> DisableAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"No Accounts Registered with {email}.");
            if (!user.IsActive)
            {
                return new Response<string>($"Account already disabled - {email}.");
            }
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            return new Response<string>(email, message: $"Account Disabled Successfully.");
        }
        public async Task<Response<string>> EnableAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<string>($"No Accounts Registered with {email}.");
            if (user.IsActive)
            {
                return new Response<string>($"Account already enabled - {email}.");
            }
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            return new Response<string>(email, message: $"Account Enabled Successfully.");
        }
        public async Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new Response<string>($"No Accounts Registered with {request.UserId}.");
            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.UserName)) user.UserName = request.UserName;
            await _userManager.UpdateAsync(user);
            return new Response<string>(user.Id.ToString(), message: $"Account Updated Successfully.");
        }
        #region Old Code

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
                        FullName = user.FullName,
                        IsActive = user.IsActive,
                        RoleName = role
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
        #endregion
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
    }
}
