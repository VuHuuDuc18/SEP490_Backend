using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Breed
{
    public class UpdateBreedRequest
    {
        [Required(ErrorMessage = "ID giống loài là bắt buộc.")]
        public Guid BreedId { get; set; }
        [Required(ErrorMessage = "Tên giống loài là bắt buộc.")]
        public string BreedName { get; set; }

        [Required(ErrorMessage = "ID danh mục giống loài là bắt buộc.")]
        public Guid BreedCategoryId { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải là số không âm.")]
        public int Stock { get; set; }

        public List<string> ImageLinks { get; set; } = new List<string>();
        public string Thumbnail { get; set; }
    }
}
