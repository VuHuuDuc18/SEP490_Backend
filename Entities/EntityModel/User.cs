using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Entities.EntityModel
{
    public class User : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;
        [Required] public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
