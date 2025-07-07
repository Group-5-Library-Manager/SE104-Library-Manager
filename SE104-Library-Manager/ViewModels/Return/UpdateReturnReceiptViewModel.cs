using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views.Return;
using System.Collections.ObjectModel;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class UpdateReturnReceiptViewModel : ObservableObject
{
    [ObservableProperty] private QuyDinh? quyDinh;
    [ObservableProperty] private ObservableCollection<BookReturnItem> dsSachTra = new();
    [ObservableProperty] private PhieuTra? selectedReturn;
    [ObservableProperty] private int tienPhatKyNay;
    [ObservableProperty] private int tongNoHienTai;
    private int tongNoBanDau;

    private List<SelectableBook> allSelectableBooks = new();

    private readonly IPhieuTraRepository phieuTraRepo;
    private readonly IChiTietPhieuTraRepository chiTietPhieuTraRepo;
    private readonly IDocGiaRepository docGiaRepo;
    private readonly IQuyDinhRepository quyDinhRepo;

    public UpdateReturnReceiptViewModel(
        IPhieuTraRepository phieuTraRepo,
        IChiTietPhieuTraRepository chiTietPhieuTraRepo,
        IDocGiaRepository docGiaRepo,
        IQuyDinhRepository quyDinhRepo
    )
    {
        this.phieuTraRepo = phieuTraRepo;
        this.chiTietPhieuTraRepo = chiTietPhieuTraRepo;
        this.docGiaRepo = docGiaRepo;
        this.quyDinhRepo = quyDinhRepo;
    }

    [RelayCommand]
    public async Task LoadDataAsync(int maPhieuTra)
    {
        var phieuTra = await phieuTraRepo.GetByIdAsync(maPhieuTra);
        if (phieuTra == null) return;

        SelectedReturn = phieuTra;

        tongNoBanDau = SelectedReturn.DocGia?.TongNo ?? 0;

        var sachDangMuon = await phieuTraRepo.GetSachDangMuonByDocGiaAsync(phieuTra.MaDocGia);

        // Build AllSelectableBooks
        allSelectableBooks = sachDangMuon
            .Select(s => new SelectableBook
            {
                MaPhieuMuon = s.MaPhieuMuon,
                MaSach = s.MaSach,
                TenSach = s.Sach.TenSach,
            })
            .Concat(phieuTra.DsChiTietPhieuTra.Select(ct => new SelectableBook
            {
                MaPhieuMuon = ct.MaPhieuMuon,
                MaSach = ct.MaSach,
                TenSach = ct.Sach.TenSach
            }))
            .GroupBy(b => new { b.MaSach, b.MaPhieuMuon })
            .Select(g => g.First())
            .ToList();

        QuyDinh = await quyDinhRepo.GetQuyDinhAsync();

        DsSachTra.Clear();
        foreach (var ct in phieuTra.DsChiTietPhieuTra)
        {
            var item = new BookReturnItem(this, QuyDinh!)
            {
                MaPhieuMuon = ct.MaPhieuMuon,
                MaSach = ct.MaSach,
                TenSach = ct.Sach.TenSach,
                NgayMuon = ct.PhieuMuon?.NgayMuon ?? DateOnly.MinValue,
                TienPhat = ct.TienPhat,
                SelectedBook = allSelectableBooks
                    .FirstOrDefault(b => b.MaSach == ct.MaSach && b.MaPhieuMuon == ct.MaPhieuMuon)
            };
            DsSachTra.Add(item);
        }

        UpdateAllAvailableBooks();
        TinhTongTienPhatKyNay();
        CapNhatTongNoHienTai();
    }

    [RelayCommand]
    public void RemoveBook(BookReturnItem? item)
    {
        if (item == null) return;

        DsSachTra.Remove(item);
        UpdateAllAvailableBooks();
        TinhTongTienPhatKyNay();
    }

    [RelayCommand]
    public void AddBook()
    {
        if (SelectedReturn == null || QuyDinh == null) return;

        var available = CreateAvailableBooksList();
        if (!available.Any())
        {
            MessageBox.Show("Không còn sách nào để chọn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var item = new BookReturnItem(this, QuyDinh);
        DsSachTra.Add(item);
        UpdateAllAvailableBooks();
    }

    private void UpdateAllAvailableBooks()
    {
        var selectedIds = DsSachTra
            .Where(i => i.SelectedBook != null)
            .Select(i => (i.SelectedBook!.MaSach, i.SelectedBook.MaPhieuMuon))
            .ToList();

        foreach (var item in DsSachTra)
        {
            var current = item.SelectedBook;

            var list = allSelectableBooks
                .Where(s => !selectedIds.Contains((s.MaSach, s.MaPhieuMuon))
                    || (current != null && s.MaSach == current.MaSach && s.MaPhieuMuon == current.MaPhieuMuon))
                .OrderBy(s => s.TenSach)
                .ToList();

            SyncAvailableBooks(item, list);
        }
    }

    private List<SelectableBook> CreateAvailableBooksList(BookReturnItem? excludeItem = null)
    {
        var selected = DsSachTra
            .Where(i => i.SelectedBook != null && i != excludeItem)
            .Select(i => (i.SelectedBook!.MaSach, i.SelectedBook.MaPhieuMuon))
            .ToList();

        return allSelectableBooks
            .Where(s => !selected.Contains((s.MaSach, s.MaPhieuMuon)))
            .ToList();
    }

    private void TinhTongTienPhatKyNay()
    {
        TienPhatKyNay = DsSachTra.Sum(s => s.TienPhat);
        CapNhatTongNoHienTai();
    }
    private void CapNhatTongNoHienTai()
    {
        // TongNoHienTai = TongNoBanDau - TienPhatCu + TienPhatKyNay
        var tienPhatCu = SelectedReturn?.DsChiTietPhieuTra.Sum(ct => ct.TienPhat) ?? 0;
        TongNoHienTai = tongNoBanDau - tienPhatCu + TienPhatKyNay;
    }

    [RelayCommand]
    public async Task UpdateReturnAsync()
    {
        if (SelectedReturn == null || DsSachTra.Count == 0)
        {
            MessageBox.Show("Thông tin chưa đầy đủ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validate return quantities
        var validItems = DsSachTra.Where(s => s.SelectedBook != null).ToList();
        foreach (var item in validItems)
        {
            if (item.ReturnQuantity <= 0)
            {
                MessageBox.Show($"Số lượng trả cho sách '{item.TenSach}' phải lớn hơn 0.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (item.ReturnQuantity > item.BorrowedQuantity)
            {
                MessageBox.Show($"Số lượng trả cho sách '{item.TenSach}' không được vượt quá số lượng đã mượn ({item.BorrowedQuantity}).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        var maDocGia = SelectedReturn.MaDocGia;

        var chiTietCu = await chiTietPhieuTraRepo.GetByPhieuTraAsync(SelectedReturn.MaPhieuTra);
        var tienPhatCu = chiTietCu.Sum(ct => ct.TienPhat);

        var docGia = await docGiaRepo.GetByIdAsync(maDocGia);
        if (docGia != null)
        {
            docGia.TongNo -= tienPhatCu;
            if (docGia.TongNo < 0) docGia.TongNo = 0;
            await docGiaRepo.UpdateAsync(docGia);
        }

        await chiTietPhieuTraRepo.DeleteByPhieuTraAsync(SelectedReturn.MaPhieuTra);

        var listCT = DsSachTra
            .Where(s => s.SelectedBook != null && s.MaPhieuMuon > 0)
            .Select(s => new ChiTietPhieuTra
            {
                MaPhieuTra = SelectedReturn.MaPhieuTra,
                MaSach = s.SelectedBook!.MaSach,
                MaPhieuMuon = s.MaPhieuMuon,
                SoLuongTra = s.ReturnQuantity,
                TienPhat = s.TienPhat
            }).ToList();

        await chiTietPhieuTraRepo.AddRangeAsync(listCT);

        var tienPhatMoi = listCT.Sum(ct => ct.TienPhat);
        if (docGia != null)
        {
            docGia.TongNo += tienPhatMoi;
            await docGiaRepo.UpdateAsync(docGia);
        }

        SelectedReturn.TienPhatKyNay = tienPhatMoi;

        await phieuTraRepo.UpdateAsync(SelectedReturn);

        MessageBox.Show("Cập nhật phiếu trả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }

    [RelayCommand]
    public void Cancel(UpdateReturnReceiptWindow w)
    {
        w.Close();
    }

    private void SyncAvailableBooks(BookReturnItem item, List<SelectableBook> newList)
    {
        var selectedMaSach = item.SelectedBook?.MaSach;
        var selectedMaPhieuMuon = item.SelectedBook?.MaPhieuMuon;

        for (int i = item.AvailableBooks.Count - 1; i >= 0; i--)
        {
            if (!newList.Any(s => s.MaSach == item.AvailableBooks[i].MaSach && s.MaPhieuMuon == item.AvailableBooks[i].MaPhieuMuon))
            {
                item.AvailableBooks.RemoveAt(i);
            }
        }

        foreach (var sach in newList)
        {
            if (!item.AvailableBooks.Any(s => s.MaSach == sach.MaSach && s.MaPhieuMuon == sach.MaPhieuMuon))
            {
                item.AvailableBooks.Add(sach);
            }
        }

        if (selectedMaSach != null && !item.AvailableBooks.Any(s => s.MaSach == selectedMaSach && s.MaPhieuMuon == selectedMaPhieuMuon))
        {
            item.SelectedBook = null;
        }
    }


    public partial class BookReturnItem : ObservableObject
    {
        private readonly UpdateReturnReceiptViewModel vm;

        public BookReturnItem(UpdateReturnReceiptViewModel vm, QuyDinh quyDinh)
        {
            this.vm = vm;
            this.QuyDinh = quyDinh;
            AvailableBooks = new ObservableCollection<SelectableBook>();
        }

        public ObservableCollection<SelectableBook> AvailableBooks { get; }
        public QuyDinh QuyDinh { get; }

        [ObservableProperty]
        private SelectableBook? selectedBook;

        partial void OnSelectedBookChanged(SelectableBook? value)
        {
            if (value == null) return;

            MaSach = value.MaSach;
            TenSach = value.TenSach;
            MaPhieuMuon = value.MaPhieuMuon;

            var chiTiet = vm.SelectedReturn?.DsChiTietPhieuTra
                .FirstOrDefault(ct => ct.MaPhieuMuon == MaPhieuMuon);
            if (chiTiet?.PhieuMuon != null)
            {
                NgayMuon = chiTiet.PhieuMuon.NgayMuon;
                BorrowedQuantity = chiTiet.SoLuongTra; // For update, this is the returned quantity
                ReturnQuantity = chiTiet.SoLuongTra; // Default to the same quantity
            }

            // Get current book stock and status
            var sach = vm.SelectedReturn?.DsChiTietPhieuTra
                .FirstOrDefault(ct => ct.MaSach == MaSach)?.Sach;
            if (sach != null)
            {
                CurrentStock = sach.SoLuongHienCo;
                CurrentStatus = sach.TrangThai;
            }

            _ = UpdateFineAsync();
            vm.UpdateAllAvailableBooks();
        }

        public int MaSach { get; set; }
        public string TenSach { get; set; } = "";
        public int MaPhieuMuon { get; set; }

        [ObservableProperty]
        private DateOnly ngayMuon;

        [ObservableProperty]
        private int tienPhat;

        [ObservableProperty]
        private int borrowedQuantity;

        [ObservableProperty]
        private int returnQuantity = 1;

        [ObservableProperty]
        private int currentStock;

        [ObservableProperty]
        private string currentStatus = "";

        partial void OnReturnQuantityChanged(int value)
        {
            if (value > BorrowedQuantity)
            {
                ReturnQuantity = BorrowedQuantity;
                MessageBox.Show($"Số lượng trả không được vượt quá số lượng đã mượn ({BorrowedQuantity}).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (value < 1)
            {
                ReturnQuantity = 1;
                MessageBox.Show("Số lượng trả phải lớn hơn 0.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            _ = UpdateFineAsync();
        }

        public async Task UpdateFineAsync()
        {
            var daysBorrowed = (DateTime.Now.Date - NgayMuon.ToDateTime(new TimeOnly())).Days;
            var overdueDays = daysBorrowed - QuyDinh.SoNgayMuonToiDa;

            TienPhat = overdueDays > 0
                ? overdueDays * QuyDinh.TienPhatQuaHanMoiNgay
                : 0;

            await Task.CompletedTask;

            vm.TinhTongTienPhatKyNay();
        }
    }
}

public class SelectableBook
{
    public int MaSach { get; set; }
    public string TenSach { get; set; }
    public int MaPhieuMuon { get; set; }
}