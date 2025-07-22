using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Breed;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.Medicine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IBreedService
    {
  
        Task<Response<string>> CreateBreed(CreateBreedRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> UpdateBreed(UpdateBreedRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> DisableBreed(Guid breedId, CancellationToken cancellationToken = default);
        Task<Response<BreedResponse>> GetBreedById(Guid breedId, CancellationToken cancellationToken = default);
        Task<Response<List<BreedResponse>>> GetBreedByCategory(string breedName = null, Guid? breedCategoryId = null, CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<BreedResponse>>> GetPaginatedBreedList(
            ListingRequest request,
            CancellationToken cancellationToken = default);
        Task<List<BreedResponse>> GetAllBreed(CancellationToken cancellationToken = default);
        public Task<Response<bool>> ExcelDataHandle(List<CellBreedItem> data);
    }
}