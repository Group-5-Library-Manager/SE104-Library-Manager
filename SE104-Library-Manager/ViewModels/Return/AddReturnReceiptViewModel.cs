using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views.Return;
using System.Collections.ObjectModel;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Return;

public partial class AddReturnReceiptViewModel : ObservableObject
{
    private readonly IPhieuTraRepository phieuTraRepo;
    private readonly IChiTietPhieuTraRepository chiTietPhieuTraRepo;
    private readonly IQuyDinhRepository quyDinhRepo;
    private readonly IDocGiaRepository docGiaRepo;
    private readonly IStaffSessionReader staffSessionReader;
    private readonly INhanVienRepository nhanVienRepo;

    [ObservableProperty] private ObservableCollection<DocGia> readers = new();
    [ObservableProperty] private DocGia? selectedReader;
    [ObservableProperty] private ObservableCollection<BookReturnItem> selectedBooks = new();
    [ObservableProperty] private int tienPhatKyNay;
    [ObservableProperty] private DateOnly returnDate = DateOnly.FromDateTime(DateTime.Now);
    [ObservableProperty] private NhanVien? currentStaff;
    [ObservableProperty] private int tongNoHienTai;

    public bool HasSelectedReader => SelectedReader != null;

    private int tongNoBanDau;

    public AddReturnReceiptViewModel(
        IPhieuTraRepository phieuTraRepo,
        IChiTietPhieuTraRepository chiTietPhieuTraRepo,
        IQuyDinhRepository quyDinhRepo,
        IDocGiaRepository docGiaRepo,
        IStaffSessionReader staffSessionReader,
        INhanVienRepository nhanVienRepo)
    {
        this.phieuTraRepo = phieuTraRepo;
        this.chiTietPhieuTraRepo = chiTietPhieuTraRepo;
        this.quyDinhRepo = quyDinhRepo;
        this.docGiaRepo = docGiaRepo;
        this.staffSessionReader = staffSessionReader;
        this.nhanVienRepo = nhanVienRepo;

        LoadDataAsync().ConfigureAwait(false);
    }

    public async Task LoadDataAsync()
    {
        Readers = new ObservableCollection<DocGia>(await phieuTraRepo.GetDocGiaDangCoSachMuonAsync());
        SelectedBooks.Clear();
        SelectedReader = null;
        TienPhatKyNay = 0;

        ReturnDate = DateOnly.FromDateTime(DateTime.Now);

        int currentStaffId = staffSessionReader.CurrentStaffId;
        CurrentStaff = await nhanVienRepo.GetByIdAsync(currentStaffId);
    }

    partial void OnSelectedReaderChanged(DocGia? value)
    {
        SelectedBooks.Clear();
        TienPhatKyNay = 0;
        if (value != null)
        {
            tongNoBanDau = value.TongNo;
            AddBook();
        }
        else
        {
            tongNoBanDau = 0;
        }

        UpdateTongNoHienTai();

        OnPropertyChanged(nameof(HasSelectedReader));
    }

    [RelayCommand]
    private void AddBook()
    {
        if (SelectedReader == null) return;

        var newItem = new BookReturnItem(phieuTraRepo, quyDinhRepo, SelectedReader.MaDocGia, UpdateTienPhatKyNay);
        SelectedBooks.Add(newItem);
    }

    [RelayCommand]
    private void RemoveBook(BookReturnItem? item)
    {
        if (item != null)
        {
            SelectedBooks.Remove(item);
            UpdateTienPhatKyNay();
        }
    }

    private async void UpdateTienPhatKyNay()
    {
        int total = 0;
        foreach (var item in SelectedBooks)
        {
            total += await item.CalculateFineAsync();
        }
        TienPhatKyNay = total;
        UpdateTongNoHienTai();
    }

    private void UpdateTongNoHienTai()
    {
        TongNoHienTai = tongNoBanDau + TienPhatKyNay;
    }

