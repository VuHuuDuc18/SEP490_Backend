using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.DailyReport
{
    public class DailyReportResponse
    {
        public Guid Id { get; set; }
        public Guid LivestockCircleId { get; set; }
        public int DeadUnit { get; set; }
        public int GoodUnit { get; set; }
        public int BadUnit { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Danh sách liên kết ảnh (upload lên Cloudinary).
        /// </summary>
        public List<string> ImageLinks { get; set; } = new List<string>();

        /// <summary>
        /// Liên kết ảnh thumbnail (upload lên Cloudinary).
        /// </summary>
        public string Thumbnail { get; set; }
        public List<FoodReportResponse> FoodReports { get; set; }
        public List<MedicineReportResponse> MedicineReports { get; set; }


    }
}
