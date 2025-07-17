using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.LivestockCircle
{
    public class CreateLiveStockCircleFoodRequest
    {
        public Guid LivestockCircleId { get; set; }
        public Guid? FoodId { get; set; }
        public float Remaining { get; set; }
    }
}
