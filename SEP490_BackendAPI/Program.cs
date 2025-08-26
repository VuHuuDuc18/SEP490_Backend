﻿using Infrastructure.Identity;
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
using Domain.IServices;
using Infrastructure.DBContext;
using Infrastructure.Repository;
using Domain.Settings;
using System.Threading.Tasks;

namespace SEP490_BackendAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //cors
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
            //builder.Services.Configure<MailSendSettings>(builder.Configuration.GetSection("MailSettings"));
            
            //Add Infrastructures
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddIdentityInfrastructure(builder.Configuration);

            // Add Service
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Add services to the container.
            builder.Services.AddSwaggerExtensions();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            var app = builder.Build();

            app.UseCors("AllowAllOrigins");

            await app.MigrateDatabaseAsync();
            await app.Services.SeedIdentity();
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            //Tự động redirect qua https nếu có
            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
