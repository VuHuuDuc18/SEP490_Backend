using Entities.EntityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Seeds
{
    public static class RoleSeeds
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            await roleManager.CreateAsync(new Role("Admin"));
            await roleManager.CreateAsync(new Role("User"));
        }
    }
}
