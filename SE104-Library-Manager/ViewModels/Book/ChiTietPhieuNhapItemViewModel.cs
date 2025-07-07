using CommunityToolkit.Mvvm.ComponentModel;
using SE104_Library_Manager.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace SE104_Library_Manager.ViewModels.Book
{
    public partial class ChiTietPhieuNhapItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Sach> availableBooks = new();

        [ObservableProperty]
        private Sach? selectedBook;

        [ObservableProperty]
        private int quantity = 1;

        [ObservableProperty]
        private int unitPrice = 0;

        [ObservableProperty]
        private int total = 0;

        public ChiTietPhieuNhapItemViewModel() { }

        public ChiTietPhieuNhapItemViewModel(ObservableCollection<Sach> allBooks, IList<int> selectedBookIds)
        {
            UpdateAvailableBooks(allBooks, selectedBookIds);
        }

        public void UpdateAvailableBooks(ObservableCollection<Sach> allBooks, IList<int> selectedBookIds)
        {
            var filtered = allBooks.Where(s => !selectedBookIds.Contains(s.MaSach) || (SelectedBook != null && s.MaSach == SelectedBook.MaSach)).ToList();
            AvailableBooks = new ObservableCollection<Sach>(filtered);
        }

        partial void OnSelectedBookChanged(Sach? value)
        {
            if (value != null)
            {
                UnitPrice = value.TriGia;
            }
        }

        partial void OnQuantityChanged(int value)
        {
            UpdateTotal();
        }

        partial void OnUnitPriceChanged(int value)
        {
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            Total = Quantity * UnitPrice;
        }
    }
} 