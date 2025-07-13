using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Category;
using Domain.Dto.Response;
using Domain.Dto.Response.Category;
using Domain.Dto.Response.Medicine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IMedicineCategoryService
    {
        Task<Response<string>> CreateMedicineCategory(CreateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> UpdateMedicineCategory(UpdateCategoryRequest request, CancellationToken cancellationToken = default);

        Task<Response<string>> DisableMedicineCategory(Guid medicineCategoryId, CancellationToken cancellationToken = default);

        Task<Response<CategoryResponse>> GetMedicineCategoryById(Guid medicineCategoryId, CancellationToken cancellationToken = default);

        Task<Response<List<CategoryResponse>>> GetMedicineCategoryByName(string name = null, CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<CategoryResponse>>> GetPaginatedMedicineCategoryList(
            ListingRequest request,
            CancellationToken cancellationToken = default);
        Task<List<MedicineCategoryResponse>> GetAllMedicineCategory(CancellationToken cancellationToken = default);
    }
}