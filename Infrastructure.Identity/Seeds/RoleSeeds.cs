using Entities.EntityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Helper.ValueObjects;

namespace Infrastructure.Identity.Seeds
{
    public static class RoleSeeds
    {
        public static async Task SeedAsync(RoleManager<Role> roleManager)
        {
            foreach (var role in CoreRoleName.RoleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Role { Name = role });
                } 
            }
            await roleManager.CreateAsync(new Role { Name = "Admin" });
        }

    }
}
