using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Barn;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.Medicine;
using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Domain.IServices
{
    public interface IBarnService
    {

        Task<Response<string>> CreateBarn(CreateBarnRequest requestDto, CancellationToken cancellationToken = default);

        Task<Response<string>> UpdateBarn(UpdateBarnRequest requestDto, CancellationToken cancellationToken = default);

        Task<Response<string>> DisableBarn(Guid barnId, CancellationToken cancellationToken = default);

        Task<Response<BarnResponse>> GetBarnById(Guid barnId, CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<BarnResponse>>> GetBarnByWorker(ListingRequest request, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<BarnResponse>>> GetPaginatedBarnList(
    ListingRequest request,
    CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<AdminBarnResponse>>> GetPaginatedAdminBarnListAsync(
    ListingRequest request,
    CancellationToken cancellationToken = default);
        Task<Response<AdminBarnDetailResponse>> GetAdminBarnDetailAsync(
    Guid barnId,
    CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<ReleaseBarnResponse>>> GetPaginatedReleaseBarnListAsync(
            ListingRequest request,
            CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<ReleaseBarnResponse>>> SaleGetReleasedBarnList(ListingRequest request);
        Task<Response<ReleaseBarnDetailResponse>> GetReleaseBarnDetail(
            Guid BarnId,
            CancellationToken cancellationToken = default);
        public Task<Response<PaginationSet<BarnResponse>>> GetAssignedBarn(Guid tsid, ListingRequest req);
    }
}