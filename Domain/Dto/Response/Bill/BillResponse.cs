using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Bill
{
    public class BillResponse
    {
        public Guid Id { get; set; }
        public Guid UserRequestId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int Total { get; set; }
        public float? Weight { get; set; }
        public bool IsActive { get; set; }
    }
}
