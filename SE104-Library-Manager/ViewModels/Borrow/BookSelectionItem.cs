using CommunityToolkit.Mvvm.ComponentModel;
using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class BookSelectionItem: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Sach> availableBooks = new ObservableCollection<Sach>();

        [ObservableProperty]
        private Sach? selectedBook;

        private List<BanSaoSach> selectedCopies = new List<BanSaoSach>();
        public List<BanSaoSach> SelectedCopies
        {
            get => selectedCopies;
            set
            {
                selectedCopies = value;
                OnPropertyChanged(nameof(SelectedCopies));
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public int Quantity => SelectedCopies?.Count ?? 0;

        public IRelayCommand SelectCopiesCommand { get; set; }

        public string SelectedCopyIdsDisplay => SelectedCopies != null && SelectedCopies.Count > 0
            ? string.Join(", ", SelectedCopies.Select(c => c.MaBanSao))
            : "Chưa chọn bản sao";

        public BookSelectionItem()
        {
        }

        public BookSelectionItem(ObservableCollection<Sach> books)
        {
            AvailableBooks = books;
        }
    }
}
