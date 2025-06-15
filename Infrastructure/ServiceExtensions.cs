using Domain.Services;
using Domain.Services.Implements;
using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
    public static class ServiceExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            //Add service
            services.AddScoped<IBarnService, BarnService>();
            services.AddScoped<IFoodService, FoodService>();
            services.AddScoped<IMedicineService, MedicineService>();
            services.AddScoped<ILivestockCircleService, LivestockCircleService>();
            services.AddScoped<IFoodCategoryService, FoodCategoryService>();
            services.AddScoped<IBreedCategoryService, BreedCategoryService>();
            services.AddScoped<IMedicineCategoryService, MedicineCategoryService>();
            services.AddScoped<IBreedService, BreedService>();
            services.AddScoped<CloudinaryCloudService>();

            //Add repo
            services.AddScoped<IRepository<Barn>, Repository<Barn>>();
            services.AddScoped<IRepository<Food>, Repository<Food>>();
            services.AddScoped<IRepository<Medicine>, Repository<Medicine>>();
            services.AddScoped<IRepository<ImageFood>, Repository<ImageFood>>();
            services.AddScoped<IRepository<ImageMedicine>, Repository<ImageMedicine>>();
            services.AddScoped<IRepository<FoodCategory>, Repository<FoodCategory>>();
            services.AddScoped<IRepository<BreedCategory>, Repository<BreedCategory>>();
            services.AddScoped<IRepository<Breed>, Repository<Breed>>();
            services.AddScoped<IRepository<ImageBreed>, Repository<ImageBreed>>();
            services.AddScoped<IRepository<MedicineCategory>, Repository<MedicineCategory>>();
            services.AddScoped<IRepository<LivestockCircle>, Repository<LivestockCircle>>();
        }

    }
}
