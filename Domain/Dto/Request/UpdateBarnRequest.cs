using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request
{
    public class UpdateBarnRequest
    {
        [Required(ErrorMessage = "Tên chuồng trại là bắt buộc.")]
        public string BarnName { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        public string Address { get; set; }

        public string Image { get; set; }

        [Required(ErrorMessage = "ID người gia công là bắt buộc.")]
        public Guid WorkerId { get; set; }
    }
}
