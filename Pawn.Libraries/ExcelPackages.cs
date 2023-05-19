using System.Collections.Generic;
using OfficeOpenXml;
using System.Data;
using System.IO;
using OfficeOpenXml.Style;

namespace Pawn.Libraries
{
    public class ExcelPackages
    {
        private ExcelPackage _excelPackage = null;
        private ExcelRange _excelRange = null;
        private readonly string _sheetName;

        public ExcelPackages(string sheetName)
        {
            _sheetName = sheetName;
            _excelPackage = new ExcelPackage();
        }

        public ExcelPackages() { _excelPackage = new ExcelPackage(); }

        private MemoryStream LoadDataToStream(byte[] data) => new MemoryStream(data);


        /// <summary>
        /// Export Excel using DataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public MemoryStream ExportToExcel(DataTable dt, bool printHeader = true)
        {
            var excelWorksheets = _excelPackage.Workbook.Worksheets.Add(_sheetName);
            excelWorksheets.Cells["A1"].LoadFromDataTable(dt, printHeader);
            var colFromHex = System.Drawing.ColorTranslator.FromHtml("#4D77CC");
            using (var range = excelWorksheets.Cells[2, 1, 2, dt.Columns.Count])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(colFromHex);
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            excelWorksheets.Cells[2, 1, dt.Rows.Count - 1, dt.Columns.Count].AutoFitColumns();
            excelWorksheets.DeleteRow(1);
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }

        public MemoryStream ExportToExcelCashBook(DataTable dt, bool printHeader = true)
        {
            var excelWorksheets = _excelPackage.Workbook.Worksheets.Add(_sheetName);
            excelWorksheets.Cells["A1"].LoadFromDataTable(dt, printHeader);
            var colFromHex = System.Drawing.ColorTranslator.FromHtml("#4D77CC");
            using (var range = excelWorksheets.Cells[2, 1, 2, dt.Columns.Count])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(colFromHex);
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            using (var range = excelWorksheets.Cells[dt.Rows.Count, 6, dt.Rows.Count + 1, 6])
            {
                range.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                range.Style.Font.Bold = true;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (var range = excelWorksheets.Cells[dt.Rows.Count, 7, dt.Rows.Count, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (var range = excelWorksheets.Cells[dt.Rows.Count + 1, 7, dt.Rows.Count + 1, 8])
            {
                range.Merge = true;
                range.Style.Font.Bold = true;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            excelWorksheets.Cells[2, 1, dt.Rows.Count - 1, dt.Columns.Count].AutoFitColumns();
            excelWorksheets.DeleteRow(1);
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }

        /// <summary>
        /// Export Excel using IDataReader
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public MemoryStream ExportToExcel(IDataReader dataReader)
        {
            _excelPackage.Workbook.Worksheets.Add(_sheetName).Cells["A1"].LoadFromDataReader(dataReader, true);
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }

        /// <summary>
        /// Export Excel using IEnumerable<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public MemoryStream ExportToExcel<T>(IEnumerable<T> dataSource)
        {
            _excelPackage.Workbook.Worksheets.Add(_sheetName).Cells["A1"].LoadFromCollection<T>(dataSource);
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }

        /// <summary>
        /// Export Excel using List<Object>
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public MemoryStream ExportToExcel(IEnumerable<object[]> dataSource)
        {
            _excelPackage.Workbook.Worksheets.Add(_sheetName).Cells["A1"].LoadFromArrays(dataSource);
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }

        public ExcelPackages ExportExcelNoData()
        {
            var excelWorksheets = _excelPackage.Workbook.Worksheets.Add("Sheet 1");
            excelWorksheets.Cells[1, 1].Value = "No data to display";
            return this;
        }

        public MemoryStream ExportExcel()
        {
            var stream = LoadDataToStream(_excelPackage.GetAsByteArray());
            return stream;
        }
    }
}
