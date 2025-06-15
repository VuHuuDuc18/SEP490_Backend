using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class ImageFood : EntityBase
    {
        public Guid FoodId { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
        public virtual Food Food { get; set; }
    }
}
