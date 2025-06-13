using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response
{
    public class FoodResponse
    {
        public Guid Id { get; set; }
        public string FoodName { get; set; }
        public Guid FoodCategoryId { get; set; }
        public int Stock { get; set; }
        public float WeighPerUnit { get; set; }
        public bool IsActive { get; set; }
    }
}
