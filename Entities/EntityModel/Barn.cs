using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.EntityModel
{
    public class Barn : EntityBase
    {
        public string BarnName { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public Guid WorkerId { get; set; }
        public string? Description { get; set; } // mô tả chuồng trại vd quy mô, số lứa/năm ...
        public virtual User Worker  { get; set; }
    }
}
