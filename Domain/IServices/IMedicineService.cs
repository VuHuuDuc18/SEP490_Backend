using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Request.Medicine;
using Domain.Dto.Response;
using Domain.Dto.Response.Medicine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.IServices
{
    public interface IMedicineService
    {
  
        Task<Response<string>> CreateMedicine(CreateMedicineRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> UpdateMedicine(UpdateMedicineRequest request, CancellationToken cancellationToken = default);
        Task<Response<string>> DisableMedicine(Guid medicineId, CancellationToken cancellationToken = default);
        Task<Response<MedicineResponse>> GetMedicineById(Guid medicineId, CancellationToken cancellationToken = default);
        Task<Response<List<MedicineResponse>>> GetAllMedicine(CancellationToken cancellationToken = default);
        Task<Response<List<MedicineResponse>>> GetMedicineByCategory(
            string medicineName = null,
            Guid? medicineCategoryId = null,
            CancellationToken cancellationToken = default);
        Task<Response<PaginationSet<MedicineResponse>>> GetPaginatedMedicineList(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        public Task<bool> ExcelDataHandle(List<CellMedicineItem> data);


    }
}