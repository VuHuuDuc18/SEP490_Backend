using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Account
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string RoleName { get; set; }
    }
}
