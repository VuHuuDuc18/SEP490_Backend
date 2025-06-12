using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.EntityModel;
using Infrastructure.Core;
namespace Infrastructure.DBContext
{
    public class LCFMSDBContext : DbContext
    {
        public LCFMSDBContext(DbContextOptions<LCFMSDBContext> options) : base(options)
        {
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var RoleAdminId = Guid.NewGuid();


            modelBuilder.Entity<Role>().HasData(
                            new Role() { Id = RoleAdminId, RoleName = CoreRoleName.RoleNames[0], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[1], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[2], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[3], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[4], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[5], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[6], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null },
                            new Role() { Id = Guid.NewGuid(), RoleName = CoreRoleName.RoleNames[7], CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null }
                            );
            modelBuilder.Entity<User>().HasData(
                new User() { Id = Guid.NewGuid(), Email = "admin@a", Password = "123", RoleId = RoleAdminId, UserName = "Company Admin", CreatedBy = Guid.Empty, CreatedDate = DateTime.Now, IsActive = true, UpdatedBy = null, UpdatedDate = null }
                );

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseLazyLoadingProxies();

    }
}
