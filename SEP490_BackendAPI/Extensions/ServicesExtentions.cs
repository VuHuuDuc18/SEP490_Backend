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
                    Title = "LCFM System API",
                    Description = "Livestock Circle Feed Management System - API for managing livestock, feeds, medicines, and daily reports.",
                    Contact = new OpenApiContact
                    {
                        Name = "LCFM System",
                        Email = "luongcongduy826@gmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "LCFM License",
                        Url = new Uri("https://example.com/license")
                    }
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
