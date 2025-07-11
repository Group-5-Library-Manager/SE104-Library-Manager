using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SE104_Library_Manager.Views.Book;
using System.IO;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace SE104_Library_Manager.ViewModels.Book
{
    public partial class AddBookImportViewModel : ObservableObject
    {
        private readonly ISachRepository sachRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IStaffSessionReader staffSessionReader;
        private readonly IPhieuNhapRepository phieuNhapRepo;

        [ObservableProperty]
        private string staffCode = string.Empty;
        [ObservableProperty]
        private string staffName = string.Empty;
        [ObservableProperty]
        private DateOnly importDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private int totalQuantity = 0;
        [ObservableProperty]
        private int totalValue = 0;
        [ObservableProperty]
        private ObservableCollection<ChiTietPhieuNhapItemViewModel> importDetails = new();
        [ObservableProperty]
        private ObservableCollection<Sach> allBooks = new();

        public AddBookImportViewModel(
            ISachRepository sachRepo,
            INhanVienRepository nhanVienRepo,
            IStaffSessionReader staffSessionReader,
            IPhieuNhapRepository phieuNhapRepo)
        {
            this.sachRepo = sachRepo;
            this.nhanVienRepo = nhanVienRepo;
            this.staffSessionReader = staffSessionReader;
            this.phieuNhapRepo = phieuNhapRepo;
            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load all books
                var books = await sachRepo.GetAllAsync();
                AllBooks = new ObservableCollection<Sach>(books);

                // Load current staff
                if (staffSessionReader.CurrentStaffId > 0)
                {
                    var staff = await nhanVienRepo.GetByIdAsync(staffSessionReader.CurrentStaffId);
                    if (staff != null)
                    {
                        StaffCode = $"NV{staff.MaNhanVien:D3}";
                        StaffName = staff.TenNhanVien;
                    }
                }
                ImportDate = DateOnly.FromDateTime(DateTime.Now);
                ImportDetails.Clear();
                AddBookRow();
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AddBookRow()
        {
            var item = new ChiTietPhieuNhapItemViewModel(AllBooks, ImportDetails.Select(d => d.SelectedBook?.MaSach ?? -1).ToList());
            item.PropertyChanged += ImportDetail_PropertyChanged;
            ImportDetails.Add(item);
        }

        [RelayCommand]
        private void RemoveBookRow(ChiTietPhieuNhapItemViewModel? item)
        {
            if (item != null && ImportDetails.Contains(item))
            {
                ImportDetails.Remove(item);
                UpdateAvailableBooksForAllRows();
                UpdateTotals();
            }
        }

        private void ImportDetail_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChiTietPhieuNhapItemViewModel.SelectedBook) ||
                e.PropertyName == nameof(ChiTietPhieuNhapItemViewModel.Quantity) ||
                e.PropertyName == nameof(ChiTietPhieuNhapItemViewModel.UnitPrice))
            {
                UpdateAvailableBooksForAllRows();
                UpdateTotals();
            }
        }

        private void UpdateAvailableBooksForAllRows()
        {
            var selectedIds = ImportDetails.Where(d => d.SelectedBook != null).Select(d => d.SelectedBook!.MaSach).ToList();
            foreach (var item in ImportDetails)
            {
                item.UpdateAvailableBooks(AllBooks, selectedIds);
            }
        }

        private void UpdateTotals()
        {
            TotalQuantity = ImportDetails.Sum(d => d.Quantity);
            TotalValue = ImportDetails.Sum(d => d.Quantity * d.UnitPrice);
        }

        [RelayCommand]
        private async Task SaveImport(Window w)
        {
            try
            {
                if (ImportDetails.Count == 0 || ImportDetails.Any(d => d.SelectedBook == null || d.Quantity <= 0 || d.UnitPrice <= 0))
                {
                    MessageBox.Show("Vui lòng diền đầy đủ thông tin nhập sách", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var phieuNhap = new PhieuNhap
                {
                    MaNhanVien = staffSessionReader.CurrentStaffId,
                    NgayNhap = ImportDate,
                };
                var chiTietList = ImportDetails.Select(d => new ChiTietPhieuNhap
                {
                    MaPhieuNhap = 0, // This will be set by the repository
                    MaSach = d.SelectedBook!.MaSach,
                    SoLuong = d.Quantity,
                    DonGiaNhap = d.UnitPrice
                }).ToList();
                
                await phieuNhapRepo.TaoPhieuNhapAsync(phieuNhap, chiTietList);
                MessageBox.Show("Nhập thêm sách thành công", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                w.DialogResult = true;
                w.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập thêm sách: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddNewBook()
        {
            try
            {
                var addBookViewModel = App.ServiceProvider?.GetService(typeof(AddBookViewModel)) as AddBookViewModel;
                if (addBookViewModel == null)
                {
                    MessageBox.Show("Không thể khởi tạo AddBookViewModel", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var addBookWindow = new AddBookWindow(addBookViewModel);
                
                if (addBookWindow.ShowDialog() == true)
                {
                    // Refresh the book list after adding a new book
                    var books = await sachRepo.GetAllAsync();
                    AllBooks = new ObservableCollection<Sach>(books);
                    UpdateAvailableBooksForAllRows();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sách mới: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window w)
        {
            w.DialogResult = false;
            w.Close();
        }

        [RelayCommand]
        private async Task AddAndExportImportReceiptToPdf(Window w)
        {
            try
            {
                if (ImportDetails.Count == 0 || ImportDetails.Any(d => d.SelectedBook == null || d.Quantity <= 0 || d.UnitPrice <= 0))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin nhập sách", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var phieuNhap = new PhieuNhap
                {
                    MaNhanVien = staffSessionReader.CurrentStaffId,
                    NgayNhap = ImportDate,
                };
                var chiTietList = ImportDetails.Select(d => new ChiTietPhieuNhap
                {
                    MaPhieuNhap = 0,
                    MaSach = d.SelectedBook!.MaSach,
                    SoLuong = d.Quantity,
                    DonGiaNhap = d.UnitPrice
                }).ToList();
                await phieuNhapRepo.TaoPhieuNhapAsync(phieuNhap, chiTietList);

                // Lấy lại phiếu nhập vừa thêm 
                var allPhieuNhap = await phieuNhapRepo.GetAllAsync();
                var phieuNhapMoi = allPhieuNhap.OrderByDescending(p => p.MaPhieuNhap).FirstOrDefault();
                if (phieuNhapMoi == null)
                {
                    MessageBox.Show("Không tìm thấy phiếu nhập vừa thêm để xuất PDF", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await phieuNhapRepo.ExportToPdf(phieuNhapMoi);

                MessageBox.Show("Thêm và xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                w.DialogResult = true;
                w.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm và xuất PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}