using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExcelControl.Handler
{
    public static class ExportExcelHelper
    {


        public static void AddSheetToExcelFile<T>(ExcelPackage excelPackage, List<T> dataList, string sheetName, string sectionTitle)
        {
            //ExcelPackage.License.SetNonCommercialPersonal("Company Admin");
            var workSheet = excelPackage.Workbook.Worksheets.Add(sheetName);

            // Tạo tiêu đề cho section
            workSheet.Cells[1, 1].Value = sectionTitle;
            workSheet.Cells[2, 1].LoadFromCollection(dataList, true, TableStyles.Medium9);
            // Đặt chiều rộng cột mặc định

            workSheet.DefaultColWidth = 20;
        }

        public static string SaveExcelFile(ExcelPackage excelPackage, string currentUser, string Title)
        {
           // ExcelPackage.License.SetNonCommercialPersonal("Company Admin");
            excelPackage.Workbook.Properties.Author = currentUser;
            excelPackage.Workbook.Properties.Title = Title;

            var stream = new MemoryStream();
            excelPackage.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Report-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            var path = Path.Combine(Path.GetTempPath(), excelName);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            return excelName;
        }
        //public static byte[] CreateExcelTemplate<T>(string sheetName = "Template") where T : class, new()
        //{
        //    // Thiết lập license EPPlus 8 (nếu chưa đặt ở nơi khác)
        //   // ExcelPackage.License.SetNonCommercialPersonal("Vu Duc");

        //    using var package = new ExcelPackage();
        //    var worksheet = package.Workbook.Worksheets.Add(sheetName);

        //    // Lấy danh sách thuộc tính public của class T
        //    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //    worksheet.Cells[1, 1].Value = "Nhập thông tin theo mẫu:";

        //    // Ghi header (tên thuộc tính) vào dòng đầu tiên
        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        worksheet.Cells[2, i + 1].Value = properties[i].Name;
        //    }

        //    // Định dạng bảng để dễ nhìn
        //    var tableRange = worksheet.Cells[2, 1, 2, properties.Length];
        //    var table = worksheet.Tables.Add(tableRange, "DataTable");
        //    table.ShowHeader = true;
        //    table.TableStyle = TableStyles.Medium9;

        //    worksheet.DefaultColWidth = 20;

        //    return package.GetAsByteArray();
        //}
        public static byte[] GenerateExcelTemplateAndData<TTemplate, TData>(
        string templateSheetName,
        string dataSheetName,
        List<TData>? dataList)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Vu Duc");
            using (var package = new ExcelPackage())
            {
                // Sheet 1: chỉ header
                var wsTemplate = package.Workbook.Worksheets.Add(templateSheetName);
                // header
                wsTemplate.Cells[1, 1].Value = "Nhập thông tin theo mẫu:";
                
                // ten cot
                var templateProps = typeof(TTemplate).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < templateProps.Length; i++)
                    wsTemplate.Cells[2, i + 1].Value = templateProps[i].Name;
                // dinh dang
                wsTemplate.DefaultColWidth = 30;
                if (dataList != null)
                {
                    wsTemplate.Cells[1, 5].Value = "Tham khảo phân loại tại sheet số 2";
                    // Sheet 2: export data
                    var wsData = package.Workbook.Worksheets.Add(dataSheetName);
                    wsData.Cells[1, 1].Value = "Thông tin phân loại";                    
                    wsData.Cells[2, 1].LoadFromCollection(dataList, true, TableStyles.Medium9);

                    wsData.DefaultColWidth = 30;
                }
                return package.GetAsByteArray();
            }
        }
        public static List<T> ImportExcelFile<T>(Stream excelStream, string sheetName = null) where T : new()
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var result = new List<T>();
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = string.IsNullOrEmpty(sheetName)
                    ? package.Workbook.Worksheets.First()
                    : package.Workbook.Worksheets[sheetName];

                var properties = typeof(T).GetProperties();
                var colMapping = new Dictionary<int, PropertyInfo>();

                // Dòng 2 là header
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var colName = worksheet.Cells[2, col].Text;
                    var prop = properties.FirstOrDefault(p => p.Name == colName);
                    if (prop != null)
                        colMapping.Add(col, prop);
                }

                // Dữ liệu bắt đầu từ dòng 3
                for (int row = 3; row <= worksheet.Dimension.End.Row; row++)
                {
                    var obj = new T();
                    foreach (var map in colMapping)
                    {
                        var val = worksheet.Cells[row, map.Key].Text;
                        if (!string.IsNullOrEmpty(val))
                        {
                            var propType = Nullable.GetUnderlyingType(map.Value.PropertyType) ?? map.Value.PropertyType;
                            object safeValue = null;

                            if (propType == typeof(Guid))
                            {
                                safeValue = Guid.Parse(val); // hoặc Guid.TryParse(val, out var guid) ? guid : Guid.Empty;
                            }
                            else if (propType.IsEnum)
                            {
                                safeValue = Enum.Parse(propType, val);
                            }
                            else
                            {
                                safeValue = Convert.ChangeType(val, propType);
                            }
                            map.Value.SetValue(obj, safeValue);
                        }
                    }

                    result.Add(obj);
                }
            }
            return result;
        }
    }
}

