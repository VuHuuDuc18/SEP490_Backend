using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Breed
{
    public class BreedResponse
    {
        public Guid Id { get; set; }
        public string BreedName { get; set; }
        public Guid BreedCategoryId { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Danh sách liên kết ảnh từ Cloudinary.
        /// </summary>
        public List<string> ImageLinks { get; set; } = new List<string>();

        /// <summary>
        /// Liên kết ảnh thumbnail từ Cloudinary.
        /// </summary>
        public string Thumbnail { get; set; }
    }
}
