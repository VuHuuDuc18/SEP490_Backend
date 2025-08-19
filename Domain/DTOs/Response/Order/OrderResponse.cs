﻿using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Domain.Dto.Response.Breed;
using Domain.Dto.Response.Barn;

namespace Domain.DTOs.Request.Order
{
    public class OrderResponse
    {
        public Guid Id  { get; set; }
        public int GoodUnitStock { get; set; }
        public int BadUnitStock { get; set; }
        public float GoodUnitPrice { get; set; }
        public float BadUnitPrice { get; set; }
        public float AverageWeight { get; set; }
        public float? TotalBill { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public Guid LivestockCircleId { get; set; }
        public Guid CustomerId { get; set; }
        public string? BreedName { get; set; }
        public string? BreedCategory { get; set; }
        public ReleasedLivetockDetail? LivestockCircle { get; set; }
        public UserItemResponse? Customer { get; set; }
        public BarnResponse? Barn { get; set; }
        public UserItemResponse? Saler { get; set; }
    }
}
