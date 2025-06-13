using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class FoodCategory : EntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
