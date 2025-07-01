using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        //public Task<bool> CreateAccount(CreateAccountRequest req);
        //public Task<bool> ResetPassword(Guid id);
        //public Task<bool> ChangePassword(ChangePasswordRequest req);
        //public Task<PaginationSet<AccountResponse>> GetListAccount(ListingRequest req);
        Task<Response<AuthenticationResponse>> LoginAsync(AuthenticationRequest request, string ipAddress);
        Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin);
        Task<Response<string>> ConfirmEmailAsync(string userId, string code);
        Task ForgotPassword(ForgotPasswordRequest model, string origin);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<AuthenticationResponse>> RefreshTokenAsync(string token, string ipAddress);
        Task<Response<string>> RevokeTokenAsync(string token, string ipAddress);
        Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request);
        Task<Response<User>> GetUserProfile();
    }
}
