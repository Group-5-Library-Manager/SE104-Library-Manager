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

            // Query the database for PhieuTra records within the date range
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

            // Calculate overdue days and create result items
            var result = new List<LateReturnStatisticItem>();
            int index = 1;

            foreach (var chiTiet in chiTietPhieuTraList)
            {
                // Calculate overdue days
                int daysOverdue = CalculateOverdueDays(chiTiet.PhieuMuon.NgayMuon, chiTiet.PhieuTra.NgayTra, maxBorrowDays);

                if (daysOverdue > 0)
                {
                    result.Add(new LateReturnStatisticItem
                    {
                        Index = index++,
                        BookName = chiTiet.BanSaoSach.Sach.TenSach,
                        BorrowDate = chiTiet.PhieuMuon.NgayMuon,
                        ReturnDate = chiTiet.PhieuTra.NgayTra,
                        DaysOverdue = daysOverdue,
                        Fine = chiTiet.TienPhat
                    });
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
                        ReturnDate = item.ReturnDate,
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
        public DateOnly ReturnDate { get; set; }
        public int DaysOverdue { get; set; }
        public int Fine { get; set; }
        public string FormattedBorrowDate => BorrowDate.ToString("dd/MM/yyyy");
        public string FormattedReturnDate => ReturnDate.ToString("dd/MM/yyyy");
        public string FormattedFine => $"{Fine:N0} VND";
    }
}