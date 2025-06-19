using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Bill
{
    public class BillItemResponse
    {
        public Guid Id { get; set; }
        public Guid BillId { get; set; }
        public Guid? FoodId { get; set; }
        public Guid? MedicineId { get; set; }
        public Guid? BreedId { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
