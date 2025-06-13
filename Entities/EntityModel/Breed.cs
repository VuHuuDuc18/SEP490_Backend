using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Breed : EntityBase
    {
        public string BreedName { get; set; }
        public Guid BreedCategoryId { get; set; }
        public int Stock { get; set; }

        public virtual BreedCategory BreedCategory { get; set; }
    }
}
