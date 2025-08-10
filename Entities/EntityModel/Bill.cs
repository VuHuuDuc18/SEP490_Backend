using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Bill : EntityBase
    {
        public Guid UserRequestId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int Total { get; set; }
        public float? Weight { get; set; }
        public string Status { get; set; }
        public string TypeBill { get; set; }
        public DateTime DeliveryDate { get; set; }
        public virtual User UserRequest { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
    }
}
