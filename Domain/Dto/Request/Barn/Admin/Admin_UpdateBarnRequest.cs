using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Barn.Admin
{
    public class Admin_UpdateBarnRequest
    {
        [Required]
        public Guid BarnId { get; set; }
        [Required]
        public Guid BreedId { get; set; }
        [Required]
        [Range(1,10000)]
        public int Stock {  get; set; }

    }
}
