using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Barn
{
    public class BarnResponse
    {
        public Guid Id { get; set; }
        public string BarnName { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public Guid WorkerId { get; set; }
        public bool IsActive { get; set; }
    }
}
