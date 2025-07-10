using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views.Return;
using SE104_Library_Manager.ViewModels.Return;
using System.Collections.ObjectModel;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class UpdateReturnReceiptViewModel : ObservableObject
{
    [ObservableProperty] private QuyDinh? quyDinh;
    [ObservableProperty] private ObservableCollection<CopyReturnItem> dsBanSaoTra = new();
    [ObservableProperty] private PhieuTra? selectedReturn;
    [ObservableProperty] private int tienPhatKyNay;
    [ObservableProperty] private int tongNoHienTai;
    private int tongNoBanDau;

    private List<SelectableCopy> allSelectableCopies = new();

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

        var banSaoDangMuon = await phieuTraRepo.GetBanSaoDangMuonByDocGiaAsync(phieuTra.MaDocGia);

        // Build AllSelectableCopies
        allSelectableCopies = banSaoDangMuon
            .Select(ct => new SelectableCopy
            {
                MaPhieuMuon = ct.MaPhieuMuon,
                MaBanSao = ct.MaBanSao,
                TenSach = ct.BanSaoSach.Sach.TenSach,
                MaSach = ct.BanSaoSach.MaSach,
                TinhTrang = ct.BanSaoSach.TinhTrang,
                NgayMuon = ct.PhieuMuon?.NgayMuon ?? DateOnly.MinValue
            })
            .Concat(phieuTra.DsChiTietPhieuTra.Select(ct => new SelectableCopy
            {
                MaPhieuMuon = ct.MaPhieuMuon,
                MaBanSao = ct.MaBanSao,
                TenSach = ct.BanSaoSach.Sach.TenSach,
                MaSach = ct.BanSaoSach.MaSach,
                TinhTrang = ct.BanSaoSach.TinhTrang,
                NgayMuon = ct.PhieuMuon?.NgayMuon ?? DateOnly.MinValue
            }))
            .GroupBy(c => new { c.MaBanSao, c.MaPhieuMuon })
            .Select(g => g.First())
            .ToList();

        QuyDinh = await quyDinhRepo.GetQuyDinhAsync();

        DsBanSaoTra.Clear();
        foreach (var ct in phieuTra.DsChiTietPhieuTra)
        {
            var item = new CopyReturnItem(this, QuyDinh!)
            {
                MaPhieuMuon = ct.MaPhieuMuon,
                MaBanSao = ct.MaBanSao,
                TenSach = ct.BanSaoSach.Sach.TenSach,
                NgayMuon = ct.PhieuMuon?.NgayMuon ?? DateOnly.MinValue,
                TienPhat = ct.TienPhat,
                SelectedCopy = allSelectableCopies
                    .FirstOrDefault(c => c.MaBanSao == ct.MaBanSao && c.MaPhieuMuon == ct.MaPhieuMuon)
            };
            DsBanSaoTra.Add(item);
        }

        UpdateAllAvailableCopies();
        TinhTongTienPhatKyNay();
        CapNhatTongNoHienTai();
    }

    [RelayCommand]
    public void RemoveCopy(CopyReturnItem? item)
    {
        if (item == null) return;

        DsBanSaoTra.Remove(item);
        UpdateAllAvailableCopies();
        TinhTongTienPhatKyNay();
    }

    [RelayCommand]
    public void AddCopy()
    {
        if (SelectedReturn == null || QuyDinh == null) return;

        var available = CreateAvailableCopiesList();
        if (!available.Any())
        {
            MessageBox.Show("Không còn bản sao nào để chọn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var item = new CopyReturnItem(this, QuyDinh);
        DsBanSaoTra.Add(item);
        UpdateAllAvailableCopies();
    }

    private void UpdateAllAvailableCopies()
    {
        var selectedIds = DsBanSaoTra
            .Where(i => i.SelectedCopy != null)
            .Select(i => (i.SelectedCopy!.MaBanSao, i.SelectedCopy.MaPhieuMuon))
            .ToList();

        foreach (var item in DsBanSaoTra)
        {
            var current = item.SelectedCopy;

            var list = allSelectableCopies
                .Where(c => !selectedIds.Contains((c.MaBanSao, c.MaPhieuMuon))
                    || (current != null && c.MaBanSao == current.MaBanSao && c.MaPhieuMuon == current.MaPhieuMuon))
                .OrderBy(c => c.TenSach)
                .ToList();

            SyncAvailableCopies(item, list);
        }
    }

    private List<SelectableCopy> CreateAvailableCopiesList(CopyReturnItem? excludeItem = null)
    {
        var selected = DsBanSaoTra
            .Where(i => i.SelectedCopy != null && i != excludeItem)
            .Select(i => (i.SelectedCopy!.MaBanSao, i.SelectedCopy.MaPhieuMuon))
            .ToList();

        return allSelectableCopies
            .Where(c => !selected.Contains((c.MaBanSao, c.MaPhieuMuon)))
            .ToList();
    }

    private void TinhTongTienPhatKyNay()
    {
        TienPhatKyNay = DsBanSaoTra.Sum(s => s.TienPhat);
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
        if (SelectedReturn == null || DsBanSaoTra.Count == 0)
        {
            MessageBox.Show("Thông tin chưa đầy đủ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
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

        var listCT = DsBanSaoTra
            .Where(s => s.SelectedCopy != null && s.MaPhieuMuon > 0)
            .Select(s => new ChiTietPhieuTraInfo
            {
                MaBanSao = s.SelectedCopy!.MaBanSao,
                MaPhieuMuon = s.MaPhieuMuon,
                TienPhat = s.TienPhat
            }).ToList();

        var tienPhatMoi = listCT.Sum(ct => ct.TienPhat);
        if (docGia != null)
        {
            docGia.TongNo += tienPhatMoi;
            await docGiaRepo.UpdateAsync(docGia);
        }

        SelectedReturn.TienPhatKyNay = tienPhatMoi;
        SelectedReturn.TongNo = TongNoHienTai;

        await phieuTraRepo.UpdateAsync(SelectedReturn, listCT);

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

    private void SyncAvailableCopies(CopyReturnItem item, List<SelectableCopy> newList)
    {
        var selectedMaBanSao = item.SelectedCopy?.MaBanSao;
        var selectedMaPhieuMuon = item.SelectedCopy?.MaPhieuMuon;

        for (int i = item.AvailableCopies.Count - 1; i >= 0; i--)
        {
            if (!newList.Any(c => c.MaBanSao == item.AvailableCopies[i].MaBanSao && c.MaPhieuMuon == item.AvailableCopies[i].MaPhieuMuon))
            {
                item.AvailableCopies.RemoveAt(i);
            }
        }

        foreach (var copy in newList)
        {
            if (!item.AvailableCopies.Any(c => c.MaBanSao == copy.MaBanSao && c.MaPhieuMuon == copy.MaPhieuMuon))
            {
                item.AvailableCopies.Add(copy);
            }
        }

        if (selectedMaBanSao != null && !item.AvailableCopies.Any(c => c.MaBanSao == selectedMaBanSao && c.MaPhieuMuon == selectedMaPhieuMuon))
        {
            item.SelectedCopy = null;
        }
    }

    public partial class CopyReturnItem : ObservableObject
    {
        private readonly UpdateReturnReceiptViewModel vm;

        public CopyReturnItem(UpdateReturnReceiptViewModel vm, QuyDinh quyDinh)
        {
            this.vm = vm;
            this.QuyDinh = quyDinh;
            AvailableCopies = new ObservableCollection<SelectableCopy>();
        }

        public ObservableCollection<SelectableCopy> AvailableCopies { get; }
        public QuyDinh QuyDinh { get; }

        [ObservableProperty]
        private SelectableCopy? selectedCopy;

        partial void OnSelectedCopyChanged(SelectableCopy? value)
        {
            if (value == null) return;

            MaBanSao = value.MaBanSao;
            TenSach = value.TenSach;
            MaPhieuMuon = value.MaPhieuMuon;

            var chiTiet = vm.SelectedReturn?.DsChiTietPhieuTra
                .FirstOrDefault(ct => ct.MaPhieuMuon == MaPhieuMuon && ct.MaBanSao == MaBanSao);
            if (chiTiet?.PhieuMuon != null)
            {
                NgayMuon = chiTiet.PhieuMuon.NgayMuon;
            }
            else
            {
                NgayMuon = value.NgayMuon;
            }

            _ = UpdateFineAsync();
            vm.UpdateAllAvailableCopies();
        }

        public int MaBanSao { get; set; }
        public string TenSach { get; set; } = "";
        public int MaPhieuMuon { get; set; }

        [ObservableProperty]
        private DateOnly ngayMuon;

        [ObservableProperty]
        private int tienPhat;

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

public class SelectableCopy
{
    public int MaBanSao { get; set; }
    public string TenSach { get; set; }
    public int MaPhieuMuon { get; set; }
    public int MaSach { get; set; }
    public string TinhTrang { get; set; } = string.Empty;
    public DateOnly NgayMuon { get; set; } = DateOnly.MinValue;
}