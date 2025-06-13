using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Food : EntityBase
    {
        public string FoodName { get; set; }
        public Guid FoodCategoryId { get; set; }
        public int Stock { get; set; }
        public float WeighPerUnit { get; set; }

        public virtual FoodCategory FoodCategory { get; set; }
    }
}
