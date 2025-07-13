using Entities.EntityModel;
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
        public WokerResponse? Worker { get; set; }
        public bool IsActive { get; set; }
    }

    public class WokerResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
