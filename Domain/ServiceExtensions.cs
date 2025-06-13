using Domain.Services.Interfaces;
using Entities.EntityModel;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class ServiceExtensions
    {

        public static void AddDomain(this IServiceCollection services)
        {
            //Add service
            services.AddScoped<IBarnService, BarnService>();
            services.AddScoped<ILivestockCircleService, LivestockCircleService>();
            

            //Add repo
            services.AddScoped<IRepository<Barn>, Repository<Barn>>();
            services.AddScoped<IRepository<LivestockCircle>, Repository<LivestockCircle>>();
        }
    }
}
