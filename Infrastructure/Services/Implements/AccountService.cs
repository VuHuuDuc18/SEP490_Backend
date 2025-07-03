using Domain.Dto.Request.Account;
using Application.Interfaces;
using Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
using Entities.EntityModel;
using Application.Wrappers;
using Application.Exceptions;
using Infrastructure.Identity.Helpers;
using Domain.Helper.Constants;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Contexts;
using Domain.Dto.Request.User;
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
        public AccountService(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<JWTSettings> jwtSettings,
            SignInManager<User> signInManager,
            IEmailService emailService,
            IdentityContext context
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            this._emailService = emailService;
            _context = context;
        }


        public async Task<Response<List<User>>> GetAllAccountsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return new Response<List<User>>(users, message: $"All Accounts Retrieved Successfully.");
        }
        public async Task<Response<User>> GetAccountByEmailAsync(string email){
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<User>($"No Accounts Registered with {email}.");
            return new Response<User>(user, message: $"Account Retrieved Successfully.");
        }
        public async Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin)
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
