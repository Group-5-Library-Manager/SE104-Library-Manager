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
using Microsoft.Win32;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Kernel.Sketches;

namespace SE104_Library_Manager.ViewModels.Statistic
{
    public partial class PenaltyStatisticViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly ExcelExportService _excelExportService;

        [ObservableProperty]
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime _toDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<PenaltyStatisticItem> _penaltyItems = new();

        [ObservableProperty]
        private int _totalPenalty = 0;

        [ObservableProperty]
        private ISeries[] _series = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _xAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private Axis[] _yAxes = Array.Empty<Axis>();

        public PenaltyStatisticViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
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

                // Query the database for penalty data from PhieuPhat
                var penaltyData = await GetPenaltyDataAsync(fromDateOnly, toDateOnly);

                // Update the UI with the results
                PenaltyItems = new ObservableCollection<PenaltyStatisticItem>(penaltyData);
                TotalPenalty = penaltyData.Sum(item => item.Penalty);

                // Update the chart
                UpdateChart(penaltyData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu thống kê: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<PenaltyStatisticItem>> GetPenaltyDataAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Query the database for PhieuPhat records within the date range
            var phieuPhatList = await _dbService.DbContext.DsPhieuPhat
                .Where(pp => !pp.DaXoa && pp.NgayLap >= fromDate && pp.NgayLap <= toDate)
                .ToListAsync();

            // Group by date and calculate penalty collected for each day
            var penaltyByDate = phieuPhatList
                .GroupBy(pp => pp.NgayLap)
                .Select((group, index) => new PenaltyStatisticItem
                {
                    Index = index + 1,
                    Date = group.Key,
                    Penalty = group.Sum(pp => pp.TienThu), // Use TienThu (amount collected) from PhieuPhat
                    FormattedDate = group.Key.ToString("dd/MM/yyyy")
                })
                .OrderBy(item => item.Date)
                .ToList();

            return penaltyByDate;
        }

        private void UpdateChart(List<PenaltyStatisticItem> penaltyData)
        {
            if (penaltyData.Count == 0)
            {
                Series = Array.Empty<ISeries>();
                XAxes = Array.Empty<Axis>();
                YAxes = Array.Empty<Axis>();
                return;
            }

            // Create chart series
            Series = new ISeries[]
            {
                new LineSeries<PenaltyStatisticItem>
                {
                    Values = penaltyData,
                    GeometrySize = 10,
                    Stroke = new SolidColorPaint(SKColors.Blue, 3),
                    Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(100)),
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue, 2),
                    LineSmoothness = 0.5,
                    Mapping = (item, index) => new LiveChartsCore.Kernel.Coordinate(index, item.Penalty)
                }
            };

            // Create X-axis with dates
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = penaltyData.Select(item => item.FormattedDate).ToArray(),
                    LabelsRotation = 45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 1),
                    TextSize = 12
                }
            };

            // Create Y-axis
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Tiền phạt (VNĐ)",
                    NameTextSize = 14,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray, 1),
                    Labeler = value => $"{value:N0}"
                }
            };
        }

        [RelayCommand]
        private async Task ExportToExcel()
        {
            try
            {
                if (PenaltyItems.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"Thong_ke_tien_thu_phat_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Convert to export format
                    var exportData = PenaltyItems.Select(item => new Services.PenaltyStatisticItem
                    {
                        Index = item.Index,
                        Date = item.Date,
                        Penalty = item.Penalty,
                        FormattedDate = item.FormattedDate
                    }).ToList();

                    await _excelExportService.ExportPenaltyStatisticAsync(exportData, saveFileDialog.FileName);
                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class PenaltyStatisticItem
    {
        public int Index { get; set; }
        public DateOnly Date { get; set; }
        public int Penalty { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
    }
}