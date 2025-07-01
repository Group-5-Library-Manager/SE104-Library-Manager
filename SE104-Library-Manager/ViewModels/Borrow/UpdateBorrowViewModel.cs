using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SE104_Library_Manager.Entities;
using SE104_Library_Manager.Interfaces.Repositories;
using SE104_Library_Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class UpdateBorrowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int borrowId;

        [ObservableProperty]
        private DateOnly borrowDate;

        [ObservableProperty]
        private string formattedBorrowDate = string.Empty;

        [ObservableProperty]
        private ObservableCollection<DocGia> readers = new ObservableCollection<DocGia>();

        [ObservableProperty]
        private DocGia? selectedReader;

        [ObservableProperty]
        private ObservableCollection<Sach> allBooks = new ObservableCollection<Sach>();

        [ObservableProperty]
        private ObservableCollection<BookSelectionItem> selectedBooks = new ObservableCollection<BookSelectionItem>();

        [ObservableProperty]
        private NhanVien? currentStaff;

        private readonly IPhieuMuonRepository phieuMuonRepo;
        private readonly IDocGiaRepository docGiaRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IStaffSessionReader staffSessionReader;

        private bool _isUpdatingAvailableBooks = false;

        public UpdateBorrowViewModel(IPhieuMuonRepository phieuMuonRepository, IDocGiaRepository docGiaRepository,
                                    INhanVienRepository nhanVienRepository, IStaffSessionReader staffSessionReader)
        {
            phieuMuonRepo = phieuMuonRepository;
            docGiaRepo = docGiaRepository;
            nhanVienRepo = nhanVienRepository;
            this.staffSessionReader = staffSessionReader;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var dsDocGia = await docGiaRepo.GetAllAsync();
                Readers = new ObservableCollection<DocGia>(dsDocGia);

                var dsSach = await phieuMuonRepo.GetAvailableBooksAsync();
                AllBooks = new ObservableCollection<Sach>(dsSach);

                if (staffSessionReader.CurrentStaffId > 0)
                {
                    CurrentStaff = await nhanVienRepo.GetByIdAsync(staffSessionReader.CurrentStaffId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadBorrowData(PhieuMuon phieuMuon)
        {
            _isUpdatingAvailableBooks = true;

            try
            {
                BorrowId = phieuMuon.MaPhieuMuon;
                BorrowDate = phieuMuon.NgayMuon;
                FormattedBorrowDate = phieuMuon.NgayMuon.ToString("dd/MM/yyyy");
                SelectedReader = Readers.FirstOrDefault(r => r.MaDocGia == phieuMuon.MaDocGia);

                // Clear existing selected books
                SelectedBooks.Clear();

                // Load borrowed books and add them to available books if they're not already there
                foreach (var chiTiet in phieuMuon.DsChiTietPhieuMuon)
                {
                    // Add borrowed book to AllBooks if it's not already there (in case it's currently borrowed)
                    if (!AllBooks.Any(b => b.MaSach == chiTiet.MaSach))
                    {
                        try
                        {
                            var borrowedBook = await phieuMuonRepo.GetBookByIdAsync(chiTiet.MaSach);
                            if (borrowedBook != null)
                            {
                                AllBooks.Add(borrowedBook);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi tải thông tin sách {chiTiet.MaSach}: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                    }

                    // Create BookSelectionItem for each borrowed book
                    var bookItem = new BookSelectionItem();
                    var selectedBook = AllBooks.FirstOrDefault(b => b.MaSach == chiTiet.MaSach);
                    if (selectedBook != null)
                    {
                        bookItem.SelectedBook = selectedBook;
                    }

                    // Subscribe to property changes
                    bookItem.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(BookSelectionItem.SelectedBook) && !_isUpdatingAvailableBooks)
                        {
                            UpdateAllAvailableBooks();
                        }
                    };

                    SelectedBooks.Add(bookItem);
                }
            }
            finally
            {
                _isUpdatingAvailableBooks = false;
            }

            // Update available books for all items
            UpdateAllAvailableBooks();
        }

        [RelayCommand]
        private void AddBookToSelected()
        {
            var newBookItem = new BookSelectionItem();
            UpdateAvailableBooksForItem(newBookItem);

            // Subscribe to property changes to update other ComboBoxes when selection changes
            newBookItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(BookSelectionItem.SelectedBook) && !_isUpdatingAvailableBooks)
                {
                    UpdateAllAvailableBooks();
                }
            };

            SelectedBooks.Add(newBookItem);
        }

        [RelayCommand]
        private void RemoveBookFromSelected(BookSelectionItem bookItem)
        {
            if (SelectedBooks.Contains(bookItem))
            {
                SelectedBooks.Remove(bookItem);
                UpdateAllAvailableBooks();
            }
        }

        private void UpdateAllAvailableBooks()
        {
            if (_isUpdatingAvailableBooks) return;

            _isUpdatingAvailableBooks = true;

            try
            {
                // Get all currently selected book IDs
                var selectedBookIds = SelectedBooks
                    .Where(item => item.SelectedBook != null)
                    .Select(item => item.SelectedBook!.MaSach)
                    .ToHashSet();

                // Update available books for each item
                foreach (var item in SelectedBooks)
                {
                    UpdateAvailableBooksForItem(item, selectedBookIds);
                }
            }
            finally
            {
                _isUpdatingAvailableBooks = false;
            }
        }

        private void UpdateAvailableBooksForItem(BookSelectionItem item, HashSet<int>? excludeIds = null)
        {
            if (_isUpdatingAvailableBooks && excludeIds == null)
            {
                // If we're in a batch update, don't trigger individual updates
                return;
            }

            excludeIds ??= SelectedBooks
                .Where(x => x != item && x.SelectedBook != null)
                .Select(x => x.SelectedBook!.MaSach)
                .ToHashSet();

            var availableBooks = AllBooks
                .Where(book => !excludeIds.Contains(book.MaSach))
                .ToList();

            // Always ensure the currently selected book is available in its own ComboBox
            if (item.SelectedBook != null && !availableBooks.Any(b => b.MaSach == item.SelectedBook.MaSach))
            {
                availableBooks.Insert(0, item.SelectedBook);
            }

            // Store current selection to restore it after updating the collection
            var currentSelectionId = item.SelectedBook?.MaSach;

            // Clear and repopulate the available books
            item.AvailableBooks.Clear();
            foreach (var book in availableBooks)
            {
                item.AvailableBooks.Add(book);
            }

            // Restore selection if it exists in the new collection
            if (currentSelectionId.HasValue)
            {
                var bookToSelect = item.AvailableBooks.FirstOrDefault(b => b.MaSach == currentSelectionId.Value);
                if (bookToSelect != null)
                {
                    item.SelectedBook = bookToSelect;
                }
            }
        }

        [RelayCommand]
        private async Task SaveBorrow(Window window)
        {
            if (SelectedReader == null)
            {
                MessageBox.Show("Vui lòng chọn độc giả", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedBooksList = SelectedBooks
                .Where(item => item.SelectedBook != null)
                .Select(item => item.SelectedBook!)
                .ToList();

            if (selectedBooksList.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sách để mượn", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var phieuMuon = new PhieuMuon
                {
                    MaPhieuMuon = BorrowId,
                    NgayMuon = BorrowDate,
                    MaDocGia = SelectedReader.MaDocGia,
                    MaNhanVien = staffSessionReader.CurrentStaffId
                };

                var dsChiTietPhieuMuon = selectedBooksList.Select(s => new ChiTietPhieuMuon
                {
                    MaPhieuMuon = BorrowId,
                    MaSach = s.MaSach
                }).ToList();

                await phieuMuonRepo.UpdateAsync(phieuMuon, dsChiTietPhieuMuon);

                MessageBox.Show("Cập nhật phiếu mượn thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
