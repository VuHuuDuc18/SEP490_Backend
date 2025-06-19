using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill
{
    public class CreateBillRequest
    {
        [Required(ErrorMessage = "UserRequestId là bắt buộc.")]
        public Guid UserRequestId { get; set; }

        [Required(ErrorMessage = "LivestockCircleId là bắt buộc.")]
        public Guid LivestockCircleId { get; set; }

        [Required(ErrorMessage = "Tên hóa đơn là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên hóa đơn không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Note { get; set; }

        public float? Weight { get; set; }

        [Required(ErrorMessage = "Danh sách mục hóa đơn là bắt buộc.")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một mục hóa đơn.")]
        public List<CreateBillItemRequest> Items { get; set; }
    }
}
