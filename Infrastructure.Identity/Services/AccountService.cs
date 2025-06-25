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
using Infrastructure.Services;
using Domain.Helper.Constants;
using Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Contexts;
namespace Infrastructure.Identity.Services
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
                return new Response<AuthenticationResponse>(ex.Message) { Errors = new List<string>() { ex.Message} };
            }


        }

        public async Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);            
            if (userWithSameUserName != null)
            {
                return new Response<string>($"Username '{request.UserName}' is already taken.");
            }
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                    var verificationUri = await SendVerificationEmail(user, origin);
                    //TODO: Attach Email Service here and configure it via appsettings
                    //await _emailService.SendAsync(new Application.DTOs.Email.EmailRequest() { From = "mail@codewithmukesh.com", To = user.Email, Body = $"Please confirm your account by visiting this URL {verificationUri}", Subject = "Confirm Registration" });
                    //return new Response<string>(user.Id.ToString(), message: $"User Registered. Please confirm your account by visiting this URL {"verificationUri"}");
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

        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                return new Response<AuthenticationResponse>("Invalid refresh token");

            if (!refreshToken.IsActive)
                return new Response<AuthenticationResponse>("Token expired");

            var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
            if (user == null)
                return new Response<AuthenticationResponse>("User not found");

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
        public async Task<Response<List<User>>> GetAllAccountsAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return new Response<List<User>>(users, message: $"All Accounts Retrieved Successfully.");
        }
        public async Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return new Response<string>($"No Accounts Registered with {request.UserId}.");
            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (!string.IsNullOrEmpty(request.UserName)) user.UserName = request.UserName;
            await _userManager.UpdateAsync(user);
            return new Response<string>(user.Id.ToString(), message: $"Account Updated Successfully.");
        }
        public async Task<Response<User>> GetAccountByEmailAsync(string email){
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return new Response<User>($"No Accounts Registered with {email}.");
            return new Response<User>(user, message: $"Account Retrieved Successfully.");
        }
    }
}
