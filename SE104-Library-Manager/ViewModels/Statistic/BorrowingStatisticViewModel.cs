using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Statistic
{
    public partial class BorrowingStatisticViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _toDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<BorrowingStatisticItem> _borrowingItems = new();

        [ObservableProperty]
        private int _totalBorrows = 0;

        public BorrowingStatisticViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [RelayCommand]
        private async Task LoadStatistic()
        {
            try
            {
                // Convert DateTime to DateOnly for database queries
                DateOnly fromDateOnly = DateOnly.FromDateTime(FromDate);
                DateOnly toDateOnly = DateOnly.FromDateTime(ToDate);

                // Query the database for borrowing data
                var borrowingData = await GetBorrowingDataAsync(fromDateOnly, toDateOnly);

                // Update the UI with the results
                BorrowingItems = new ObservableCollection<BorrowingStatisticItem>(borrowingData);
                TotalBorrows = borrowingData.Sum(item => item.BorrowCount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<BorrowingStatisticItem>> GetBorrowingDataAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Query the database for PhieuMuon records within the date range
            var chiTietPhieuMuonList = await _dbService.DbContext.DsChiTietPhieuMuon
                .Include(ct => ct.PhieuMuon)
                .Include(ct => ct.Sach)
                .ThenInclude(s => s.TheLoai)
                .Where(ct => !ct.PhieuMuon.DaXoa &&
                       ct.PhieuMuon.NgayMuon >= fromDate &&
                       ct.PhieuMuon.NgayMuon <= toDate)
                .ToListAsync();

            // Group by genre and count borrows
            var borrowsByGenre = chiTietPhieuMuonList
                .GroupBy(ct => ct.Sach.TheLoai.MaTheLoai)
                .Select((group, index) => new
                {
                    GenreId = group.Key,
                    GenreName = group.First().Sach.TheLoai.TenTheLoai,
                    Count = group.Count()
                })
                .OrderByDescending(item => item.Count)
                .ToList();

            // Calculate percentages
            int totalBorrows = borrowsByGenre.Sum(item => item.Count);
            var result = borrowsByGenre.Select((item, index) => new BorrowingStatisticItem
            {
                Index = index + 1,
                GenreName = item.GenreName,
                BorrowCount = item.Count,
                Percentage = totalBorrows > 0 ? (double)item.Count / totalBorrows * 100 : 0
            }).ToList();

            return result;
        }
    }

    public class BorrowingStatisticItem
    {
        public int Index { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public int BorrowCount { get; set; }
        public double Percentage { get; set; }
        public string FormattedPercentage => $"{Percentage:F2}%";
    }
}