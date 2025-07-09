using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Entities.EntityModel;

namespace Domain.IServices
{
    public interface IAccountService
    {
        Task<Response<List<User>>> GetAllAccountsAsync();
        Task<Response<User>> GetAccountByEmailAsync(string email);
        Task<Response<string>> CreateAccountAsync(CreateAccountRequest request, string origin);
        Task<Response<string>> ResetPassword(ResetPasswordRequest model);
        Task<Response<string>> DeleteAccount(string email);
        Task<Response<string>> DisableAccountAsync(string email);
        Task<Response<string>> EnableAccountAsync(string email);
        Task<Response<string>> UpdateAccountAsync(UpdateAccountRequest request);
        Task<Response<PaginationSet<AccountResponse>>> GetListAccount(ListingRequest req);
    }
}
