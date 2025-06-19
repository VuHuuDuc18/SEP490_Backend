using Domain.Dto.Request.BarnPlan;
using Domain.Dto.Response.BarnPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBarnPlanService
    {
        public Task<ViewBarnPlanResponse>  GetById(Guid id);
        public Task<bool> CreateBarnPlan(CreateBarnPlanRequest req);
    }
}
