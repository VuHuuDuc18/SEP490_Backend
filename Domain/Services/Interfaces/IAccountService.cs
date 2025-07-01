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
        Task<Response<List<User>>> GetAllAccountsAsync();
        Task<Response<User>> GetAccountByEmailAsync(string email);
        Task<Response<string>> CreateAccountAsync(CreateNewAccountRequest request, string origin);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<string>> DeleteAccount(string email);
        Task<Response<string>> DisableAccountAsync(string email);
        Task<Response<string>> EnableAccountAsync(string email);
        Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request);
    }
}
