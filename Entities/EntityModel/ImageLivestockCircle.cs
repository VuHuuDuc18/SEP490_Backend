using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class ImageLivestockCircle : EntityBase
    {
        public Guid LivestockCircleId { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
        public virtual LivestockCircle LivestockCircle { get; set; }
    }
}
