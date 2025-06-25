using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<Response<AuthenticationResponse>> LoginAsync(AuthenticationRequest request, string ipAddress);
        Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin);
        Task<Response<string>> DeleteAccount(string email);
        Task<Response<string>> ConfirmEmailAsync(string userId, string code);
        Task ForgotPassword(ForgotPasswordRequest model, string origin);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token, string ipAddress);
        Task<Response<string>> RevokeTokenAsync(string token, string ipAddress);
        Task<Response<string>> DisableAccountAsync(string email);
        Task<Response<string>> EnableAccountAsync(string email);
        Task<Response<List<User>>> GetAllAccountsAsync();
    }
}
