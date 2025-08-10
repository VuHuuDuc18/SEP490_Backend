using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.LivestockCircle
{
    public  class SetPreOrderFieldRequest
    {
        public Guid LivestockCircleId { get; set; }
        public float? SamplePrice { get; set; }
        public DateTime? PreOrderDate { get; set; }
    }
}
