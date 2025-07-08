using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Dto.Request.Bill
{
    public class CreateMedicineRequestDto
    {
        [Required(ErrorMessage = "UserRequestId là bắt buộc.")]
        public Guid UserRequestId { get; set; }
        [Required(ErrorMessage = "LivestockCircleId là bắt buộc.")]
        public Guid LivestockCircleId { get; set; }
        public string Note { get; set; }
        [Required(ErrorMessage = "Phải cung cấp ít nhất một mặt hàng thuốc.")]
        public List<MedicineItemRequest> MedicineItems { get; set; } = new List<MedicineItemRequest>();
    }

    public class AddMedicineItemToBillDto
    {
        [Required(ErrorMessage = "Phải cung cấp ít nhất một mặt hàng thuốc.")]
        public List<MedicineItemRequest> MedicineItems { get; set; } = new List<MedicineItemRequest>();
    }

    public class UpdateMedicineItemInBillDto
    {
        [Required(ErrorMessage = "Phải cung cấp chính xác một mặt hàng thuốc.")]
        public List<MedicineItemRequest> MedicineItems { get; set; } = new List<MedicineItemRequest>();
    }

    public class MedicineItemRequest
    {
        [Required(ErrorMessage = "ItemId là bắt buộc.")]
        public Guid ItemId { get; set; }
        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}