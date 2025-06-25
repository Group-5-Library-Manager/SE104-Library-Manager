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
    public partial class AddBorrowViewModel: ObservableObject
    {
        [ObservableProperty]
        private DateOnly borrowDate = DateOnly.FromDateTime(DateTime.Now);

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

        private bool _isUpdatingAvailableBooks = false;

        private readonly IPhieuMuonRepository phieuMuonRepo;
        private readonly IDocGiaRepository docGiaRepo;
        private readonly INhanVienRepository nhanVienRepo;
        private readonly IStaffSessionReader staffSessionReader;

        public AddBorrowViewModel(IPhieuMuonRepository phieuMuonRepository, IDocGiaRepository docGiaRepository,
                                 INhanVienRepository nhanVienRepository ,IStaffSessionReader staffSessionReader)
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
                BorrowDate = DateOnly.FromDateTime(DateTime.Now);
                AddInitialBookItem();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddInitialBookItem()
        {
            var initialBookItem = new BookSelectionItem();
            UpdateAvailableBooksForItem(initialBookItem);

            initialBookItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(BookSelectionItem.SelectedBook) && !_isUpdatingAvailableBooks)
                {
                    UpdateAllAvailableBooks();
                }
            };

            SelectedBooks.Add(initialBookItem);
        }

        [RelayCommand]
        private void AddBookToSelected()
        {
            var newBookItem = new BookSelectionItem();
            UpdateAvailableBooksForItem(newBookItem);

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
                    NgayMuon = BorrowDate,
                    MaDocGia = SelectedReader.MaDocGia,
                    MaNhanVien = staffSessionReader.CurrentStaffId
                };

                var dsChiTietPhieuMuon = selectedBooksList.Select(s => new ChiTietPhieuMuon
                {
                    MaPhieuMuon = 0, // Sẽ được cập nhật sau khi thêm phiếu mượn
                    MaSach = s.MaSach
                }).ToList();

                await phieuMuonRepo.AddAsync(phieuMuon, dsChiTietPhieuMuon);

                MessageBox.Show("Thêm phiếu mượn thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm phiếu mượn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
