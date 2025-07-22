using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.BarnPlan;
using Domain.Dto.Response;
using Domain.Dto.Response.BarnPlan;

namespace Domain.IServices
{
    public interface IBarnPlanService
    {
        public Task<Response<ViewBarnPlanResponse>>  GetById(Guid id);
        public Task<Response<ViewBarnPlanResponse>> GetByLiveStockCircleId(Guid id);
        public Task<Response<bool>> CreateBarnPlan(CreateBarnPlanRequest req);
        public Task<Response<bool>> UpdateBarnPlan(UpdateBarnPlanRequest req);
        public Task<Response<bool>> DisableBarnPlan(Guid id);
        public Task<Response<PaginationSet<ViewBarnPlanResponse>>> ListingHistoryBarnPlan(Guid livestockCircleId,ListingRequest req);
    }
}
