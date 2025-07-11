using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SE104_Library_Manager.ViewModels.Statistic
{
    public partial class LateReturnStatisticViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly IQuyDinhRepository _quyDinhRepo;
        private readonly ExcelExportService _excelExportService;

        [ObservableProperty]
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _toDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<LateReturnStatisticItem> _lateReturnItems = new();

        // Chart properties for LiveCharts2
        [ObservableProperty]
        private ISeries[] _series = new ISeries[0];
        [ObservableProperty]
        private Axis[] _xAxes = new Axis[0];
        [ObservableProperty]
        private Axis[] _yAxes = new Axis[0];

        public LateReturnStatisticViewModel(DatabaseService dbService, IQuyDinhRepository quyDinhRepo)
        {
            _dbService = dbService;
            _quyDinhRepo = quyDinhRepo;
            _excelExportService = new ExcelExportService();
        }

        [RelayCommand]
        private async Task LoadStatistic()
        {
            try
            {
                // Convert DateTime to DateOnly for database queries
                DateOnly fromDateOnly = DateOnly.FromDateTime(FromDate);
                DateOnly toDateOnly = DateOnly.FromDateTime(ToDate);

                // Query the database for late return data
                var lateReturnData = await GetLateReturnDataAsync(fromDateOnly, toDateOnly);

                // Update the UI with the results
                LateReturnItems = new ObservableCollection<LateReturnStatisticItem>(lateReturnData);

                // Prepare chart data: Bar chart, X = BookName, Y = DaysOverdue (sum for each book)
                var chartData = lateReturnData
                    .GroupBy(x => x.BookName)
                    .Select(g => new { BookName = g.Key, TotalDaysOverdue = g.Sum(x => x.DaysOverdue) })
                    .OrderByDescending(x => x.TotalDaysOverdue)
                    .ToList();

                Series = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = chartData.Select(x => x.TotalDaysOverdue).ToArray(),
                        Name = "Số ngày trả trễ"
                    }
                };
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = chartData.Select(x => x.BookName).ToArray(),
                        LabelsRotation = 15,
                        Name = "Tên sách"
                    }
                };
                YAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Tổng số ngày trả trễ"
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<LateReturnStatisticItem>> GetLateReturnDataAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Get the maximum borrow days from QuyDinh
            var quyDinh = await _quyDinhRepo.GetQuyDinhAsync();
            int maxBorrowDays = quyDinh.SoNgayMuonToiDa;

            var result = new List<LateReturnStatisticItem>();
            int index = 1;

            // 1. Returned late books (as before)
            var chiTietPhieuTraList = await _dbService.DbContext.DsChiTietPhieuTra
                .Include(ct => ct.PhieuTra)
                .Include(ct => ct.PhieuMuon)
                .Include(ct => ct.BanSaoSach)
                    .ThenInclude(bs => bs.Sach)
                .Where(ct => !ct.DaXoa &&
                       !ct.PhieuTra.DaXoa &&
                       ct.PhieuTra.NgayTra >= fromDate &&
                       ct.PhieuTra.NgayTra <= toDate &&
                       ct.TienPhat > 0) // Only include items with fines
                .ToListAsync();

            foreach (var chiTiet in chiTietPhieuTraList)
            {
                int daysOverdue = CalculateOverdueDays(
                    chiTiet.PhieuMuon.NgayMuon, // NgayMuon is required, not nullable
                    chiTiet.PhieuTra.NgayTra,   // NgayTra is required, not nullable
                    maxBorrowDays);
                if (daysOverdue > 0)
                {
                    result.Add(new LateReturnStatisticItem
                    {
                        Index = index++,
                        BookName = chiTiet.BanSaoSach.Sach.TenSach,
                        BorrowDate = chiTiet.PhieuMuon.NgayMuon, // not nullable
                        ReturnDate = chiTiet.PhieuTra.NgayTra,   // not nullable
                        DaysOverdue = daysOverdue,
                        Fine = chiTiet.TienPhat
                    });
                }
            }

            // 2. Overdue books not yet returned
            var today = DateOnly.FromDateTime(DateTime.Now);
            var allBorrowings = await _dbService.DbContext.DsChiTietPhieuMuon
                .Include(ct => ct.PhieuMuon)
                .Include(ct => ct.BanSaoSach)
                    .ThenInclude(bs => bs.Sach)
                .Where(ct => !ct.PhieuMuon.DaXoa &&
                             ct.PhieuMuon.NgayMuon >= fromDate &&
                             ct.PhieuMuon.NgayMuon <= toDate)
                .ToListAsync();

            var allReturnedPairs = await _dbService.DbContext.DsChiTietPhieuTra
                .Where(ctpt => !ctpt.DaXoa)
                .Select(ctpt => new { ctpt.MaPhieuMuon, ctpt.MaBanSao })
                .ToListAsync();

            foreach (var chiTiet in allBorrowings)
            {
                bool isReturned = allReturnedPairs.Any(pair =>
                    pair.MaPhieuMuon == chiTiet.MaPhieuMuon &&
                    pair.MaBanSao == chiTiet.MaBanSao);

                if (!isReturned)
                {
                    int daysOverdue = CalculateOverdueDays(
                        chiTiet.PhieuMuon.NgayMuon,
                        today,
                        maxBorrowDays);
                    if (daysOverdue > 0)
                    {
                        result.Add(new LateReturnStatisticItem
                        {
                            Index = index++,
                            BookName = chiTiet.BanSaoSach.Sach.TenSach,
                            BorrowDate = chiTiet.PhieuMuon.NgayMuon, // not nullable
                            ReturnDate = null, // Not yet returned
                            DaysOverdue = daysOverdue,
                            Fine = 0 // Not yet fined
                        });
                    }
                }
            }

            return result.OrderByDescending(item => item.DaysOverdue).ToList();
        }

        private int CalculateOverdueDays(DateOnly borrowDate, DateOnly returnDate, int maxBorrowDays)
        {
            // Calculate the due date
            DateOnly dueDate = borrowDate.AddDays(maxBorrowDays);

            // Calculate days overdue
            if (returnDate > dueDate)
            {
                return returnDate.DayNumber - dueDate.DayNumber;
            }

            return 0;
        }

        [RelayCommand]
        private async Task ExportToExcel()
        {
            try
            {
                if (LateReturnItems.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Thong_ke_sach_tra_tre_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Convert to export format
                    var exportData = LateReturnItems.Select(item => new Services.LateReturnStatisticItem
                    {
                        Index = item.Index,
                        BookName = item.BookName,
                        BorrowDate = item.BorrowDate,
                        ReturnDate = (DateOnly)item.ReturnDate,
                        DaysOverdue = item.DaysOverdue,
                        Fine = item.Fine
                    }).ToList();

                    await _excelExportService.ExportLateReturnStatisticAsync(exportData, saveFileDialog.FileName);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class LateReturnStatisticItem
    {
        public int Index { get; set; }
        public string BookName { get; set; } = string.Empty;
        public DateOnly BorrowDate { get; set; }
        public DateOnly? ReturnDate { get; set; } // Nullable for not yet returned
        public int DaysOverdue { get; set; }
        public int Fine { get; set; }
        public string FormattedBorrowDate => BorrowDate.ToString("dd/MM/yyyy");
        public string FormattedReturnDate => ReturnDate.HasValue ? ReturnDate.Value.ToString("dd/MM/yyyy") : "Chưa trả";
        public string FormattedFine => $"{Fine:N0} VND";
    }
}