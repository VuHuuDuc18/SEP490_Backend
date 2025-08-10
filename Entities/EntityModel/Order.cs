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
        public Guid SaleStaffId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public int GoodUnitStock { get; set; }
        public float? GoodUnitPrice { get; set; }
        public int BadUnitStock { get; set; }
        public float? BadUnitPrice { get; set; }
        public DateTime? PickupDate { get; set; }
        public string Status {  get; set; }
        public string? Note { get; set; }

        public virtual User Customer { get; set; }
        public virtual User SaleStaff { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
    }
}
