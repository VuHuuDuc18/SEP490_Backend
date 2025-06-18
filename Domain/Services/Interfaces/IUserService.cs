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
        public Task<bool> CreateAccount(CreateAccountRequest req);
        public Task<bool> ResetPassword(string email);
        public Task<bool> ChangePassword(ChangePasswordRequest req);
        public Task<PaginationSet<AccountResponse>> GetListAccount(ListingRequest req);
    }
}
