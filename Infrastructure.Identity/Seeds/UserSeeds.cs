using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Entities.EntityModel;
using Azure.Identity;
using Domain.Helper.Constants;

namespace Infrastructure.Identity.Seeds
{
    public static class UserSeeds
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            var defaultUser = new User
            {
                UserName = "Admin",
                Email = "admin@a",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, RoleConstant.CompanyAdmin);
                }
                else
                {
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(user, RoleConstant.CompanyAdmin))
                {
                    await userManager.AddToRoleAsync(user, RoleConstant.CompanyAdmin);
                }
            }
        }

    }
}
