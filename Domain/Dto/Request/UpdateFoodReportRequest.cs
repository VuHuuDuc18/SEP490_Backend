using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request
{
    public class UpdateFoodReportRequest
    {
        [Required(ErrorMessage = "ID thức ăn là bắt buộc.")]
        public Guid FoodId { get; set; }

        [Required(ErrorMessage = "ID báo cáo là bắt buộc.")]
        public Guid ReportId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
