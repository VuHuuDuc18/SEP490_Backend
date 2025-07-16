using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.Response.Role
{
    public class RoleResponse
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
    }
    public class AdminRoleResponse
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}
