using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class BarnPlanFood : EntityBase
    {
        public Guid BarnPlanId { get; set; }
        public Guid FoodId { get; set; }
        public float Stock {  get; set; }
        public string Note { get; set; }

        public virtual Food Food { get; set; }
        public virtual BarnPlan BarnPlan { get; set; }
    }
}
