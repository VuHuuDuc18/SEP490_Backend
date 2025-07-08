using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.Food
{
    public class CellFoodItem
    {
        public string Ten { get; set; }
        //public string Ma_dang_ky { get; set; }
         
        public string Phan_Loai  { get; set; }
        public float Trong_luong_Theo_Kg {  get; set; }
        public int So_luong { get; set; }
    }
}
