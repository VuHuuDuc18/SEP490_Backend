using Domain.Dto.Request;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
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
            var random = new Random();
            byte[] bytes = new byte[6];
            random.NextBytes(bytes);
          
            //string generatedPassword = System.Text.Encoding.UTF8.GetString(bytes);
            //_userrepo.Insert(new User()
            //{
            //    Email = req.Email,
            //    Password = generatedPassword,
            //    RoleId = req.RoleId,
            //    UserName = req.UserName
            //});
            return await _userrepo.CommitAsync()>0;
        }

        public List<User> GetListUser()
        {
            throw new NotImplementedException();
        }
    }
}
