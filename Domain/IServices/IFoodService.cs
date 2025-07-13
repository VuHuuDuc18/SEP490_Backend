using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Food;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Food;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IFoodService
    {
        Task<Response<string>> CreateFood(CreateFoodRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> UpdateFood(UpdateFoodRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> DisableFood(Guid foodId, CancellationToken cancellationToken = default);

        Task<Response<FoodResponse>> GetFoodById(Guid foodId, CancellationToken cancellationToken = default);

        Task<Response<List<FoodResponse>>> GetFoodByCategory(string foodName = null, Guid? foodCategoryId = null, CancellationToken cancellationToken = default);

        Task<Response<PaginationSet<FoodResponse>>> GetPaginatedFoodList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        Task<List<FoodResponse>> GetAllFood(CancellationToken cancellationToken = default);

        public Task<bool> ExcelDataHandle(List<CellFoodItem> data);
    }
}