using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Implements
{
    public class UserService : IUserService
    {
        public readonly IRepository<User> _userrepo;
        public UserService(IRepository<User> rp)
        {
            _userrepo = rp;
        }
        public async Task<bool> CreateAccount(CreateAccountRequest req)
        {
           
            _userrepo.Insert(new User()
            {
                Email = req.Email,
                Password = Extensions.PasswordGenerate.GenerateRandomCode(),
                RoleId = req.RoleId,
                UserName = req.UserName
            });
            return await _userrepo.CommitAsync()>0;
        }

        public List<User> GetListUser()
        {
            throw new NotImplementedException();
        }
    }
}
