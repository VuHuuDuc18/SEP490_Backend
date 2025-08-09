using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill
{
        public class UpdateBillFoodDto
        {

            [Required(ErrorMessage = "ID hóa đơn là bắt buộc.")]
            public Guid BillId { get; set; }
            [Required(ErrorMessage = "Phải cung cấp danh sách mặt hàng thức ăn.")]
            public List<FoodItemRequest> FoodItems { get; set; } = new List<FoodItemRequest>();
            public DateTime DeliveryDate { get; set; }
    }

        public class UpdateBillMedicineDto
        {
            [Required(ErrorMessage = "ID hóa đơn là bắt buộc.")]
             public Guid BillId { get; set; }
             [Required(ErrorMessage = "Phải cung cấp danh sách mặt hàng thuốc.")]
            public List<MedicineItemRequest> MedicineItems { get; set; } = new List<MedicineItemRequest>();
            public DateTime DeliveryDate { get; set; }
    }

        public class UpdateBillBreedDto
        {
            //[Required(ErrorMessage = "ID hóa đơn là bắt buộc.")]
            //public Guid BillId { get; set; }
            [Required(ErrorMessage = "Phải cung cấp danh sách mặt hàng giống.")]
            public List<BreedItemRequest> BreedItems { get; set; } = new List<BreedItemRequest>();
    }
    
}
