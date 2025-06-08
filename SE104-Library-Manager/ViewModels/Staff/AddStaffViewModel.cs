using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels;

public partial class AddStaffViewModel : ObservableObject
{
    [ObservableProperty]
    private string tenNhanVien = string.Empty;

    [ObservableProperty]
    private DateTime ngaySinh = new DateTime(DateTime.Now.Year - 18, DateTime.Now.Month, DateTime.Now.Day);

    [ObservableProperty]
    private ObservableCollection<ChucVu> dsChucVu = new ObservableCollection<ChucVu>();

    [ObservableProperty]
    private ChucVu? selectedChucVu;

    [ObservableProperty]
    private ObservableCollection<BoPhan> dsBoPhan = new ObservableCollection<BoPhan>();

    [ObservableProperty]
    private BoPhan? selectedBoPhan;

    [ObservableProperty]
    private ObservableCollection<BangCap> dsBangCap = new ObservableCollection<BangCap>();

    [ObservableProperty]
    private BangCap? selectedBangCap;

    [ObservableProperty]
    private ObservableCollection<VaiTro> dsVaiTro = new ObservableCollection<VaiTro>();

    [ObservableProperty]
    private string dienThoai = string.Empty;

    [ObservableProperty]
    private string diaChi = string.Empty;

    [ObservableProperty]
    private string tenDangNhap = string.Empty;


    private INhanVienRepository nhanVienRepo;
    private IChucVuRepository chucVuRepo;
    private IBoPhanRepository boPhanRepo;
    private IBangCapRepository bangCapRepo;
    private IVaiTroRepository vaiTroRepo;

    public AddStaffViewModel(INhanVienRepository nhanVienRepository, IChucVuRepository chucVuRepository, IBoPhanRepository boPhanRepository, IBangCapRepository bangCapRepository, IVaiTroRepository vaiTroRepository)
    {
        nhanVienRepo = nhanVienRepository;
        chucVuRepo = chucVuRepository;
        boPhanRepo = boPhanRepository;
        bangCapRepo = bangCapRepository;
        vaiTroRepo = vaiTroRepository;

        LoadDataAsync().ConfigureAwait(false);
    }

    private async Task LoadDataAsync()
    {
        DsChucVu = new ObservableCollection<ChucVu>(await chucVuRepo.GetAllAsync() ?? new List<ChucVu>());
        DsBoPhan = new ObservableCollection<BoPhan>(await boPhanRepo.GetAllAsync() ?? new List<BoPhan>());
        DsBangCap = new ObservableCollection<BangCap>(await bangCapRepo.GetAllAsync() ?? new List<BangCap>());
        DsVaiTro = new ObservableCollection<VaiTro>(await vaiTroRepo.GetAllAsync() ?? new List<VaiTro>());
    }

    [RelayCommand]
    public async Task AddAsync(Tuple<PasswordBox, PasswordBox, AddStaffWindow> data)
    {

        if (SelectedChucVu == null)
        {
            MessageBox.Show("Vui lòng chọn chức vụ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedBoPhan == null)
        {
            MessageBox.Show("Vui lòng chọn bộ phận.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedBangCap == null)
        {
            MessageBox.Show("Vui lòng chọn bằng cấp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string password = data.Item1.Password.Trim();
        string confirmPassword = data.Item2.Password.Trim();
        AddStaffWindow window = data.Item3;

        if (password != confirmPassword)
        {
            MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var newStaff = new NhanVien
        {
            TenNhanVien = TenNhanVien.Trim(),
            NgaySinh = DateOnly.FromDateTime(NgaySinh.Date),
            MaChucVu = SelectedChucVu.MaChucVu,
            MaBoPhan = SelectedBoPhan.MaBoPhan,
            MaBangCap = SelectedBangCap.MaBangCap,
            DienThoai = DienThoai.Trim(),
            DiaChi = DiaChi.Trim(),
        };

        var vaiTro = DsVaiTro
            .FirstOrDefault(vt => vt.TenVaiTro == "Thủ thư");

        if (vaiTro == null)
        {
            vaiTro = dsVaiTro.Last();
        }

        var account = new TaiKhoan
        {
            MaNhanVien = 0, // Update this after adding the staff
            TenDangNhap = TenDangNhap.Trim(),
            MatKhau = password,
            MaVaiTro = vaiTro.MaVaiTro
        };

        try
        {
            await nhanVienRepo.AddAsync(newStaff, account);

            MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void Cancel(AddStaffWindow window)
    {
        window.Close();
    }
}