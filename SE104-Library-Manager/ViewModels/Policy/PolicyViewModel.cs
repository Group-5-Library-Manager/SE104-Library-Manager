using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SE104_Library_Manager.ViewModels.Policy;
public partial class PolicyViewModel : ObservableObject
{
    private readonly IQuyDinhRepository quyDinhRepo;
    private readonly IDocGiaRepository docGiaRepo;
    private readonly IPhieuMuonRepository phieuMuonRepo;
    private readonly ILoaiDocGiaRepository loaiDocGiaRepo;
    private readonly ISachRepository sachRepo;
    private readonly INhanVienRepository nhanVienRepo;
    private readonly IChucVuRepository chucVuRepo;
    private readonly IBoPhanRepository boPhanRepo;
    private readonly IBangCapRepository bangCapRepo;
    private readonly ITheLoaiRepository theLoaiRepo;
    private readonly ITacGiaRepository tacGiaRepo;
    private readonly IVaiTroRepository vaiTroRepo;

    public PolicyViewModel(
        IQuyDinhRepository quyDinhRepo,
        IDocGiaRepository docGiaRepo,
        IPhieuMuonRepository phieuMuonRepo,
        ILoaiDocGiaRepository loaiDocGiaRepo,
        ISachRepository sachRepo,
        INhanVienRepository nhanVienRepo,
        IChucVuRepository chucVuRepo,
        IBoPhanRepository boPhanRepo,
        IBangCapRepository bangCapRepo,
        ITheLoaiRepository theLoaiRepo,
        ITacGiaRepository tacGiaRepo,
        IVaiTroRepository vaiTroRepo
    )
    {
        this.quyDinhRepo = quyDinhRepo;
        this.docGiaRepo = docGiaRepo;
        this.phieuMuonRepo = phieuMuonRepo;
        this.loaiDocGiaRepo = loaiDocGiaRepo;
        this.sachRepo = sachRepo;
        this.nhanVienRepo = nhanVienRepo;
        this.chucVuRepo = chucVuRepo;
        this.boPhanRepo = boPhanRepo;
        this.bangCapRepo = bangCapRepo;
        this.theLoaiRepo = theLoaiRepo;
        this.tacGiaRepo = tacGiaRepo;
        this.vaiTroRepo = vaiTroRepo;
        LoadDataAsync();
    }

    [ObservableProperty] private QuyDinh currentQuyDinh = new();

    public async void LoadDataAsync()
    {
        CurrentQuyDinh = await quyDinhRepo.GetQuyDinhAsync();
    }

