using Domain.Dto.Response.Bill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.DailyReport
{
    public class MedicineReportResponse
    {
        public Guid Id { get; set; }
        public MedicineBillResponse Medicine { get; set; }
        public Guid ReportId { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}
