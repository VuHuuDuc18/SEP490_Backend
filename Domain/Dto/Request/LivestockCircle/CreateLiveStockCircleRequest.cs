using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.Request.LivestockCircle
{
    /// <summary>
    /// DTO dùng để nhận dữ liệu khi tạo chu kỳ chăn nuôi.
    /// </summary>
    public class CreateLivestockCircleRequest
    {
        [Required(ErrorMessage = "Tên chu kỳ chăn nuôi là bắt buộc.")]
        public string LivestockCircleName { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateTime StartDate { get; set; }

        //[Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        //public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Tổng số đơn vị giống là bắt buộc.")]
        public int TotalUnit { get; set; }

        [Required(ErrorMessage = "ID giống là bắt buộc.")]
        public Guid BreedId { get; set; }

        [Required(ErrorMessage = "ID chuồng trại là bắt buộc.")]
        public Guid BarnId { get; set; }

        [Required(ErrorMessage = "ID nhân viên kỹ thuật là bắt buộc.")]
        public Guid TechicalStaffId { get; set; }
    }
}
