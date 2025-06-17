using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request
{
    public class DeleteAccountRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
