using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Medicine
{
    public class CellMedicineItem
    {
        //public Guid Id { get; set; }
        public string Ten_Thuoc { get; set; }
        public string Ma_dang_ky { get; set; }
        public string Phan_Loai_Thuoc { get; set; }
        public int So_luong { get; set; }
    }
}
