using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs.Request.Order
{
    public class UpdateOrderRequest
    {
        [Required(ErrorMessage = "OrderId không được để trống")]
        public Guid OrderId { get; set; }
        public int? GoodUnitStock { get; set; }
        public int? BadUnitStock { get; set; }
        public DateTime? PickupDate { get; set; }
    }
}
