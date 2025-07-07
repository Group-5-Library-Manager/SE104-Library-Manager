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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SE104_Library_Manager.ViewModels.Book
{
    public partial class BookViewModel(ISachRepository sachRepo, ITheLoaiRepository theLoaiRepo, ITacGiaRepository tacGiaRepo, INhaXuatBanRepository nhaXuatBanRepo, IQuyDinhRepository quyDinhRepo) : ObservableObject
    {
        [ObservableProperty]
        private TabItem selectedTab = null!;

        [ObservableProperty]
        private ObservableCollection<Sach> dsSach = new ObservableCollection<Sach>();

        [ObservableProperty]
        private Sach? selectedBook;

        [ObservableProperty]
        private Sach? selectedBookForEdit;

        [ObservableProperty]
        private TheLoai? selectedGenre;

        [ObservableProperty]
        private TheLoai? selectedGenreForEdit;

        [ObservableProperty]
        private TacGia? selectedAuthor;

        [ObservableProperty]
        private TacGia? selectedAuthorForEdit;

        [ObservableProperty]
        private NhaXuatBan? selectedPublisher;

        [ObservableProperty]
        private NhaXuatBan? selectedPublisherForEdit;

        [ObservableProperty]
        private ObservableCollection<TheLoai> dsTheLoai = new ObservableCollection<TheLoai>();

        [ObservableProperty]
        private ObservableCollection<TacGia> dsTacGia = new ObservableCollection<TacGia>();

        [ObservableProperty]
        private ObservableCollection<NhaXuatBan> dsNXB = new ObservableCollection<NhaXuatBan>();

        [ObservableProperty]
        private string searchBookQuery = string.Empty;

        [ObservableProperty]
        private string searchGenreQuery = string.Empty;

        [ObservableProperty]
        private string searchAuthorQuery = string.Empty;

        [ObservableProperty]
        private string searchPublisherQuery = string.Empty;

        [ObservableProperty]
        private QuyDinh quyDinhHienTai = null!;

        private List<Sach> originalDsSach = new List<Sach>();
        private List<TheLoai> originalDsTheLoai = new List<TheLoai>();
        private List<TacGia> originalDsTacGia = new List<TacGia>();
        private List<NhaXuatBan> originalDsNXB = new List<NhaXuatBan>();

        private async Task LoadDataAsync()
        {
            originalDsSach = await sachRepo.GetAllAsync();
            originalDsTheLoai = await theLoaiRepo.GetAllAsync();
            originalDsTacGia = await tacGiaRepo.GetAllAsync();
            originalDsNXB = await nhaXuatBanRepo.GetAllAsync();

            if (originalDsSach == null || originalDsTheLoai == null || originalDsTacGia == null || originalDsNXB == null) return;

            DsSach = new ObservableCollection<Sach>(originalDsSach);

            DsTheLoai = new ObservableCollection<TheLoai>(originalDsTheLoai);

            DsTacGia = new ObservableCollection<TacGia>(originalDsTacGia);

            DsNXB = new ObservableCollection<NhaXuatBan>(originalDsNXB);

            QuyDinhHienTai = await quyDinhRepo.GetQuyDinhAsync();

            SearchBookQuery = string.Empty;
            SelectedBook = null;
        }

        [RelayCommand]
        public async Task AddBook()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddBookWindow)) as AddBookWindow;
            if (w == null) return;
            w.Owner = Application.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task EditBook()
        {
            if (SelectedBookForEdit == null || SelectedBook == null) return;

            SelectedBookForEdit.TenSach = SelectedBookForEdit.TenSach.Trim();

            try
            {
                await sachRepo.UpdateAsync(SelectedBookForEdit);
                var index = DsSach.IndexOf(SelectedBook);
                var originalIndex = originalDsSach.IndexOf(SelectedBook);
                var updatedBook = await sachRepo.GetByIdAsync(SelectedBookForEdit.MaSach);

                if (index >= 0 && originalIndex >= 0 && updatedBook != null)
                {
                    DsSach[index] = updatedBook;
                    originalDsSach[originalIndex] = updatedBook;
                }

                SelectedBook = updatedBook;

                MessageBox.Show("Cập nhật thông tin thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteBook()
        {
            if (SelectedBookForEdit == null || SelectedBook == null) return;

            try
            {
                MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa sách này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                await sachRepo.DeleteAsync(SelectedBook.MaSach);
                DsSach.Remove(SelectedBook);
                originalDsSach.Remove(SelectedBook);
                SelectedBook = null;

                MessageBox.Show("Xóa sách thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void SearchBooks()
        {
            if (SearchBookQuery == null || SearchBookQuery.Trim() == string.Empty)
            {
                DsSach = new ObservableCollection<Sach>(originalDsSach);
                return;
            }

            var filteredBooks = originalDsSach
                .Where(r => r.TenSach.Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.TheLoai.TenTheLoai.Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.MaTheLoai.ToString().Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.TacGia.TenTacGia.Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.MaTacGia.ToString().Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.NhaXuatBan.TenNhaXuatBan.Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.MaNhaXuatBan.ToString().Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase) ||
                            r.NamXuatBan.ToString().Contains(SearchBookQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            DsSach = new ObservableCollection<Sach>(filteredBooks);
        }

        [RelayCommand]
        public async Task AddAuthor()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddAuthorWindow)) as AddAuthorWindow;
            if (w == null) return;
            w.Owner = Application.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task EditAuthor()
        {
            if (SelectedAuthor == null || SelectedAuthorForEdit == null) return;

            SelectedAuthorForEdit.TenTacGia = SelectedAuthorForEdit.TenTacGia.Trim();

            try
            {
                await tacGiaRepo.UpdateAsync(SelectedAuthorForEdit);
                var index = DsTacGia.IndexOf(SelectedAuthor);
                var originalIndex = originalDsTacGia.IndexOf(SelectedAuthor);
                var updatedAuthor = await tacGiaRepo.GetByIdAsync(SelectedAuthorForEdit.MaTacGia);

                if (index >= 0 && originalIndex >= 0 && updatedAuthor != null)
                {
                    DsTacGia[index] = updatedAuthor;
                    originalDsTacGia[originalIndex] = updatedAuthor;
                }

                SelectedAuthor = updatedAuthor;

                MessageBox.Show("Cập nhật thông tin tác giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteAuthor()
        {
            if (SelectedAuthor == null || SelectedAuthorForEdit == null) return;

            try
            {
                MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa tác giả này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                await tacGiaRepo.DeleteAsync(SelectedAuthor.MaTacGia);
                DsTacGia.Remove(SelectedAuthor);
                originalDsTacGia.Remove(SelectedAuthor);
                SelectedAuthor = null;

                MessageBox.Show("Xóa tác giả thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void SearchAuthors()
        {
            if (SearchAuthorQuery == null || SearchAuthorQuery.Trim() == string.Empty)
            {
                DsTacGia = new ObservableCollection<TacGia>(originalDsTacGia);
                return;
            }

            var filteredAuthors = originalDsTacGia
                .Where(rt => rt.TenTacGia.Contains(SearchAuthorQuery, StringComparison.OrdinalIgnoreCase) ||
                             rt.MaTacGia.ToString().Contains(SearchAuthorQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            DsTacGia = new ObservableCollection<TacGia>(filteredAuthors);
        }

        [RelayCommand]
        public async Task AddGenre()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddGenreWindow)) as AddGenreWindow;
            if (w == null) return;
            w.Owner = Application.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task EditGenre()
        {
            if (SelectedGenre == null || SelectedGenreForEdit == null) return;

            SelectedGenreForEdit.TenTheLoai = SelectedGenreForEdit.TenTheLoai.Trim();

            try
            {
                await theLoaiRepo.UpdateAsync(SelectedGenreForEdit);
                var index = DsTheLoai.IndexOf(SelectedGenre);
                var originalIndex = originalDsTheLoai.IndexOf(SelectedGenre);
                var updatedGenre = await theLoaiRepo.GetByIdAsync(SelectedGenreForEdit.MaTheLoai);

                if (index >= 0 && originalIndex >= 0 && updatedGenre != null)
                {
                    DsTheLoai[index] = updatedGenre;
                    originalDsTheLoai[originalIndex] = updatedGenre;
                }

                SelectedGenre = updatedGenre;

                MessageBox.Show("Cập nhật thông tin thể loại thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeleteGenre()
        {
            if (SelectedGenre == null || SelectedGenreForEdit == null) return;

            try
            {
                MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa thể loại này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                await theLoaiRepo.DeleteAsync(SelectedGenre.MaTheLoai);
                DsTheLoai.Remove(SelectedGenre);
                originalDsTheLoai.Remove(SelectedGenre);
                SelectedGenre = null;

                MessageBox.Show("Xóa thể loại thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void SearchGenres()
        {
            if (SearchGenreQuery == null || SearchGenreQuery.Trim() == string.Empty)
            {
                DsTheLoai = new ObservableCollection<TheLoai>(originalDsTheLoai);
                return;
            }

            var filteredGenres = originalDsTheLoai
                .Where(rt => rt.TenTheLoai.Contains(SearchGenreQuery, StringComparison.OrdinalIgnoreCase) ||
                             rt.MaTheLoai.ToString().Contains(SearchGenreQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            DsTheLoai = new ObservableCollection<TheLoai>(filteredGenres);
        }

        [RelayCommand]
        public async Task AddPublisher()
        {
            var w = App.ServiceProvider?.GetService(typeof(AddPublisherWindow)) as AddPublisherWindow;
            if (w == null) return;
            w.Owner = Application.Current.MainWindow;
            w.ShowDialog();

            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task EditPublisher()
        {
            if (SelectedPublisher == null || SelectedPublisherForEdit == null) return;

            SelectedPublisherForEdit.TenNhaXuatBan = SelectedPublisherForEdit.TenNhaXuatBan.Trim();

            try
            {
                await nhaXuatBanRepo.UpdateAsync(SelectedPublisherForEdit);
                var index = DsNXB.IndexOf(SelectedPublisher);
                var originalIndex = originalDsNXB.IndexOf(SelectedPublisher);
                var updatedPublisher = await nhaXuatBanRepo.GetByIdAsync(SelectedPublisherForEdit.MaNhaXuatBan);

                if (index >= 0 && originalIndex >= 0 && updatedPublisher != null)
                {
                    DsNXB[index] = updatedPublisher;
                    originalDsNXB[originalIndex] = updatedPublisher;
                }

                SelectedPublisher = updatedPublisher;

                MessageBox.Show("Cập nhật thông tin nhà xuất bản thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public async Task DeletePublisher()
        {
            if (SelectedPublisher == null || SelectedPublisherForEdit == null) return;

            try
            {
                MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa nhà xuất bản này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                await nhaXuatBanRepo.DeleteAsync(SelectedPublisher.MaNhaXuatBan);
                DsNXB.Remove(SelectedPublisher);
                originalDsNXB.Remove(SelectedPublisher);
                SelectedPublisher = null;

                MessageBox.Show("Xóa nhà xuất bản thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void SearchPublishers()
        {
            if (SearchPublisherQuery == null || SearchPublisherQuery.Trim() == string.Empty)
            {
                DsNXB = new ObservableCollection<NhaXuatBan>(originalDsNXB);
                return;
            }

            var filteredPublishers = originalDsNXB
                .Where(rt => rt.TenNhaXuatBan.Contains(SearchPublisherQuery, StringComparison.OrdinalIgnoreCase) ||
                             rt.MaNhaXuatBan.ToString().Contains(SearchPublisherQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            DsNXB = new ObservableCollection<NhaXuatBan>(filteredPublishers);
        }

        [RelayCommand]
        public async Task ImportBooks()
        {
            try
            {
                var w = App.ServiceProvider?.GetService(typeof(AddBookImportWindow)) as AddBookImportWindow;
                if (w == null)
                {
                    MessageBox.Show("Failed to resolve AddBookImportWindow from DI", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                w.Owner = Application.Current.MainWindow;
                w.ShowDialog();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ImportBooks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedTabChanged(TabItem value)
        {
            if (value.Header.ToString() == "Sách")
            {
                LoadDataAsync().ConfigureAwait(false);
            }
        }
        partial void OnSelectedBookChanged(Sach? value)
        {
            if (value == null)
            {
                SelectedBookForEdit = null;
            }
            else
            {
                SelectedBookForEdit = new Sach
                {
                    MaSach = value.MaSach,
                    TenSach = value.TenSach,
                    MaTacGia = value.MaTacGia,
                    MaTheLoai = value.MaTheLoai,
                    MaNhaXuatBan = value.MaNhaXuatBan,
                    NamXuatBan = value.NamXuatBan,
                    TriGia = value.TriGia,
                    NgayNhap = value.NgayNhap,
                    TrangThai = value.TrangThai
                };
            }
        }
        partial void OnSelectedAuthorChanged(TacGia? value)
        {
            if (value == null)
            {
                SelectedAuthorForEdit = null;
            }
            else
            {
                SelectedAuthorForEdit = new TacGia
                {
                    MaTacGia = value.MaTacGia,
                    TenTacGia = value.TenTacGia
                };
            }
        }
        partial void OnSelectedGenreChanged(TheLoai? value)
        {
            if (value == null)
            {
                SelectedGenreForEdit = null;
            }
            else
            {
                SelectedGenreForEdit = new TheLoai
                {
                    MaTheLoai = value.MaTheLoai,
                    TenTheLoai = value.TenTheLoai
                };
            }
        }
        partial void OnSelectedPublisherChanged(NhaXuatBan? value)
        {
            if (value == null)
            {
                SelectedPublisherForEdit = null;
            }
            else
            {
                SelectedPublisherForEdit = new NhaXuatBan
                {
                    MaNhaXuatBan = value.MaNhaXuatBan,
                    TenNhaXuatBan = value.TenNhaXuatBan
                };
            }
        }
        partial void OnSearchBookQueryChanged(string value)
        {
            SearchBooks();
        }
        partial void OnSearchAuthorQueryChanged(string value)
        {
            SearchAuthors();
        }
        partial void OnSearchGenreQueryChanged(string value)
        {
            SearchGenres();
        }
        partial void OnSearchPublisherQueryChanged(string value)
        {
            SearchPublishers();
        }
    }
}
