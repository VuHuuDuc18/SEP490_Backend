﻿using Domain.Dto.Request;
using Domain.Dto.Request.Medicine;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ExcelControl.Handler;
using OfficeOpenXml.DataValidation;
using System.IO;
using System;
using Domain.Dto.Response.Medicine;
using Domain.IServices;

namespace SEP490_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineService _medicineService;
        private readonly IMedicineCategoryService _medicineCategoryService;


        public MedicineController(IMedicineService medicineService, IMedicineCategoryService medicineCategoryService)
        {
            _medicineService = medicineService;
            _medicineCategoryService = medicineCategoryService;
        }


        [HttpPost("medicine-room-staff/get-medicine-list")]
        public async Task<IActionResult> GetPaginatedMedicines([FromBody] ListingRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.GetPaginatedMedicineList(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("medicine-room-staff/create-medicine")]
        public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.CreateMedicine(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("medicine-room-staff/update-medicine")]
        public async Task<IActionResult> UpdateMedicine([FromBody] UpdateMedicineRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.UpdateMedicine(request, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("medicne-room-staff/disable-medicine/{MedicineId}")]
        public async Task<IActionResult> DisableMedicine([FromRoute] Guid MedicineId, CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.DisableMedicine(MedicineId, cancellationToken);
            if (!response.Succeeded)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("get-medicine-by-id/{MedicineId}")]
        public async Task<IActionResult> GetMedicineById([FromRoute] Guid MedicineId, CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.GetMedicineById(MedicineId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }
        [HttpGet("get-all-medicine")]
        public async Task<IActionResult> GetAllMedicine(CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.GetAllMedicine(cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }
        [HttpGet("get-medicine-by-category")]
        public async Task<IActionResult> GetMedicineByCategory(
            [FromQuery] string medicineName = null,
            [FromQuery] Guid? medicineCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _medicineService.GetMedicineByCategory(medicineName, medicineCategoryId, cancellationToken);
            if (!response.Succeeded)
                return NotFound(response);
            return Ok(response);
        }

        //[HttpPost("export")]
        //public async Task<ActionResult> Export(ListingRequest request)
        //{
        //    // ExcelPackage package = new ExcelPackage();
        //    //B1: set linence

        //    ExcelPackage.License.SetNonCommercialPersonal("Vu Duc");
        //    // nhan data
        //    var model = await _medicineService.GetPaginatedMedicineList(request);

        //    using (var excelPackage = new ExcelPackage())
        //    {
        //        //B2 fill data
        //        // Thêm sheet với dữ liệu
        //        ExportExcelHelper.AddSheetToExcelFile(excelPackage, model.Result.Items, "People", "Danh sách nhân viên");

        //        // Lưu file vào thư mục tạm và lấy tên file
        //        string fileName = ExportExcelHelper.SaveExcelFile(excelPackage, "API User", "Báo cáo nhân viên");

        //        //B3 xuat data
        //        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        //            return BadRequest("Invalid file name.");

        //        var path = Path.Combine(Path.GetTempPath(), fileName);
        //        if (!System.IO.File.Exists(path))
        //            return NotFound("File not found.");

        //        var bytes = System.IO.File.ReadAllBytes(path);
        //        return File(bytes,
        //                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                    fileName);
        //    }
        //}
        [HttpPost("import")]
        public async Task<IActionResult> ImportPersonExcel(IFormFile file)
        {
            try
            {


                ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                List<CellMedicineItem> items;
                using (var stream = new MemoryStream())
                {

                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    items = ExportExcelHelper.ImportExcelFile<CellMedicineItem>(stream);
                }
                
                return Ok(await _medicineService.ExcelDataHandle(items));
            }catch (Exception ex)
            {
                throw new Exception("Lỗi dữ liệu file");
            }
        }
        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal(Domain.Helper.Constants.LienceConstant.NonCommercialPersonal);
            var MedicineCategoryList = new List<MedicineCategoryResponse>();

            MedicineCategoryList = await _medicineCategoryService.GetAllMedicineCategory();

            var fileBytes = ExportExcelHelper.GenerateExcelTemplateAndData<CellMedicineItem, MedicineCategoryResponse>("Bảng thuốc", "Phân loại mẫu thuốc", MedicineCategoryList);
            string fileName = $"Medicine-Import-Data-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}