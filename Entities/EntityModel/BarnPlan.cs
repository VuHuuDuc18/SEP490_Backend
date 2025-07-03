using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class BarnPlan : EntityBase
    {
        public Guid LivestockCircleId {  get; set; }    
        public string Note { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
        
    }
}
