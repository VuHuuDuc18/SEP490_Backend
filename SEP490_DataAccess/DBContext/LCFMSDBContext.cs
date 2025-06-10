using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.EntityModel;

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
            List<User> users = new List<User>()
            {

            };

            Users.AddRange(users);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseLazyLoadingProxies();

    }
}
