using Domain.Dto.Response.Breed;
using Domain.Dto.Response.User;

namespace Domain.DTOs.Request.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LivestockCircleId { get; set; }
        public int GoodUnitStock { get; set; }
        public int BadUnitStock { get; set; }
        public float? TotalBill { get; set; }
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public string? BreedName { get; set; }
        public UserItemResponse? Customer { get; set; }
    }
}
