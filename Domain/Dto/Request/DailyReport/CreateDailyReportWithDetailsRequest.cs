using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.DailyReport
{
    public class CreateDailyReportWithDetailsRequest
    {
        [Required(ErrorMessage = "ID vòng chăn nuôi là bắt buộc.")]
        public Guid LivestockCircleId { get; set; }

        [Required(ErrorMessage = "Số lượng chết là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng chết phải là số không âm.")]
        public int DeadUnit { get; set; }

        [Required(ErrorMessage = "Số lượng tốt là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tốt phải là số không âm.")]
        public int GoodUnit { get; set; }

        [Required(ErrorMessage = "Số lượng xấu là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng xấu phải là số không âm.")]
        public int BadUnit { get; set; }

        public string Note { get; set; }


        /// <summary>
        /// Danh sách liên kết ảnh (upload lên Cloudinary).
        /// </summary>
        public List<string> ImageLinks { get; set; } = new List<string>();

        /// <summary>
        /// Liên kết ảnh thumbnail (upload lên Cloudinary).
        /// </summary>
        public string Thumbnail { get; set; }
        public List<CreateFoodReportRequest> FoodReports { get; set; }
        public List<CreateMedicineReportRequest> MedicineReports { get; set; }

    }
}
