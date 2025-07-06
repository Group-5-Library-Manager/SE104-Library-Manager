using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.Services
{
    public class ExcelExportService
    {
        public ExcelExportService()
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task ExportBorrowingStatisticAsync(List<BorrowingStatisticItem> data, string filePath)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Tình hình mượn sách");

                // Set headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Thể loại sách";
                worksheet.Cells[1, 3].Value = "Số lượt mượn";
                worksheet.Cells[1, 4].Value = "Tỷ lệ (%)";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    worksheet.Cells[i + 2, 1].Value = item.Index;
                    worksheet.Cells[i + 2, 2].Value = item.GenreName;
                    worksheet.Cells[i + 2, 3].Value = item.BorrowCount;
                    worksheet.Cells[i + 2, 4].Value = item.Percentage;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add summary
                int totalRow = data.Count + 3;
                worksheet.Cells[totalRow, 1].Value = "Tổng cộng";
                worksheet.Cells[totalRow, 3].Value = data.Sum(x => x.BorrowCount);
                worksheet.Cells[totalRow, 4].Value = 100;

                // Style summary row
                using (var range = worksheet.Cells[totalRow, 1, totalRow, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xuất file Excel: {ex.Message}");
            }
        }

        public async Task ExportLateReturnStatisticAsync(List<LateReturnStatisticItem> data, string filePath)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Sách trả trễ");

                // Set headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Tên sách";
                worksheet.Cells[1, 3].Value = "Ngày mượn";
                worksheet.Cells[1, 4].Value = "Ngày trả";
                worksheet.Cells[1, 5].Value = "Số ngày trễ";
                worksheet.Cells[1, 6].Value = "Tiền phạt (VND)";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    worksheet.Cells[i + 2, 1].Value = item.Index;
                    worksheet.Cells[i + 2, 2].Value = item.BookName;
                    worksheet.Cells[i + 2, 3].Value = item.FormattedBorrowDate;
                    worksheet.Cells[i + 2, 4].Value = item.FormattedReturnDate;
                    worksheet.Cells[i + 2, 5].Value = item.DaysOverdue;
                    worksheet.Cells[i + 2, 6].Value = item.Fine;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add summary
                int totalRow = data.Count + 3;
                worksheet.Cells[totalRow, 1].Value = "Tổng cộng";
                worksheet.Cells[totalRow, 5].Value = data.Sum(x => x.DaysOverdue);
                worksheet.Cells[totalRow, 6].Value = data.Sum(x => x.Fine);

                // Style summary row
                using (var range = worksheet.Cells[totalRow, 1, totalRow, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xuất file Excel: {ex.Message}");
            }
        }

        public async Task ExportPenaltyStatisticAsync(List<PenaltyStatisticItem> data, string filePath)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Tiền thu phạt");

                // Set headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Ngày";
                worksheet.Cells[1, 3].Value = "Tiền thu phạt (VND)";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    worksheet.Cells[i + 2, 1].Value = item.Index;
                    worksheet.Cells[i + 2, 2].Value = item.FormattedDate;
                    worksheet.Cells[i + 2, 3].Value = item.Penalty;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add summary
                int totalRow = data.Count + 3;
                worksheet.Cells[totalRow, 1].Value = "Tổng cộng";
                worksheet.Cells[totalRow, 3].Value = data.Sum(x => x.Penalty);

                // Style summary row
                using (var range = worksheet.Cells[totalRow, 1, totalRow, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                await package.SaveAsAsync(new FileInfo(filePath));
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xuất file Excel: {ex.Message}");
            }
        }
    }

    // Data classes for export
    public class BorrowingStatisticItem
    {
        public int Index { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
        public double Percentage { get; set; }
    }

    public class LateReturnStatisticItem
    {
        public int Index { get; set; }
        public string BookName { get; set; } = string.Empty;
        public DateOnly BorrowDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public int DaysOverdue { get; set; }
        public int Fine { get; set; }
        public string FormattedBorrowDate => BorrowDate.ToString("dd/MM/yyyy");
        public string FormattedReturnDate => ReturnDate.ToString("dd/MM/yyyy");
    }

    public class PenaltyStatisticItem
    {
        public int Index { get; set; }
        public DateOnly Date { get; set; }
        public int Penalty { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
    }
} 