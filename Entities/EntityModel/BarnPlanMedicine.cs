using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class BarnPlanMedicine : EntityBase
    {
        public Guid BarnPlanId { get; set; }
        public Guid MedicineId { get; set; }
        public float Stock { get; set; }
        public string Note { get; set; }

        public virtual Medicine Medicine { get; set; }
        public virtual BarnPlan BarnPlan { get; set; }
    }
}
