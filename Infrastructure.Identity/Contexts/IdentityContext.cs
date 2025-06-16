using Entities.EntityModel;
using Infrastructure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Contexts
{
    public class IdentityContext : IdentityDbContext<User,Role,Guid>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options):base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "User");
            });

            builder.Entity<Role>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");
                //in case you chagned the TKey type
                  entity.HasKey(key => new { key.UserId, key.RoleId });
            });
            
            //builder.Entity<UserClaims>(entity =>
            //{
            //    entity.ToTable("UserClaims");
            //    //entity.HasKey(key => new {key.UserId, key.Id });
            //});

            //builder.Entity<UserLogins>(entity =>
            //{
            //    entity.ToTable("UserLogins");
            //    //in case you chagned the TKey type
            //    //entity.HasKey(key => new { key.UserId, key.ProviderKey, key.LoginProvider});       
            //});

            //builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            //{
            //    entity.ToTable("RoleClaims");
            //    //entity.HasKey(key => new { key.RoleId, key.Id });

            //});

            //builder.Entity<IdentityUserToken<Guid>>(entity =>
            //{
            //    entity.ToTable("UserTokens");
            //    //in case you chagned the TKey type
            //    entity.HasKey(key => new { key.UserId, key.LoginProvider, key.Name });
            //});
        }
    }
}
