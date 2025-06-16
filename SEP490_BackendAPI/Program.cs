using Infrastructure.Identity;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using SEP490_BackendAPI.Extensions;
using Microsoft.EntityFrameworkCore.Design;
using Domain.Services;
using Infrastructure.DBContext;
using Infrastructure.Repository;

namespace SEP490_BackendAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // cors
            //builder.Services.AddCors(options =>

            //{
            //    options.AddPolicy("AllowAllOrigins",
            //        builder =>
            //        {
            //            builder.AllowAnyOrigin()
            //                   .AllowAnyMethod()
            //                   .AllowAnyHeader();
            //        });
            //});
           //builder.Services.Configure<MailSendSettings>(builder.Configuration.GetSection("MailSettings"));
            //connect DB SQL
            builder.Services.AddDbContext<LCFMSDBContext>(options => 
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), 
               ServiceLifetime.Transient);
            builder.Services.AddScoped<DbContext, LCFMSDBContext>();
            
            //Add service extensions
            builder.Services.Configure<CloudinaryConfig>(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.AddInfrastructure(builder.Configuration);

            // Add Service
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //builder.Services.AddTransient<IUserService,UserService>() ;

            // Add services to the container.
            builder.Services.AddSwaggerExtensions();

            builder.Services.AddIdentityInfrastructure(builder.Configuration);

            var servicesProvider = builder.Services.BuildServiceProvider();
            ServicesExtentions.SeedIdentity(servicesProvider);

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
