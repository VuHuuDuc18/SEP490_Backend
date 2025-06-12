using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class LivestockCircleFood : EntityBase
    {
        public Guid LivestockCircleId { get; set; }
        public Guid FoodId { get; set; }
        public float Remaining { get; set; }

        public virtual Food Food { get; set; }
        public virtual LivestockCircle LSCircle { get; set; }
    }
}
