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

    public AddReaderViewModel(IDocGiaRepository docGiaRepository, ILoaiDocGiaRepository loaiDocGiaRepository)
    {
        docGiaRepo = docGiaRepository;
        loaiDocGiaRepo = loaiDocGiaRepository;
        LoadDataAsync().ConfigureAwait(false);
    }

    private async Task LoadDataAsync()
    {
        var dsLoaiDocGia = await loaiDocGiaRepo.GetAllAsync();
        if (dsLoaiDocGia == null) return;

        ReaderTypes = new ObservableCollection<LoaiDocGia>(await loaiDocGiaRepo.GetAllAsync());
    }

    [RelayCommand]
    public async Task AddReaderType()
    {
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
}
