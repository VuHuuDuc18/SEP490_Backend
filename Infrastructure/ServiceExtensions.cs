using Domain.IServices;
using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.DBContext;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Add DBContext
            services.AddDbContext<LCFMSDBContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
               ServiceLifetime.Transient);
            services.AddScoped<DbContext, LCFMSDBContext>();

            //Add Cloudiary Config to DI Container
            services.Configure<CloudinaryConfig>(configuration.GetSection("Cloudinary"));
            //Register services
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IBarnPlanService, BarnPlanService>();
            services.AddTransient<IUserService, UserService>();
            services.AddScoped<IBarnService, BarnService>();
            services.AddScoped<IFoodService, FoodService>();
            services.AddScoped<IMedicineService, MedicineService>();
            services.AddScoped<ILivestockCircleService, LivestockCircleService>();
            services.AddScoped<IFoodCategoryService, FoodCategoryService>();
            services.AddScoped<IBreedCategoryService, BreedCategoryService>();
            services.AddScoped<IMedicineCategoryService, MedicineCategoryService>();
            services.AddScoped<IBreedService, BreedService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IDailyReportService, DailyReportService>();
            services.AddScoped<IBillService, BillService>();
            services.AddScoped<CloudinaryCloudService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<INotificationService, NotificationServices>();

            //Add repo
            services.AddScoped<IRepository<Notification>, Repository<Notification>>();
            services.AddScoped<IRepository<BarnPlan>, Repository<BarnPlan>>();
            services.AddScoped<IRepository<BarnPlanFood>, Repository<BarnPlanFood>>();
            services.AddScoped<IRepository<BarnPlanMedicine>, Repository<BarnPlanMedicine>>();
            services.AddScoped<IRepository<Role>, Repository<Role>>();
            services.AddScoped<IRepository<Bill>, Repository<Bill>>();
            services.AddScoped<IRepository<BillItem>, Repository<BillItem>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Barn>, Repository<Barn>>();
            services.AddScoped<IRepository<Food>, Repository<Food>>();
            services.AddScoped<IRepository<Medicine>, Repository<Medicine>>();
            services.AddScoped<IRepository<ImageFood>, Repository<ImageFood>>();
            services.AddScoped<IRepository<ImageMedicine>, Repository<ImageMedicine>>();
            services.AddScoped<IRepository<ImageDailyReport>, Repository<ImageDailyReport>>();
            services.AddScoped<IRepository<FoodCategory>, Repository<FoodCategory>>();
            services.AddScoped<IRepository<BreedCategory>, Repository<BreedCategory>>();
            services.AddScoped<IRepository<Breed>, Repository<Breed>>();
            services.AddScoped<IRepository<ImageBreed>, Repository<ImageBreed>>();
            services.AddScoped<IRepository<MedicineCategory>, Repository<MedicineCategory>>();
            services.AddScoped<IRepository<LivestockCircle>, Repository<LivestockCircle>>();
            services.AddScoped<IRepository<FoodReport>, Repository<FoodReport>>();
            services.AddScoped<IRepository<MedicineReport>, Repository<MedicineReport>>();
            services.AddScoped<IRepository<LivestockCircleFood>, Repository<LivestockCircleFood>>();
            services.AddScoped<IRepository<LivestockCircleMedicine>, Repository<LivestockCircleMedicine>>();
            services.AddScoped<IRepository<DailyReport>, Repository<DailyReport>>();

            // Thêm Background Service (Singleton)
            services.AddHostedService<LivestockWeightUpdateEmailService>();

            // Đăng ký interface
            services.AddScoped<ILivestockWeightUpdateEmailService, LivestockWeightUpdateEmailService>();




        }

    }
}
