using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace SE104_Library_Manager.ViewModels.Statistic
{
    public partial class RevenueStatisticViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _toDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<RevenueStatisticItem> _revenueItems = new();

        [ObservableProperty]
        private int _totalRevenue = 0;

        [ObservableProperty]
        private ISeries[] _series = Array.Empty<ISeries>();

        [ObservableProperty]
        private PointF[] _chartPoints = Array.Empty<PointF>();

        public RevenueStatisticViewModel(DatabaseService dbService)
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

                // Query the database for revenue data
                var revenueData = await GetRevenueDataAsync(fromDateOnly, toDateOnly);

                // Update the UI with the results
                RevenueItems = new ObservableCollection<RevenueStatisticItem>(revenueData);
                TotalRevenue = revenueData.Sum(item => item.Revenue);

                // Update the chart
                UpdateChart(revenueData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<RevenueStatisticItem>> GetRevenueDataAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Query the database for PhieuTra records within the date range
            var phieuTraList = await _dbService.DbContext.DsPhieuTra
                .Where(pt => !pt.DaXoa && pt.NgayTra >= fromDate && pt.NgayTra <= toDate)
                .ToListAsync();

            // Group by date and calculate revenue for each day
            var revenueByDate = phieuTraList
                .GroupBy(pt => pt.NgayTra)
                .Select((group, index) => new RevenueStatisticItem
                {
                    Index = index + 1,
                    Date = group.Key,
                    Revenue = group.Sum(pt => pt.TienPhatKyNay),
                    FormattedDate = group.Key.ToString("dd/MM/yyyy")
                })
                .OrderBy(item => item.Date)
                .ToList();

            return revenueByDate;
        }

        private void UpdateChart(List<RevenueStatisticItem> revenueData)
        {
            // Create chart series
            Series = new ISeries[]
            {
                new LineSeries<RevenueStatisticItem>
                {
                    Values = revenueData,
                    GeometrySize = 10,
                    Stroke = new SolidColorPaint(SKColors.Blue, 3),
                    Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(100)),
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue, 2),
                    LineSmoothness = 0.5,
                    Mapping = (item, index) => new LiveChartsCore.Kernel.Coordinate(index, item.Revenue)
                }
            };

            // Create chart points for the polygon
            var points = new List<PointF>();
            if (revenueData.Count > 0)
            {
                float xScale = 700f / Math.Max(1, revenueData.Count - 1); // Width of chart area is 700
                int maxRevenue = revenueData.Max(item => item.Revenue);
                float yScale = maxRevenue > 0 ? 300f / maxRevenue : 1f; // Height of chart area is 300

                // Add bottom-left point
                points.Add(new PointF(50, 300)); // Starting point at bottom-left of chart area

                // Add data points
                for (int i = 0; i < revenueData.Count; i++)
                {
                    float x = 50 + i * xScale;
                    float y = 300 - revenueData[i].Revenue * yScale;
                    points.Add(new PointF(x, y));
                }

                // Add bottom-right point
                points.Add(new PointF(750, 300)); // Ending point at bottom-right of chart area
            }

            ChartPoints = points.ToArray();
        }
    }

    public class RevenueStatisticItem
    {
        public int Index { get; set; }
        public DateOnly Date { get; set; }
        public int Revenue { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
    }
}