using Domain.Dto.Response.Bill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Response.LivestockCircle
{
    public class FoodRemainingResponse
    {
        public Guid Id { get; set; }
        public Guid LivestockCircleId { get; set; }
        public FoodBillResponse Food { get; set; }
        public float Remaining { get; set; }
    }
    public class MedicineRemainingResponse
    {
        public Guid Id { get; set; }
        public Guid LivestockCircleId { get; set; }
        public MedicineBillResponse Medicine { get; set; }
        public float Remaining { get; set; }
    }
}
