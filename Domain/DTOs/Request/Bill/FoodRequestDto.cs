using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Dto.Request.Bill
{
    public class CreateFoodRequestDto
    {
        [Required(ErrorMessage = "LivestockCircleId là bắt buộc.")]
        public Guid LivestockCircleId { get; set; }
        public string Note { get; set; }
        [Required(ErrorMessage = "Phải cung cấp ít nhất một mặt hàng thức ăn.")]
        public List<FoodItemRequest> FoodItems { get; set; } = new List<FoodItemRequest>();
    }

    public class AddFoodItemToBillDto
    {
        [Required(ErrorMessage = "Phải cung cấp ít nhất một mặt hàng thức ăn.")]
        public List<FoodItemRequest> FoodItems { get; set; } = new List<FoodItemRequest>();
    }

    public class UpdateFoodItemInBillDto
    {
        [Required(ErrorMessage = "Phải cung cấp chính xác một mặt hàng thức ăn.")]
        public List<FoodItemRequest> FoodItems { get; set; } = new List<FoodItemRequest>();
    }

    public class FoodItemRequest
    {
        [Required(ErrorMessage = "ItemId là bắt buộc.")]
        public Guid ItemId { get; set; }
        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}