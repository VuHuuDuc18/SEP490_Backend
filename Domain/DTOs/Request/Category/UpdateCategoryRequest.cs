using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Category
{
    public class UpdateCategoryRequest 
    { 
        [Required(ErrorMessage = "Id danh mục là bắt buộc.")]
        public Guid Id {  get; set; }
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
