using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace SE104_Library_Manager.ViewModels;

public partial class AddReaderViewModel : ObservableObject
{
    [ObservableProperty]
    private string todayDate = DateTime.Now.ToString("dd/MM/yyyy");

    [ObservableProperty]
    private string readerName = string.Empty;

    [ObservableProperty]
    private DateTime birthDate = new DateTime(DateTime.Now.Year - 18, DateTime.Now.Month, DateTime.Now.Day);

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LoaiDocGia> readerTypes = new ObservableCollection<LoaiDocGia>();

    [ObservableProperty]
    private LoaiDocGia? selectedReaderType;

    private IDocGiaRepository docGiaRepo;
    private ILoaiDocGiaRepository loaiDocGiaRepo;
    private IQuyDinhRepository quyDinhRepo;

    private QuyDinh quyDinh = null!;

    public AddReaderViewModel(IDocGiaRepository docGiaRepository, ILoaiDocGiaRepository loaiDocGiaRepository, IQuyDinhRepository quyDinhRepository)
    {
        docGiaRepo = docGiaRepository;
        loaiDocGiaRepo = loaiDocGiaRepository;
        quyDinhRepo = quyDinhRepository;
        LoadDataAsync().ConfigureAwait(false);
    }

    private async Task LoadDataAsync()
    {
        var dsLoaiDocGia = await loaiDocGiaRepo.GetAllAsync();
        if (dsLoaiDocGia == null) return;

        ReaderTypes = new ObservableCollection<LoaiDocGia>(await loaiDocGiaRepo.GetAllAsync());

        quyDinh = await quyDinhRepo.GetQuyDinhAsync();
    }

    [RelayCommand]
    public async Task AddReaderType()
    {
        if (ReaderTypes.Count >= quyDinh.SoLoaiDocGiaToiDa)
        {
            MessageBox.Show($"Số lượng loại độc giả đã đạt giới hạn tối đa là {quyDinh.SoLoaiDocGiaToiDa}.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var w = App.ServiceProvider?.GetService(typeof(AddReaderTypeWindow)) as AddReaderTypeWindow;
        if (w == null) return;

        w.Owner = App.Current.MainWindow;
        w.ShowDialog();

        await LoadDataAsync();
        SelectedReaderType = null;
    }

    [RelayCommand]
    public async Task AddAsync(AddReaderWindow w)
    {
        if (SelectedReaderType == null)
        {
            MessageBox.Show("Vui lòng chọn loại độc giả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var reader = new DocGia
        {
            TenDocGia = ReaderName.Trim(),
            DiaChi = Address.Trim(),
            Email = Email.Trim(),
            MaLoaiDocGia = SelectedReaderType.MaLoaiDocGia,
            NgaySinh = DateOnly.FromDateTime(BirthDate.Date),
            NgayLapThe = DateOnly.FromDateTime(DateTime.Now)
        };

        try
        {
            await docGiaRepo.AddAsync(reader);

            MessageBox.Show("Thêm độc giả thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            w.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    [RelayCommand]
    public void Cancel(AddReaderWindow w)
    { 
        w.Close();
    }

    partial void OnBirthDateChanged(DateTime value)
    {
        int minAge = quyDinh.TuoiDocGiaToiThieu;
        int maxAge = quyDinh.TuoiDocGiaToiDa;

        // Birthday must in the past
        if (value > DateTime.Now)
        {
            MessageBox.Show("Ngày sinh không thể lớn hơn ngày hiện tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            BirthDate = new DateTime(DateTime.Now.Year - minAge, DateTime.Now.Month, DateTime.Now.Day);
            return;
        }


        int currentYear = DateTime.Now.Year;
        int selectedYear = value.Year;
        int readerAge = currentYear - selectedYear;

        // If not yet had birthday this year, subtract one from age
        if (DateTime.Now < new DateTime(currentYear, value.Month, value.Day))
        {
            readerAge--;
        }

        if (readerAge < minAge || readerAge > maxAge)
        {
            MessageBox.Show($"Độc giả phải từ {minAge} đến {maxAge} tuổi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            BirthDate = new DateTime(currentYear - minAge, DateTime.Now.Month, DateTime.Now.Day);
            return;
        }
    }
}
