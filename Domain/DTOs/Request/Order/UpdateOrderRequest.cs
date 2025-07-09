using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.Order
{
    public class UpdateOrderRequest
    {
        public Guid LivestockCircleId { get; set; }
        public int GoodUnitStock { get; set; }
        public int BadUnitStock { get; set; }
        public float TotalBill { get; set; }
    }
}
