using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.LivestockCircle
{
    public class ChangeStatusRequest
    {
        [Required]
        public Guid LivestockCircleId { get; set; }
        [Required]
        public string Status {  get; set; }
    }
}
