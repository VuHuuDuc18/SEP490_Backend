using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request
{
    public class UpdateFoodRequest
    {
        [Required(ErrorMessage = "Tên thức ăn là bắt buộc.")]
        public string FoodName { get; set; }

        [Required(ErrorMessage = "ID danh mục thức ăn là bắt buộc.")]
        public Guid FoodCategoryId { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải là số không âm.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Cân nặng mỗi đơn vị là bắt buộc.")]
        [Range(0.01f, float.MaxValue, ErrorMessage = "Cân nặng mỗi đơn vị phải lớn hơn 0.")]
        public float WeighPerUnit { get; set; }

        /// <summary>
        /// Danh sách liên kết ảnh (upload lên Cloudinary) .
        /// </summary>
        public List<string> ImageLinks { get; set; } = new List<string>();

        /// <summary>
        /// Liên kết ảnh thumbnail (upload lên Cloudinary).
        /// </summary>
        public string Thumbnail { get; set; }
    }
}