    [RelayCommand]
    private async Task ConfirmReturnAsync()
    {
        if (SelectedReader == null || SelectedBooks.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn độc giả và ít nhất một sách để trả.");
            return;
        }

        var validBooks = SelectedBooks
            .Where(b => b.SelectedBook != null)
            .ToList();

        if (validBooks.Count == 0)
        {
            MessageBox.Show("Danh sách sách không hợp lệ.");
            return;
        }

        // Kiểm tra sách trùng nhau
        var distinctBooks = validBooks.Select(b => b.SelectedBook?.MaSach).Distinct().Count();
        if (distinctBooks < validBooks.Count)
        {
            MessageBox.Show("Có sách bị trùng trong danh sách. Vui lòng kiểm tra lại.");
            return;
        }

        // Kiểm tra số lượng trả hợp lệ
        foreach (var item in validBooks)
        {
            if (item.ReturnQuantity <= 0)
            {
                MessageBox.Show($"Số lượng trả cho sách '{item.SelectedBook?.TenSach}' phải lớn hơn 0.");
                return;
            }
            if (item.ReturnQuantity > item.BorrowedQuantity)
            {
                MessageBox.Show($"Số lượng trả cho sách '{item.SelectedBook?.TenSach}' không được vượt quá số lượng đã mượn ({item.BorrowedQuantity}).");
                return;
            }
        }

        try
        {
            // Thêm phiếu trả mới
            var phieuTra = new PhieuTra
            {
                MaNhanVien = staffSessionReader.CurrentStaffId,
                MaDocGia = SelectedReader.MaDocGia,
                NgayTra = ReturnDate,
                TienPhatKyNay = TienPhatKyNay,
                TongNo = TongNoHienTai
            };

            await phieuTraRepo.AddAsync(phieuTra);

            var dsChiTiet = new List<ChiTietPhieuTra>();

            foreach (var item in validBooks)
            {
                if (item.SelectedBook == null) continue;

                var chiTietMuon = await phieuTraRepo.GetChiTietMuonMoiNhatChuaTraAsync(item.SelectedBook.MaSach);
                if (chiTietMuon == null) continue;

                dsChiTiet.Add(new ChiTietPhieuTra
                {
                    MaPhieuTra = phieuTra.MaPhieuTra,
                    MaPhieuMuon = chiTietMuon.MaPhieuMuon,
                    MaSach = item.SelectedBook.MaSach,
                    SoLuongTra = item.ReturnQuantity,
                    TienPhat = item.Fine
                });
            }

            if (dsChiTiet.Count > 0)
            {
                await chiTietPhieuTraRepo.AddRangeAsync(dsChiTiet);
                MessageBox.Show("Trả sách thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                SelectedReader.TongNo = TongNoHienTai;
                await docGiaRepo.UpdateAsync(SelectedReader);

                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            else
            {
                throw new InvalidOperationException("Không có sách nào hợp lệ để trả.");
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi trả sách: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public partial class BookReturnItem : ObservableObject
    {
        private readonly IPhieuTraRepository phieuTraRepo;
        private readonly IQuyDinhRepository quyDinhRepo;
        private readonly int maDocGia;
        private readonly Action updateParentTotal;

        public ObservableCollection<Sach> BorrowedBooks { get; } = new();

        [ObservableProperty] private Sach? selectedBook;
        [ObservableProperty] private DateOnly? borrowDate;
        [ObservableProperty] private int fine;
        [ObservableProperty] private int borrowedQuantity;
        [ObservableProperty] private int returnQuantity = 1;
        [ObservableProperty] private int currentStock;
        [ObservableProperty] private string currentStatus = "";

        public BookReturnItem(IPhieuTraRepository repo, IQuyDinhRepository quyDinh, int maDocGia, Action onChange)
        {
            phieuTraRepo = repo;
            quyDinhRepo = quyDinh;
            this.maDocGia = maDocGia;
            updateParentTotal = onChange;
            _ = LoadBorrowedBooksAsync();
        }

        private async Task LoadBorrowedBooksAsync()
        {
            var chiTietDangMuon = await phieuTraRepo.GetSachDangMuonByDocGiaAsync(maDocGia);
            var books = chiTietDangMuon.Select(ct => ct.Sach).ToList();

            BorrowedBooks.Clear();
            foreach (var b in books)
                BorrowedBooks.Add(b);
        }

        partial void OnSelectedBookChanged(Sach? value)
        {
            if (value != null)
            {
                CurrentStock = value.SoLuongHienCo;
                CurrentStatus = value.TrangThai;
                ReturnQuantity = 1; // Reset to 1 when book changes
                _ = UpdateFineAsync();
            }
            else
            {
                CurrentStock = 0;
                CurrentStatus = "";
                ReturnQuantity = 1;
                BorrowedQuantity = 0;
            }
        }

        partial void OnReturnQuantityChanged(int value)
        {
            if (value > BorrowedQuantity)
            {
                ReturnQuantity = BorrowedQuantity;
            }
            else if (value < 1)
            {
                ReturnQuantity = 1;
            }
            _ = UpdateFineAsync();
        }

        public async Task<int> CalculateFineAsync()
        {
            if (SelectedBook == null) return 0;
            var quyDinh = await quyDinhRepo.GetQuyDinhAsync();
            var chiTiet = await phieuTraRepo.GetChiTietMuonMoiNhatChuaTraAsync(SelectedBook.MaSach);

            if (chiTiet?.PhieuMuon != null)
            {
                BorrowDate = chiTiet.PhieuMuon.NgayMuon;
                BorrowedQuantity = chiTiet.SoLuongMuon;
                
                // Ensure return quantity doesn't exceed borrowed quantity
                if (ReturnQuantity > BorrowedQuantity)
                {
                    ReturnQuantity = BorrowedQuantity;
                }
                
                var today = DateOnly.FromDateTime(DateTime.Now);
                int soNgayTre = (today.DayNumber - BorrowDate.Value.DayNumber) - quyDinh.SoNgayMuonToiDa;
                int finePerBook = soNgayTre > 0 ? soNgayTre * quyDinh.TienPhatQuaHanMoiNgay : 0;
                return finePerBook * ReturnQuantity;
            }
            return 0;
        }

        private async Task UpdateFineAsync()
        {
            Fine = await CalculateFineAsync();
            updateParentTotal();
        }
    }

    [RelayCommand]
    public void Cancel(AddReturnReceiptWindow w)
    {
        w.Close();
    }
}