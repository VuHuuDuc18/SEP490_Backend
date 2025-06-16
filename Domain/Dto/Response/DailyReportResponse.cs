using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response
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
        public List<FoodReportResponse> FoodReports { get; set; }
        public List<MedicineReportResponse> MedicineReports { get; set; }
    }
}
