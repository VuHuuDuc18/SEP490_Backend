using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class FoodReport : EntityBase
    {
        public Guid FoodId { get; set; }
        public Guid ReportId { get; set; }
        public int Quantity { get; set; }
        public virtual Food Food { get; set; }
        public virtual DailyReport Report { get; set; }
    }
}
