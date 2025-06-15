using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class ImageMedicine : EntityBase
    {
        public Guid MedicineId { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
        public virtual Medicine Medicine { get; set; }
    }
}
