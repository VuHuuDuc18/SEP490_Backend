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
        public virtual DbSet<Barn> Barns { get; set; }
        public virtual DbSet<BarnPlan> BarnPlans { get; set; }
        public virtual DbSet<BarnPlanFood> BarnPlanFoods { get; set; }
        public virtual DbSet<BarnPlanMedicine> BarnPlanMedicines { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<BillItem> BillItems { get; set; }
        public virtual DbSet<Breed> Breeds  { get; set; }
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
        public virtual DbSet<Notification> Notifications { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseLazyLoadingProxies();

    }
}
