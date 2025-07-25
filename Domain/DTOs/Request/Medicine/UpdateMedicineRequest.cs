﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Medicine
{
    public class UpdateMedicineRequest
    {
        [Required(ErrorMessage = "MedicineId không được để trống")]
        public Guid MedicineId { get; set; }
        [Required(ErrorMessage = "Tên thuốc là bắt buộc.")]
        public string MedicineName { get; set; }

        [Required(ErrorMessage = "Mã thuốc là bắt buộc.")]
        public string MedicineCode { get; set; }

        [Required(ErrorMessage = "ID danh mục thuốc là bắt buộc.")]
        public Guid MedicineCategoryId { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải là số không âm.")]
        public int Stock { get; set; }
        public List<string> ImageLinks { get; set; } = new List<string>();
        public string Thumbnail { get; set; }
    }
}
