using SE104_Library_Manager.ViewModels.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SE104_Library_Manager.Views.Book
{
    /// <summary>
    /// Interaction logic for AddPublisherWindow.xaml
    /// </summary>
    public partial class AddPublisherWindow : Window
    {
        public AddPublisherWindow(AddPublisherViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
