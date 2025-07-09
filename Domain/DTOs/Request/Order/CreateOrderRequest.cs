using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Request.Order
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "LivestockCircleId không được để trống")]
        public Guid LivestockCircleId { get; set; }
        [Required(ErrorMessage = "GoodUnitStock không được để trống")]
        public int GoodUnitStock { get; set; }
        [Required(ErrorMessage = "BadUnitStock không được để trống")]
        public int BadUnitStock { get; set; }
        [Required(ErrorMessage = "PickupDate không được để trống")]
        public DateTime PickupDate { get; set; }
    }
}
