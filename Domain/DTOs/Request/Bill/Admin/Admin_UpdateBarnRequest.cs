using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Bill.Admin
{
    public class Admin_UpdateBarnRequest
    {
        [Required]
        public Guid LivestockCircleId { get; set; }
        //[Required]
       // public Guid BarnId { get; set; }
        [Required]
        public Guid BreedId { get; set; }
        [Required]
        [Range(1, 100001, ErrorMessage = "Số lượng phải trong 0 - 100001.")]
        public int Stock { get; set; }
        public string? Note { get; set; }


    }
}
