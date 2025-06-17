using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.DailyReport
{
    public class UpdateMedicineReportRequest
    {
        [Required(ErrorMessage = "ID báo cáo thuốc là bắt buộc.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "ID thuốc là bắt buộc.")]
        public Guid MedicineId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
