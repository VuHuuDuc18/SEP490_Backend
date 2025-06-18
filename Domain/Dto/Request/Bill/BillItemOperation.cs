using System.ComponentModel.DataAnnotations;

namespace Domain.Dto.Request.Bill
{
    public class BillItemOperation
    {
        // Loại thao tác: "Add", "Update", "Remove"
        [Required(ErrorMessage = "Loại thao tác là bắt buộc.")]
        [RegularExpression("^(Add|Update|Remove)$", ErrorMessage = "Loại thao tác phải là 'Add', 'Update', hoặc 'Remove'.")]
        public string OperationType { get; set; }

        // Dữ liệu cho Add/Update: sử dụng CreateBillItemRequest
        public CreateBillItemRequest ItemData { get; set; }

        // ID của BillItem cho Update/Remove
        public Guid? ItemId { get; set; }
    }
}