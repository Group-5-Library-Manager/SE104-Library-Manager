using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Views;
using SE104_Library_Manager.Views.Book;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Book
{
    public partial class AddBookViewModel : ObservableObject
    {
        [ObservableProperty]
        private string todayDate = DateTime.Now.ToString("dd/MM/yyyy");

        [ObservableProperty]
        private string bookName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TacGia> authors = new ObservableCollection<TacGia>();

        [ObservableProperty]
        private TacGia? selectedAuthor;

        [ObservableProperty]
        private ObservableCollection<TheLoai> genres = new ObservableCollection<TheLoai>();

        [ObservableProperty]
        private TheLoai? selectedGenre;

        [ObservableProperty]
        private int price = 0;

        [ObservableProperty]
        private ObservableCollection<NhaXuatBan> publishers = new ObservableCollection<NhaXuatBan>();

        [ObservableProperty]
        private NhaXuatBan? selectedPublisher;

        [ObservableProperty]
        private int publishYear = DateTime.Now.Year;

        private ISachRepository sachRepo;
        private ITheLoaiRepository theLoaiRepo;
        private ITacGiaRepository tacGiaRepo;
        private INhaXuatBanRepository nhaXuatBanRepo;
        public AddBookViewModel(ISachRepository sachRepo, ITheLoaiRepository theLoaiRepo, ITacGiaRepository tacGiaRepo, INhaXuatBanRepository nhaXuatBanrRepo)
        {
            this.sachRepo = sachRepo;
            this.theLoaiRepo = theLoaiRepo;
            this.tacGiaRepo = tacGiaRepo;
            this.nhaXuatBanRepo = nhaXuatBanrRepo;
            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            var dsTheLoai = await theLoaiRepo.GetAllAsync();
            if (dsTheLoai == null) return;

            Genres = new ObservableCollection<TheLoai>(await theLoaiRepo.GetAllAsync());

            var dsTacGia = await tacGiaRepo.GetAllAsync();
            if (dsTacGia == null) return;

            Authors = new ObservableCollection<TacGia>(await tacGiaRepo.GetAllAsync());

            var dsNXB = await nhaXuatBanRepo.GetAllAsync();
            if (dsNXB == null) return;

            Publishers = new ObservableCollection<NhaXuatBan>(await nhaXuatBanRepo.GetAllAsync());
        }

        [RelayCommand]
        public async Task AddAuthor()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddAuthorWindow)) as AddAuthorWindow;
            if (w == null) return;

            w.Owner = App.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
            SelectedAuthor = null;
        }

        [RelayCommand]
        public async Task AddGenre()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddGenreWindow)) as AddGenreWindow;
            if (w == null) return;

            w.Owner = App.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
            SelectedGenre = null;
        }

        [RelayCommand]
        public async Task AddPublisher()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddPublisherWindow)) as AddPublisherWindow;
            if (w == null) return;

            w.Owner = App.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
            SelectedPublisher = null;
        }

        [RelayCommand]
        public async Task AddAsync(AddBookWindow w)
        {
            if (SelectedAuthor == null)
            {
                MessageBox.Show("Vui lòng chọn tác giả.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedGenre == null)
            {
                MessageBox.Show("Vui lòng chọn thể loại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedPublisher == null)
            {
                MessageBox.Show("Vui lòng chọn nhà xuất bản.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var book = new Sach
            {
                TenSach = BookName.Trim(),
                MaTheLoai = SelectedGenre.MaTheLoai,
                MaTacGia = SelectedAuthor.MaTacGia,
                MaNhaXuatBan = SelectedPublisher.MaNhaXuatBan,
                NamXuatBan = PublishYear,
                NgayNhap = DateOnly.FromDateTime(DateTime.Now),
                TriGia = Price,
                SoLuongHienCo = 1, // Initial quantity
                SoLuongTong = 1,   // Initial total quantity
                TrangThai = "Còn sách" // Temporary value to satisfy required constraint
            };

            // Set TrangThai based on quantity
            SE104_Library_Manager.Repositories.SachRepository.UpdateBookStatus(book);

            try
            {
                await sachRepo.AddAsync(book);

                MessageBox.Show("Thêm sách thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                w.DialogResult = true;
                w.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        [RelayCommand]
        public void Cancel(AddBookWindow w)
        {
            w.Close();
        }
    }
}
