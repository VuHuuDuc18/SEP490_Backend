using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class BillItem : EntityBase
    {
        public Guid BillId { get; set; }
        public Guid? FoodId { get; set; }
        public Guid? MedicineId { get; set; }
        public Guid? BreedId { get; set; }
        public int Stock { get; set; }


        public virtual Bill Bill { get; set; }
        public virtual Food Food { get; set; }
        public virtual Medicine Medicine { get; set; }
        public virtual Breed Breed { get; set; }
    }
}
