using Domain.Dto.Request;
using Domain.Dto.Request.Bill;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBillService
    {
        Task<(bool Success, string ErrorMessage)> CreateAsync(
            CreateBillRequest request,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> UpdateAsync(
            Guid id,
            UpdateBillRequest request,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> DeleteBillItemAsync(
            Guid billItemId,
            CancellationToken cancellationToken = default);

        Task<(bool Success, string ErrorMessage)> DisableAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetPaginatedListAsync(
            ListingRequest request,
            CancellationToken cancellationToken = default);

        Task<(PaginationSet<BillItemResponse> Result, string ErrorMessage)> GetBillItemsByBillIdAsync(
            Guid billId,
            ListingRequest request,
            CancellationToken cancellationToken = default);

        Task<(BillResponse Bill, string ErrorMessage)> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thay đổi status của billl.
        /// </summary>
        Task<(bool Success, string ErrorMessage)> ChangeBillStatusAsync(
           Guid billId, string newStatus, CancellationToken cancellationToken = default);

        // Lấy danh sách hóa đơn chỉ chứa các mục hóa đơn thuộc loại được chỉ định (Food, Medicine hoặc Breed)
        Task<(PaginationSet<BillResponse> Result, string ErrorMessage)> GetBillsByItemTypeAsync(
           ListingRequest request, string itemType, CancellationToken cancellationToken = default);
    }
}
