using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class MedicineReport : EntityBase
    {
        public Guid MedicineId { get; set; }
        public Guid ReportId { get; set; }
        public int Quantity { get; set; }
        public virtual Medicine Medicine { get; set; }
        public virtual DailyReport Report { get; set; }
    }
}

