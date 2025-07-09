using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.DTOs.Request.Order
{
    public class UpdateOrderRequest
    {
        public int? GoodUnitStock { get; set; }
        public int? BadUnitStock { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
