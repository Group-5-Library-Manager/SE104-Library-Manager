using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels;

public partial class StaffViewModel(INhanVienRepository nhanVienRepo, IChucVuRepository chucVuRepo, IBoPhanRepository boPhanRepo, IBangCapRepository bangCapRepo, ITaiKhoanRepository taiKhoanRepo, IQuyDinhRepository quyDinhRepo) : ObservableObject
{
    [ObservableProperty]
    private TabItem selectedTab = null!;

    [ObservableProperty]
    private ObservableCollection<NhanVien> dsNhanVien = new ObservableCollection<NhanVien>();

    [ObservableProperty]
    private NhanVien? selectedStaff;

    [ObservableProperty]
    private NhanVien? selectedStaffForEdit;

    [ObservableProperty]
    private DateTime? selectedStaffForEditBirthday;

    [ObservableProperty]
    private TaiKhoan? selectedAccountForEdit;

    [ObservableProperty]
    private string searchStaffQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChucVu> dsChucVu = new ObservableCollection<ChucVu>();

    [ObservableProperty]
    private ChucVu? selectedPosition;

    [ObservableProperty]
    private ChucVu? selectedPositionForEdit;

    [ObservableProperty]
    private string searchPositionQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BoPhan> dsBoPhan = new ObservableCollection<BoPhan>();

    [ObservableProperty]
    private BoPhan? selectedDepartment;

    [ObservableProperty]
    private BoPhan? selectedDepartmentForEdit;

    [ObservableProperty]
    private string searchDepartmentQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BangCap> dsBangCap = new ObservableCollection<BangCap>();

    [ObservableProperty]
    private BangCap? selectedDegree;

    [ObservableProperty]
    private BangCap? selectedDegreeForEdit;

    [ObservableProperty]
    private string searchDegreeQuery = string.Empty;

    private List<NhanVien> originalNhanVien = new List<NhanVien>();
    private List<ChucVu> originalChucVu = new List<ChucVu>();
    private List<BoPhan> originalBoPhan = new List<BoPhan>();
    private List<BangCap> originalBangCap = new List<BangCap>();

    private QuyDinh quyDinh = null!;
    private bool loadDataFailed = false;

    private async Task LoadDataAsync()
    {
        if (loadDataFailed) return; 

        try
        {
            originalNhanVien = await nhanVienRepo.GetAllAsync();
            originalChucVu = await chucVuRepo.GetAllAsync();
            originalBoPhan = await boPhanRepo.GetAllAsync();
            originalBangCap = await bangCapRepo.GetAllAsync();

            DsNhanVien = new ObservableCollection<NhanVien>(originalNhanVien);
            DsChucVu = new ObservableCollection<ChucVu>(originalChucVu);
            DsBoPhan = new ObservableCollection<BoPhan>(originalBoPhan);
            DsBangCap = new ObservableCollection<BangCap>(originalBangCap);

            quyDinh = await quyDinhRepo.GetQuyDinhAsync();
        }
        catch (Exception ex)
        {
            loadDataFailed = true;
        }
    }

