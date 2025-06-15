using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class DailyReport : EntityBase
    {
        public Guid LivestockCircleId { get; set; }
        public int DeadUnit { get; set; }
        public int GoodUnit { get; set; }
        public int BadUnit { get; set; }
        public string Note { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
    }
}
