using Entities.EntityModel;
using Infrastructure.Identity.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Runtime.CompilerServices;
namespace SEP490_BackendAPI.Extensions
{
    public static class ServicesExtentions
    {
        public static void AddSwaggerExtensions(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "LCFMSystem",
                    Description = "This Api will be responsible for overall data distribution and authorization.",
                    //Contact = new OpenApiContact
                    //{
                    //    Name = "LCFMSystem",
                    //    Email = "hello@codewithmukesh.com",
                    //    Url = new Uri("https://codewithmukesh.com/contact"),
                    //}
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
                //options.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }
        
        public static async Task SeedIdentity(this IServiceProvider servicesProvider)
        {
            var userManager = servicesProvider.GetRequiredService<UserManager<User>>();
            var roleManaer = servicesProvider.GetRequiredService<RoleManager<Role>>();
            await RoleSeeds.SeedAsync(roleManaer);
            await UserSeeds.SeedAsync(userManager, roleManaer);
        }
    }
}
