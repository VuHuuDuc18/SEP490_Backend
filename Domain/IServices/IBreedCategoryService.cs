using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.Category;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IBreedCategoryService
    {

        Task<Response<string>> CreateBreedCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> UpdateBreedCategory(UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> DisableBreedCategory(Guid breedCategoryId, CancellationToken cancellationToken = default);

        Task<Response<BreedCategoryResponse>> GetBreedCategoryById(Guid breedCategoryId, CancellationToken cancellationToken = default);

        Task<Response<List<BreedCategoryResponse>>> GetBreedCategoryByName(string name = null, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<CategoryResponse>>> GetPaginatedBreedCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        Task<List<BreedCategoryResponse>> GetAllCategory(CancellationToken cancellationToken = default);
    }
}