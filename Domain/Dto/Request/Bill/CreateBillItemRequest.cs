using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill
{
    public class CreateBillItemRequest
    {
        // Chỉ một trong FoodId, MedicineId, hoặc BreedId được phép có giá trị
        public Guid? FoodId { get; set; }
        public Guid? MedicineId { get; set; }
        public Guid? BreedId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Stock { get; set; }

        // Custom validation để đảm bảo chỉ một ID được set
        public bool IsValidItem()
        {
            var ids = new[] { FoodId, MedicineId, BreedId }.Count(id => id.HasValue);
            return ids == 1;
        }
    }
}
