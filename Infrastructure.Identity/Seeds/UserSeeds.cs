using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Entities.EntityModel;
using Azure.Identity;

namespace Infrastructure.Identity.Seeds
{
    public static class UserSeeds
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            var defaultUser = new User
            {
                UserName = "Admin",
                Email = "admin@123",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser);
                await userManager.AddToRoleAsync(defaultUser, "Admin");
            }
        }
    }
}