    [RelayCommand]
    public async Task AddStaffAsync()
    {
        var w = App.ServiceProvider?.GetService(typeof(AddStaffWindow)) as AddStaffWindow;
        if (w == null) return;
        w.Owner = App.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditStaffAsync()
    {
        if (SelectedStaffForEdit == null || SelectedStaff == null) return;

        SelectedStaffForEdit.TenNhanVien = SelectedStaffForEdit.TenNhanVien.Trim();
        SelectedStaffForEdit.DiaChi = SelectedStaffForEdit.DiaChi.Trim();
        SelectedStaffForEdit.DienThoai = SelectedStaffForEdit.DienThoai.Trim();

        try
        {
            await nhanVienRepo.UpdateAsync(SelectedStaffForEdit);
            var updatedStaff = await nhanVienRepo.GetByIdAsync(SelectedStaffForEdit.MaNhanVien);
            var index = DsNhanVien.IndexOf(SelectedStaff);
            var originalIndex = originalNhanVien.IndexOf(SelectedStaff);

            if (index >= 0 && originalIndex >= 0 && updatedStaff != null)
            {
                DsNhanVien[index] = updatedStaff;
                originalNhanVien[originalIndex] = updatedStaff;
            }

            SelectedStaff = updatedStaff;

            MessageBox.Show("Cập nhật thông tin thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task EditAccountAsync()
    {
        if (SelectedAccountForEdit == null || SelectedStaff == null) return;

        SelectedAccountForEdit.TenDangNhap = SelectedAccountForEdit.TenDangNhap.Trim();

        try
        {
            await taiKhoanRepo.UpdateUsernameAsync(SelectedAccountForEdit.MaNhanVien, SelectedAccountForEdit.TenDangNhap);

            var updatedStaff = await nhanVienRepo.GetByIdAsync(SelectedStaff.MaNhanVien);
            var index = DsNhanVien.IndexOf(SelectedStaff);
            var originalIndex = originalNhanVien.IndexOf(SelectedStaff);

            if (index >= 0 && originalIndex >= 0 && updatedStaff != null)
            {
                DsNhanVien[index] = updatedStaff;
                originalNhanVien[originalIndex] = updatedStaff;
            }

            SelectedStaff = updatedStaff;

            MessageBox.Show("Cập nhật tài khoản thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task ResetAccountPasswordAsync(Tuple<PasswordBox, PasswordBox, AddStaffWindow> pwds)
    {
        if (SelectedStaff == null || SelectedAccountForEdit == null) return;

        PasswordBox newPasswordBox = pwds.Item1;
        PasswordBox confirmPasswordBox = pwds.Item2;

        string newPassword = newPasswordBox.Password;
        string confirmPassword = confirmPasswordBox.Password;

        if (newPassword != confirmPassword)
        {
            MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn đặt lại mật khẩu cho tài khoản này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await taiKhoanRepo.UpdatePasswordAsync(SelectedAccountForEdit.MaNhanVien, newPassword);

            newPasswordBox.Password = string.Empty;
            confirmPasswordBox.Password = string.Empty;
            MessageBox.Show("Đặt lại mật khẩu thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }

    [RelayCommand]
    public async Task DeleteStaffAsync()
    {
        if (SelectedStaff == null) return;

        MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await nhanVienRepo.DeleteAsync(SelectedStaff.MaNhanVien);
            DsNhanVien.Remove(SelectedStaff);
            originalNhanVien.Remove(SelectedStaff);
            SelectedStaff = null;

            MessageBox.Show("Xóa nhân viên thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task AddPositionAsync()
    {
        if (originalChucVu.Count >= quyDinh.SoChucVuToiDa)
        {
            MessageBox.Show($"Số lượng chức vụ đã đạt giới hạn tối đa là {quyDinh.SoChucVuToiDa}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddPositionWindow)) as AddPositionWindow;
        if (w == null) return;
        w.Owner = App.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditPositionAsync()
    {
        if (SelectedPositionForEdit == null || SelectedPosition == null) return;

        SelectedPositionForEdit.TenChucVu = SelectedPositionForEdit.TenChucVu.Trim();

        try
        {
            await chucVuRepo.UpdateAsync(SelectedPositionForEdit);
            var updatedPosition = await chucVuRepo.GetByIdAsync(SelectedPositionForEdit.MaChucVu);
            var index = DsChucVu.IndexOf(SelectedPosition);
            var originalIndex = originalChucVu.IndexOf(SelectedPosition);
            if (index >= 0 && originalIndex >= 0 && updatedPosition != null)
            {
                DsChucVu[index] = updatedPosition;
                originalChucVu[originalIndex] = updatedPosition;
            }
            SelectedPosition = updatedPosition;
            MessageBox.Show("Cập nhật chức vụ thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeletePositionAsync()
    {
        if (SelectedPosition == null) return;
        MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa chức vụ này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            await chucVuRepo.DeleteAsync(SelectedPosition.MaChucVu);
            originalChucVu.Remove(SelectedPosition);
            DsChucVu.Remove(SelectedPosition);
            SelectedPosition = null;
            MessageBox.Show("Xóa chức vụ thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task AddDepartmentAsync()
    {
        if (originalBoPhan.Count >= quyDinh.SoBoPhanToiDa)
        {
            MessageBox.Show($"Số lượng bộ phận đã đạt giới hạn tối đa là {quyDinh.SoBoPhanToiDa}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddDepartmentWindow)) as AddDepartmentWindow;
        if (w == null) return;
        w.Owner = App.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditDepartmentAsync()
    {
        if (SelectedDepartmentForEdit == null || SelectedDepartment == null) return;
        SelectedDepartmentForEdit.TenBoPhan = SelectedDepartmentForEdit.TenBoPhan.Trim();
        try
        {
            await boPhanRepo.UpdateAsync(SelectedDepartmentForEdit);
            var updatedDepartment = await boPhanRepo.GetByIdAsync(SelectedDepartmentForEdit.MaBoPhan);
            var index = DsBoPhan.IndexOf(SelectedDepartment);
            var originalIndex = originalBoPhan.IndexOf(SelectedDepartment);
            if (index >= 0 && originalIndex >= 0 && updatedDepartment != null)
            {
                DsBoPhan[index] = updatedDepartment;
                originalBoPhan[originalIndex] = updatedDepartment;
            }
            SelectedDepartment = updatedDepartment;
            MessageBox.Show("Cập nhật bộ phận thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeleteDepartmentAsync()
    {
        if (SelectedDepartment == null) return;
        MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa bộ phận này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            await boPhanRepo.DeleteAsync(SelectedDepartment.MaBoPhan);
            originalBoPhan.Remove(SelectedDepartment);
            DsBoPhan.Remove(SelectedDepartment);
            SelectedDepartment = null;
            MessageBox.Show("Xóa bộ phận thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task AddDegreeAsync()
    {
        if (originalBangCap.Count >= quyDinh.SoBangCapToiDa)
        {
            MessageBox.Show($"Số lượng bằng cấp đã đạt giới hạn tối đa là {quyDinh.SoBangCapToiDa}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddDegreeWindow)) as AddDegreeWindow;
        if (w == null) return;
        w.Owner = App.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditDegreeAsync()
    {
        if (SelectedDegreeForEdit == null || SelectedDegree == null) return;
        SelectedDegreeForEdit.TenBangCap = SelectedDegreeForEdit.TenBangCap.Trim();
        try
        {
            await bangCapRepo.UpdateAsync(SelectedDegreeForEdit);
            var updatedDegree = await bangCapRepo.GetByIdAsync(SelectedDegreeForEdit.MaBangCap);
            var index = DsBangCap.IndexOf(SelectedDegree);
            var originalIndex = originalBangCap.IndexOf(SelectedDegree);
            if (index >= 0 && originalIndex >= 0 && updatedDegree != null)
            {
                DsBangCap[index] = updatedDegree;
                originalBangCap[originalIndex] = updatedDegree;
            }
            SelectedDegree = updatedDegree;
            MessageBox.Show("Cập nhật bằng cấp thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeleteDegreeAsync()
    {
        if (SelectedDegree == null) return;
        MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa bằng cấp này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            await bangCapRepo.DeleteAsync(SelectedDegree.MaBangCap);
            originalBangCap.Remove(SelectedDegree);
            DsBangCap.Remove(SelectedDegree);
            SelectedDegree = null;
            MessageBox.Show("Xóa bằng cấp thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void SearchStaff()
    {
        if (string.IsNullOrWhiteSpace(SearchStaffQuery))
        {
            DsNhanVien = new ObservableCollection<NhanVien>(originalNhanVien);
        }
        else
        {
            DsNhanVien = new ObservableCollection<NhanVien>(originalNhanVien
                .Where(nv => nv.TenNhanVien.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.DienThoai.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.DiaChi.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.MaNhanVien.ToString().Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.ChucVu.TenChucVu.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.BangCap.TenBangCap.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)
                          || nv.BoPhan.TenBoPhan.Contains(SearchStaffQuery, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [RelayCommand]
    public void SearchPosition()
    {
        if (string.IsNullOrWhiteSpace(SearchPositionQuery))
        {
            DsChucVu = new ObservableCollection<ChucVu>(originalChucVu);
        }
        else
        {
            DsChucVu = new ObservableCollection<ChucVu>(originalChucVu
                .Where(cv => cv.MaChucVu.ToString().Contains(SearchPositionQuery, StringComparison.OrdinalIgnoreCase)
                          || cv.TenChucVu.Contains(SearchPositionQuery, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [RelayCommand]
    public void SearchDepartment()
    {
        if (string.IsNullOrWhiteSpace(SearchDepartmentQuery))
        {
            DsBoPhan = new ObservableCollection<BoPhan>(originalBoPhan);
        }
        else
        {
            DsBoPhan = new ObservableCollection<BoPhan>(originalBoPhan
                .Where(bp => bp.MaBoPhan.ToString().Contains(SearchDepartmentQuery, StringComparison.OrdinalIgnoreCase)
                          || bp.TenBoPhan.Contains(SearchDepartmentQuery, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [RelayCommand]
    public void SearchDegree()
    {
        if (string.IsNullOrWhiteSpace(SearchDegreeQuery))
        {
            DsBangCap = new ObservableCollection<BangCap>(originalBangCap);
        }
        else
        {
            DsBangCap = new ObservableCollection<BangCap>(originalBangCap
                .Where(bc => bc.MaBangCap.ToString().Contains(SearchDegreeQuery, StringComparison.OrdinalIgnoreCase)
                          || bc.TenBangCap.Contains(SearchDegreeQuery, StringComparison.OrdinalIgnoreCase)));
        }
    }

    partial void OnSelectedStaffChanged(NhanVien? value)
    {
        if (value == null)
        {
            SelectedStaffForEdit = null;
            SelectedStaffForEditBirthday = null;
            SelectedAccountForEdit = null;
            return;
        }

        SelectedStaffForEdit = new NhanVien
        {
            MaNhanVien = value.MaNhanVien,
            TenNhanVien = value.TenNhanVien,
            DiaChi = value.DiaChi,
            DienThoai = value.DienThoai,
            NgaySinh = value.NgaySinh,
            MaChucVu = value.MaChucVu,
            MaBangCap = value.MaBangCap,
            MaBoPhan = value.MaBoPhan,
        };

        SelectedStaffForEditBirthday = value.NgaySinh.ToDateTime(new TimeOnly(0, 0));

        SelectedAccountForEdit = new TaiKhoan
        {
            MaNhanVien = value.MaNhanVien,
            TenDangNhap = value.TaiKhoan.TenDangNhap,
            MatKhau = string.Empty, // Password should not be shown or edited directly
            MaVaiTro = value.TaiKhoan.MaVaiTro,
            VaiTro = value.TaiKhoan.VaiTro
        };
    }
    partial void OnSearchStaffQueryChanged(string value)
    {
        SearchStaff();
    }

    partial void OnSearchPositionQueryChanged(string value)
    {
        SearchPosition();
    }

    partial void OnSearchDepartmentQueryChanged(string value)
    {
        SearchDepartment();
    }

    partial void OnSearchDegreeQueryChanged(string value)
    {
        SearchDegree();
    }

    partial void OnSelectedPositionChanged(ChucVu? value)
    {
        if (value == null)
        {
            SelectedPositionForEdit = null;
            return;
        }
        SelectedPositionForEdit = new ChucVu
        {
            MaChucVu = value.MaChucVu,
            TenChucVu = value.TenChucVu,
        };
    }

    partial void OnSelectedDepartmentChanged(BoPhan? value)
    {
        if (value == null)
        {
            SelectedDepartmentForEdit = null;
            return;
        }
        SelectedDepartmentForEdit = new BoPhan
        {
            MaBoPhan = value.MaBoPhan,
            TenBoPhan = value.TenBoPhan,
        };
    }

    partial void OnSelectedDegreeChanged(BangCap? value)
    {
        if (value == null)
        {
            SelectedDegreeForEdit = null;
            return;
        }
        SelectedDegreeForEdit = new BangCap
        {
            MaBangCap = value.MaBangCap,
            TenBangCap = value.TenBangCap,
        };
    }

    partial void OnSelectedTabChanged(TabItem value)
    {
        if (value.Header.ToString() == "Nhân viên")
        {
            LoadDataAsync().ConfigureAwait(false);
        }
    }

    partial void OnSelectedStaffForEditBirthdayChanged(DateTime? value)
    {
        if (value == null || SelectedStaffForEdit == null || SelectedStaffForEdit.NgaySinh == DateOnly.FromDateTime(value.Value))
        {
            return;
        }

        if (value.Value > DateTime.Now)
        {
            MessageBox.Show("Ngày sinh không thể lớn hơn ngày hiện tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            SelectedStaffForEditBirthday = SelectedStaffForEdit.NgaySinh.ToDateTime(new TimeOnly(0, 0));
            return;
        }

        SelectedStaffForEdit.NgaySinh = DateOnly.FromDateTime(value.Value);
    }
}
