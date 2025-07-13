using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Category;
using Domain.Dto.Response.Food;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IFoodCategoryService
    {

        Task<Response<string>> CreateFoodCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default);


        Task<Response<string>> UpdateFoodCategory(UpdateCategoryRequest request, CancellationToken cancellationToken = default);


        Task<Response<string>> DisableFoodCategory(Guid foodCategoryId, CancellationToken cancellationToken = default);

 
        Task<Response<CategoryResponse>> GetFoodCategoryById(Guid foodCategoryId, CancellationToken cancellationToken = default);


        Task<Response<List<CategoryResponse>>> GetFoodCategoryByName(string name = null, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<CategoryResponse>>> GetPaginatedFoodCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default);


        Task<List<FoodCategoryResponse>> GetAllCategory(CancellationToken cancellationToken = default);
    }
}