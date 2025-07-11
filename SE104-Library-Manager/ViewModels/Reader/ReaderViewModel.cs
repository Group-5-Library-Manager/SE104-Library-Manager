using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels;

public partial class ReaderViewModel(IDocGiaRepository docGiaRepo, ILoaiDocGiaRepository loaiDocGiaRepo, IQuyDinhRepository quyDinhRepo) : ObservableObject
{
    [ObservableProperty]
    private TabItem selectedTab = null!;

    [ObservableProperty]
    private ObservableCollection<DocGia> dsDocGia = new ObservableCollection<DocGia>();

    [ObservableProperty]
    private DocGia? selectedReader;

    [ObservableProperty]
    private DocGia? selectedReaderForEdit;

    [ObservableProperty]
    private DateTime? selectedReaderForEditBirthday;

    [ObservableProperty]
    private LoaiDocGia? selectedReaderType;

    [ObservableProperty]
    private LoaiDocGia? selectedReaderTypeForEdit;

    [ObservableProperty]
    private ObservableCollection<LoaiDocGia> dsLoaiDocGia = new ObservableCollection<LoaiDocGia>();

    [ObservableProperty]
    private string searchReaderQuery = string.Empty;

    [ObservableProperty]
    private string searchReaderTypeQuery = string.Empty;

    [ObservableProperty]
    private QuyDinh quyDinhHienTai = null!;

    private List<DocGia> originalDsDocGia = new List<DocGia>();
    private List<LoaiDocGia> originalDsLoaiDocGia = new List<LoaiDocGia>();

    private async Task LoadDataAsync()
    {
        originalDsDocGia = await docGiaRepo.GetAllAsync();
        originalDsLoaiDocGia = await loaiDocGiaRepo.GetAllAsync();

        if (originalDsDocGia == null || originalDsLoaiDocGia == null) return;
        
        DsDocGia = new ObservableCollection<DocGia>(originalDsDocGia);

        DsLoaiDocGia = new ObservableCollection<LoaiDocGia>(originalDsLoaiDocGia);

        QuyDinhHienTai = await quyDinhRepo.GetQuyDinhAsync();

        SearchReaderQuery = string.Empty;
        SelectedReader = null;
    }

