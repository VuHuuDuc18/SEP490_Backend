using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.Order
{
    public class ViewOrderDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public int GoodUnitStock { get; set; }
        public int BadUnitStock { get; set; }
        public float TotalBill { get; set; }
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
