using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class LivestockCircleMedicine : EntityBase
    {
        public Guid LivestockCircleId { get; set; }
        public Guid MedicineId { get; set; }
        public float Remaining { get; set; }

        public virtual Medicine Medicine { get; set; }
        public virtual LivestockCircle LSCircle { get; set; }
    }
}