    [RelayCommand]
    public async Task AddReader()
    {
        var w = App.ServiceProvider?.GetService(typeof(AddReaderWindow)) as AddReaderWindow;
        if (w == null) return;
        w.Owner = Application.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditReader() 
    {
        if (SelectedReaderForEdit == null || SelectedReader == null) return;

        SelectedReaderForEdit.TenDocGia = SelectedReaderForEdit.TenDocGia.Trim();
        SelectedReaderForEdit.DiaChi = SelectedReaderForEdit.DiaChi.Trim();
        SelectedReaderForEdit.Email = SelectedReaderForEdit.Email.Trim();

        try
        {
            await docGiaRepo.UpdateAsync(SelectedReaderForEdit);
            var index = DsDocGia.IndexOf(SelectedReader);
            var originalIndex = originalDsDocGia.IndexOf(SelectedReader);
            var updatedReader = await docGiaRepo.GetByIdAsync(SelectedReaderForEdit.MaDocGia);

            if (index >= 0 && originalIndex >= 0&& updatedReader != null)
            {
                DsDocGia[index] = updatedReader;
                originalDsDocGia[originalIndex] = updatedReader;
            }

            SelectedReader = updatedReader;

            MessageBox.Show("Cập nhật thông tin thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeleteReader()
    {
        if (SelectedReaderForEdit == null || SelectedReader == null) return;

        try
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa độc giả này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            await docGiaRepo.DeleteAsync(SelectedReader.MaDocGia);
            DsDocGia.Remove(SelectedReader);
            originalDsDocGia.Remove(SelectedReader);
            SelectedReader = null;

            MessageBox.Show("Xóa độc giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task RenewReader()
    {
        if (SelectedReader == null || SelectedReaderForEdit == null) return;

        MessageBoxResult options = MessageBox.Show("Bạn có chắc chắn muốn gia hạn thẻ độc giả này? (Các thay đổi chưa lưu sẽ mất)", "Xác nhận gia hạn", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (options != MessageBoxResult.Yes) return;


        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            SelectedReader.NgayLapThe = today;

            await docGiaRepo.UpdateAsync(SelectedReader);
            var index = DsDocGia.IndexOf(SelectedReader);
            var originalIndex = originalDsDocGia.IndexOf(SelectedReader);
            var updatedReader = await docGiaRepo.GetByIdAsync(SelectedReader.MaDocGia);

            if (index >= 0 && originalIndex >= 0 && updatedReader != null)
            {
                DsDocGia[index] = updatedReader;
                originalDsDocGia[originalIndex] = updatedReader;
            }

            SelectedReader = updatedReader;

            MessageBox.Show("Gia hạn thẻ độc giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void SearchReaders()
    {
        if (SearchReaderQuery == null || SearchReaderQuery.Trim() == string.Empty)
        {
            DsDocGia = new ObservableCollection<DocGia>(originalDsDocGia);
            return;
        }

        var filteredReaders = originalDsDocGia
            .Where(r => r.TenDocGia.Contains(SearchReaderQuery, StringComparison.OrdinalIgnoreCase) ||
                        r.Email.Contains(SearchReaderQuery, StringComparison.OrdinalIgnoreCase) ||
                        r.DiaChi.Contains(SearchReaderQuery, StringComparison.OrdinalIgnoreCase) ||
                        r.LoaiDocGia.TenLoaiDocGia.Contains(SearchReaderQuery, StringComparison.OrdinalIgnoreCase) ||
                        r.MaLoaiDocGia.ToString().Contains(SearchReaderQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        DsDocGia = new ObservableCollection<DocGia>(filteredReaders);
    }

    [RelayCommand]
    public async Task AddReaderType()
    {
        if (originalDsLoaiDocGia != null && originalDsLoaiDocGia.Count >= QuyDinhHienTai.SoLoaiDocGiaToiDa)
        {
            MessageBox.Show($"Số lượng loại độc giả đã đạt giới hạn tối đa là {QuyDinhHienTai.SoLoaiDocGiaToiDa}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddReaderTypeWindow)) as AddReaderTypeWindow;
        if (w == null) return;
        w.Owner = Application.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task EditReaderType()
    {
        if (SelectedReaderType == null || SelectedReaderTypeForEdit == null) return;

        SelectedReaderTypeForEdit.TenLoaiDocGia = SelectedReaderTypeForEdit.TenLoaiDocGia.Trim();

        try
        {
            await loaiDocGiaRepo.UpdateAsync(SelectedReaderTypeForEdit);
            var index = DsLoaiDocGia.IndexOf(SelectedReaderType);
            var originalIndex = originalDsLoaiDocGia.IndexOf(SelectedReaderType);
            var updatedReaderType = await loaiDocGiaRepo.GetByIdAsync(SelectedReaderTypeForEdit.MaLoaiDocGia);

            if (index >= 0 && originalIndex >= 0 && updatedReaderType != null)
            {
                DsLoaiDocGia[index] = updatedReaderType;
                originalDsLoaiDocGia[originalIndex] = updatedReaderType;
            }

            SelectedReaderType = updatedReaderType;

            MessageBox.Show("Cập nhật thông tin loại độc giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public async Task DeleteReaderType()
    {
        if (SelectedReaderType == null || SelectedReaderTypeForEdit == null) return;

        try
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa loại độc giả này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            await loaiDocGiaRepo.DeleteAsync(SelectedReaderType.MaLoaiDocGia);
            originalDsLoaiDocGia.Remove(SelectedReaderType);
            DsLoaiDocGia.Remove(SelectedReaderType);
            SelectedReaderType = null;
            
            MessageBox.Show("Xóa loại độc giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void SearchReaderTypes()
    {
        if (SearchReaderTypeQuery == null || SearchReaderTypeQuery.Trim() == string.Empty)
        {
            DsLoaiDocGia = new ObservableCollection<LoaiDocGia>(originalDsLoaiDocGia);
            return;
        }

        var filteredReaderTypes = originalDsLoaiDocGia
            .Where(rt => rt.TenLoaiDocGia.Contains(SearchReaderTypeQuery, StringComparison.OrdinalIgnoreCase) ||
                         rt.MaLoaiDocGia.ToString().Contains(SearchReaderTypeQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        DsLoaiDocGia = new ObservableCollection<LoaiDocGia>(filteredReaderTypes);
    }

    partial void OnSelectedReaderChanged(DocGia? value)
    {
        if (value == null)
        {
            SelectedReaderForEdit = null;
            SelectedReaderForEditBirthday = null;
        }
        else
        {
            SelectedReaderForEdit = new DocGia 
            {
                MaDocGia = value.MaDocGia,
                TenDocGia = value.TenDocGia,
                DiaChi = value.DiaChi,
                Email = value.Email,
                MaLoaiDocGia = value.MaLoaiDocGia,
                NgaySinh = value.NgaySinh,
                NgayLapThe = value.NgayLapThe,
                TongNo = value.TongNo
            };

            SelectedReaderForEditBirthday = value.NgaySinh.ToDateTime(TimeOnly.MinValue);
        }
    }

    partial void OnSelectedReaderTypeChanged(LoaiDocGia? value)
    {
        if (value == null)
        {
            SelectedReaderTypeForEdit = null;
        }
        else
        {
            SelectedReaderTypeForEdit = new LoaiDocGia 
            {
                MaLoaiDocGia = value.MaLoaiDocGia,
                TenLoaiDocGia = value.TenLoaiDocGia
            };
        }
    }

    partial void OnSearchReaderQueryChanged(string value)
    {
        SearchReaders();
    }

    partial void OnSearchReaderTypeQueryChanged(string value)
    {
        SearchReaderTypes();
    }

    partial void OnSelectedTabChanged(TabItem value)
    {
        if (value.Header.ToString() == "Độc giả")
        {
            LoadDataAsync().ConfigureAwait(false);
        }
    }

    partial void OnSelectedReaderForEditBirthdayChanged(DateTime? value)
    {
        if (value == null || SelectedReaderForEdit == null || DateOnly.FromDateTime(value.Value) == SelectedReaderForEdit.NgaySinh) return;

        int minAge = quyDinhHienTai.TuoiDocGiaToiThieu;
        int maxAge = quyDinhHienTai.TuoiDocGiaToiDa;

        if (value > DateTime.Now)
        {
            MessageBox.Show("Ngày sinh không được lớn hơn ngày hiện tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            SelectedReaderForEditBirthday = SelectedReaderForEdit.NgaySinh.ToDateTime(TimeOnly.MinValue); // Reset to old value
            return;
        }

        int currentYear = DateTime.Now.Year;
        int selectedYear = value.Value.Year;
        int currentAge = currentYear - selectedYear;

        // If not yet had birthday this year, subtract one from age
        if (DateTime.Now < new DateTime(currentYear, value.Value.Month, value.Value.Day))
        {
            currentAge--;
        }

        if (currentAge < minAge || currentAge > maxAge)
        {
            MessageBox.Show($"Độc giả phải từ {minAge} đến {maxAge} tuổi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            SelectedReaderForEditBirthday = SelectedReaderForEdit.NgaySinh.ToDateTime(TimeOnly.MinValue); // Reset to old value
            return;
        }

        // Update the birthday in SelectedReaderForEdit
        SelectedReaderForEdit.NgaySinh = DateOnly.FromDateTime(value.Value);
    }
}