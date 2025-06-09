using SE104_Library_Manager.ViewModels;
using System.Windows;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for AddReaderWindow.xaml
    /// </summary>
    public partial class AddReaderWindow : Window
    {
        public AddReaderWindow(AddReaderViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
