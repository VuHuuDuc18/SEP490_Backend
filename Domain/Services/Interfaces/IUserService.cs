using Domain.Dto.Request.Account;
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
        public List<User> GetListUser();
    }
}
