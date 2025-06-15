using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Order : EntityBase
    {
        public Guid CustomerId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public int GoodUnitStock { get; set; }
        public int BadUnitStock { get; set; }
        public float TotalBill { get; set; }
        public string Status {  get; set; }
        public string Note { get; set; }
        public virtual User Customer { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
    }
}
