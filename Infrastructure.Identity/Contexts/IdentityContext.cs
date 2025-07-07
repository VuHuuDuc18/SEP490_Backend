using Entities.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Identity.Models;
namespace Infrastructure.Identity.Contexts
{
    public class IdentityContext : IdentityDbContext<User,Role,Guid>
    {
        public virtual DbSet<Barn> Barns { get; set; }
        public virtual DbSet<BarnPlan> BarnPlans { get; set; }
        public virtual DbSet<BarnPlanFood> BarnPlanFoods { get; set; }
        public virtual DbSet<BarnPlanMedicine> BarnPlanMedicines { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<BillItem> BillItems { get; set; }
        public virtual DbSet<Breed> Breeds { get; set; }
        public virtual DbSet<BreedCategory> BreedCategories { get; set; }
        public virtual DbSet<DailyReport> DailyReports { get; set; }
        public virtual DbSet<Food> Foods { get; set; }
        public virtual DbSet<FoodCategory> FoodCategories { get; set; }
        public virtual DbSet<FoodReport> FoodReports { get; set; }
        public virtual DbSet<LivestockCircle> LivestockCircles { get; set; }
        public virtual DbSet<LivestockCircleFood> LivestockCircleFoods { get; set; }
        public virtual DbSet<LivestockCircleMedicine> LivestockCircleMedicines { get; set; }
        public virtual DbSet<Medicine> Medicines { get; set; }
        public virtual DbSet<MedicineReport> MedicineReports { get; set; }
        public virtual DbSet<MedicineCategory> MedicineCategories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ImageFood> ImageFoods { get; set; }
        public virtual DbSet<ImageMedicine> ImageMedicines { get; set; }
        public virtual DbSet<ImageBreed> ImageBreeds { get; set; }
        public virtual DbSet<ImageLivestockCircle> ImageLivestockCircles { get; set; }
        public virtual DbSet<ImageDailyReport> ImageDailyReports { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
                entity.ToTable(name: "Users");
            });

            builder.Entity<Role>(entity =>
            {
                entity.ToTable(name: "Roles");
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

            builder.Entity<RefreshToken>()
                .HasKey(x => x.Id);

            builder.Entity<RefreshToken>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
