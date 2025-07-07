using CommunityToolkit.Mvvm.ComponentModel;
using SE104_Library_Manager.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_Library_Manager.ViewModels.Borrow
{
    public partial class BookSelectionItem: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Sach> availableBooks = new ObservableCollection<Sach>();

        [ObservableProperty]
        private Sach? selectedBook;

        [ObservableProperty]
        private int quantity = 1;

        public BookSelectionItem()
        {
        }

        public BookSelectionItem(ObservableCollection<Sach> books)
        {
            AvailableBooks = books;
        }
    }
}
