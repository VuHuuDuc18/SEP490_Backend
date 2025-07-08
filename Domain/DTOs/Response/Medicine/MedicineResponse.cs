using Entities.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Response.Medicine
{
    public class MedicineResponse
    {
        public Guid Id { get; set; }
        public string MedicineName { get; set; }
        public MedicineCategoryResponse MedicineCategory { get; set; }
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

    public class MedicineCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
