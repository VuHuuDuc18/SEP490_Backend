using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill
{
    public class UpdateBillRequest
    {
        [Required(ErrorMessage = "Tên hóa đơn là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên hóa đơn không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Note { get; set; }

        public float? Weight { get; set; }

        // Danh sách các thao tác với BillItem: thêm mới, cập nhật, hoặc xóa
        public List<BillItemOperation> ItemOperations { get; set; }
    }
}
