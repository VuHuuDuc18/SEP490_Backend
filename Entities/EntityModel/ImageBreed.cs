using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class ImageBreed : EntityBase
    {
        public Guid BreedId { get; set; }
        public string Thumnail { get; set; }
        public string ImageLink { get; set; }
        public virtual Breed Breed { get; set; }
    }
}
