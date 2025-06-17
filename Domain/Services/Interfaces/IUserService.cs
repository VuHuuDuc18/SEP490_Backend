using Domain.Dto.Request;
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
        public Task<bool> ResetPassword(Guid id);
        public Task<bool> ChangePassword(ChangePasswordRequest req);
        public List<User> GetListAccount();
    }
}
