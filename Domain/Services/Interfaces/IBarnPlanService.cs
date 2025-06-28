using Domain.Dto.Request;
using Domain.Dto.Request.BarnPlan;
using Domain.Dto.Response;
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
        public Task<ViewBarnPlanResponse> GetByLiveStockCircleId(Guid id);
        public Task<bool> CreateBarnPlan(CreateBarnPlanRequest req);
        public Task<bool> UpdateBarnPlan(UpdateBarnPlanRequest req);
        public Task<bool> DisableBarnPlan(Guid id);
        public Task<PaginationSet<ViewBarnPlanResponse>> ListingHistoryBarnPlan(Guid livestockCircleId,ListingRequest req);
    }
}
