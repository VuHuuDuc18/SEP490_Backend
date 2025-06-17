using Domain.Dto.Request;
using Domain.Dto.Request.Account;
using Domain.Dto.Response.Account;
using Domain.Dto.Response;
using Domain.Helper.Constants;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Domain.Extensions;

namespace Domain.Services.Implements
{
    public class UserService : IUserService
    {
        public readonly IRepository<User> _userrepo;
        public readonly IEmailService _emailservice;
        public readonly IRepository<Role> _rolerepo;
        public UserService(IRepository<User> rp, IEmailService em, IRepository<Role> r)
        {
            _userrepo = rp;
            _emailservice = em;
            _rolerepo = r;
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest req)
        {
            // confirm
            if (!req.NewPassword.Equals(req.ConfirmPassword))
            {
                throw new Exception("Mật khẩu xác nhận không trùng khớp");
            }
            // old pass cf
            var user = await _userrepo.GetById(req.UserId);
            if (!user.Password.Equals(req.OldPassword))
            {
                throw new Exception("Mật khẩu cũ không khớp");
            }
            user.Password = req.NewPassword;
            return await _userrepo.CommitAsync() > 0;
        }

        public async Task<bool> CreateAccount(CreateAccountRequest req)
        {
            // bien
            var userPassword = Extensions.PasswordGenerate.GenerateRandomCode();
            string roleName = (await _rolerepo.GetById(req.RoleId)).RoleName;
            
            // ínert
            _userrepo.Insert(new User()
            {
                Email = req.Email,
                Password = userPassword,
                RoleId = req.RoleId,
                UserName = req.UserName
            });


            // send mail
            if (await _userrepo.CommitAsync() > 0)
            {
                string Body = Extensions.MailBodyGenerate.BodyCreateAccount(req.Email, userPassword, roleName);
                await _emailservice.SendEmailAsync(req.Email, EmailConstant.EMAILSUBJECTCREATEACCOUNT, Body);
            }
            else
            {
                throw new Exception("Không thể tạo tài khoản");
            }
            return true;
        }

        
        public async Task<PaginationSet<AccountResponse>> GetListAccount(ListingRequest req)
        {
            var AccountItems = _userrepo.GetQueryable()
                .Select(it => new AccountResponse()
            {
                    Id = it.Id,
                    UserName = it.UserName,
                    IsActive = it.IsActive,
                    RoleName = it.Role.RoleName,
            });
            if (req.Filter != null)
            {
                AccountItems = AccountItems.Filter(req.Filter);
            }

            if (req.SearchString != null)
            {
                AccountItems = AccountItems.SearchString(req.SearchString);
            }


            var result = await AccountItems.Pagination(req.PageIndex, req.PageSize, req.Sort);
            return result;
        }

        public async Task<bool> ResetPassword(Guid id)
        {
            var userPassword = Extensions.PasswordGenerate.GenerateRandomCode();
            // update
            var user = await _userrepo.GetById(id);
            user.Password = userPassword;
            //send mail
            if (await _userrepo.CommitAsync() > 0)
            {
                string Body = Extensions.MailBodyGenerate.BodyResetPassword(user.Email, userPassword);
                await _emailservice.SendEmailAsync(user.Email, EmailConstant.EMAILSUBJECTRESETPASSWORD, Body);
            }
            else
            {
                throw new Exception("Không thể đổi mật khẩu");
            }
            return await _userrepo.CommitAsync() > 0;
        }
    }
}
