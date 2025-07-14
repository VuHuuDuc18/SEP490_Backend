using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Food
{
    public class FoodResponse
    {
        public Guid Id { get; set; }
        public string FoodName { get; set; }
        public FoodCategoryResponse FoodCategory { get; set; }
        public int Stock { get; set; }
        public float WeighPerUnit { get; set; }
        public bool IsActive { get; set; }
        public List<string> ImageLinks { get; set; } = new List<string>();
        public string Thumbnail { get; set; }
    }

    public class FoodCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