    [RelayCommand]
    public async Task Save(UserControl uc)
    {
        if (HasValidationError(uc))
        {
            MessageBox.Show("Vui lòng sửa tất cả ô nhập còn lỗi (viền đỏ) thành dạng số và không được để trống.",
                "Lỗi nhập liệu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var dsDocGia = await docGiaRepo.GetAllAsync();
        var dsPhieuMuon = await phieuMuonRepo.GetAllAsync();
        var dsLoaiDocGia = await loaiDocGiaRepo.GetAllAsync();
        var dsSach = await sachRepo.GetAllAsync();
        var dsNhanVien = await nhanVienRepo.GetAllAsync();
        var dsChucVu = await chucVuRepo.GetAllAsync();
        var dsBoPhan = await boPhanRepo.GetAllAsync();
        var dsBangCap = await bangCapRepo.GetAllAsync();
        var dsTheLoai = await theLoaiRepo.GetAllAsync();
        var dsTacGia = await tacGiaRepo.GetAllAsync();
        var dsVaiTro = await vaiTroRepo.GetAllAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Validate tuổi độc giả
        var tuoiMin = CurrentQuyDinh.TuoiDocGiaToiThieu;
        var tuoiMax = CurrentQuyDinh.TuoiDocGiaToiDa;
        var docGiaViPham = dsDocGia.FirstOrDefault(dg =>
        {
            var tuoi = today.Year - dg.NgaySinh.Year - (today.DayOfYear < dg.NgaySinh.DayOfYear ? 1 : 0);
            return tuoi < tuoiMin || tuoi > tuoiMax;
        });

        if (docGiaViPham != null)
        {
            MessageBox.Show($"Có độc giả không thỏa mãn quy định tuổi ({tuoiMin}-{tuoiMax}): {docGiaViPham.TenDocGia} ({docGiaViPham.MaDocGia})",
                "Lỗi quy định",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số sách mượn tối đa cho từng độc giả
        foreach (var docGia in dsDocGia)
        {
            // Lấy tất cả các phiếu mượn chưa bị xóa của độc giả 
            var phieuMuons = docGia.DsPhieuMuon.Where(pm => !pm.DaXoa);  

            // Lấy tất cả các bản sao sách đã mượn
            var allBorrowed = phieuMuons.SelectMany(pm => pm.DsChiTietPhieuMuon).Select(ct => new { ct.MaBanSao, ct.MaPhieuMuon });

            // Lấy tất cả các bản sao sách đã trả (theo MaPhieuMuon và MaBanSao)
            var allReturned = phieuMuons
                .SelectMany(pm => pm.DsChiTietPhieuTra)
                .Select(ctpt => new { ctpt.MaBanSao, ctpt.MaPhieuMuon });

            // Sách đang mượn <- những sách đã mượn nhưng chưa trả
            var soSachDangMuon = allBorrowed
                .Where(b => !allReturned.Any(r => r.MaBanSao == b.MaBanSao && r.MaPhieuMuon == b.MaPhieuMuon))
                .Count();

            if (soSachDangMuon > CurrentQuyDinh.SoSachMuonToiDa)
            {
                MessageBox.Show($"Độc giả {docGia.TenDocGia} (Mã: DG{docGia.MaDocGia}) đang mượn {soSachDangMuon} sách, vượt quá số sách mượn tối đa mới ({CurrentQuyDinh.SoSachMuonToiDa}).",
                    "Lỗi quy định",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LoadDataAsync();
                return;
            }
        }

        // Validate số năm xuất bản tối đa 
        if (dsSach.Any(s => (today.Year - s.NamXuatBan) > CurrentQuyDinh.SoNamXuatBanToiDa))
        {
            MessageBox.Show("Có sách đã xuất bản quá số năm tối đa mới.", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số loại độc giả tối đa
        if (dsLoaiDocGia.Count > CurrentQuyDinh.SoLoaiDocGiaToiDa)
        {
            MessageBox.Show($"Số loại độc giả hiện tại ({dsLoaiDocGia.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoLoaiDocGiaToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số thể loại tối đa
        if (dsTheLoai.Count > CurrentQuyDinh.SoTheLoaiToiDa)
        {
            MessageBox.Show($"Số thể loại hiện tại ({dsTheLoai.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoTheLoaiToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số tác giả tối đa
        if (dsTacGia.Count > CurrentQuyDinh.SoTacGiaToiDa)
        {
            MessageBox.Show($"Số tác giả hiện tại ({dsTacGia.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoTacGiaToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số bộ phận tối đa
        if (dsBoPhan.Count > CurrentQuyDinh.SoBoPhanToiDa)
        {
            MessageBox.Show($"Số bộ phận hiện tại ({dsBoPhan.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoBoPhanToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số bằng cấp tối đa
        if (dsBangCap.Count > CurrentQuyDinh.SoBangCapToiDa)
        {
            MessageBox.Show($"Số bằng cấp hiện tại ({dsBangCap.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoBangCapToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        // Validate số chức vụ tối đa
        if (dsChucVu.Count > CurrentQuyDinh.SoChucVuToiDa)
        {
            MessageBox.Show($"Số chức vụ hiện tại ({dsChucVu.Count}) vượt quá quy định mới ({CurrentQuyDinh.SoChucVuToiDa}).", "Lỗi quy định", MessageBoxButton.OK, MessageBoxImage.Error);
            LoadDataAsync();
            return;
        }

        try
        {
            await quyDinhRepo.UpdateAsync(CurrentQuyDinh);
            MessageBox.Show("Cập nhật quy định thành công!",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi cập nhật quy định: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    public static bool HasValidationError(DependencyObject parent)
    {
        if (Validation.GetHasError(parent))
            return true;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (HasValidationError(child))
                return true;
        }
        return false;
    }
}
