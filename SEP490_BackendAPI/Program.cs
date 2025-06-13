using Entities.EntityModel;
using Infrastructure.DBContext;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using SEP490_BackendAPI.Extensions;
using Microsoft.EntityFrameworkCore.Design;

namespace SEP490_BackendAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // cors
            builder.Services.AddCors(options =>

            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
            // connect DB SQL
            //builder.Services.AddDbContext<LCFMSDBContext>(options => 
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), 
            //    ServiceLifetime.Transient);
            //builder.Services.AddScoped<DbContext, LCFMSDBContext>();

            
            // Add services to the container.
            builder.Services.AddSwaggerExtensions();

            builder.Services.AddIdentityInfrastructure(builder.Configuration);

            //var servicesProvider = builder.Services.BuildServiceProvider();
            //ServicesExtentions.SeedIdentity(servicesProvider);
            //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
