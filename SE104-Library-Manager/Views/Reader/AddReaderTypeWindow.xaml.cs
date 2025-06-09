using SE104_Library_Manager.ViewModels;
using System.Windows;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for AddReaderTypeWindow.xaml
    /// </summary>
    public partial class AddReaderTypeWindow : Window
    {
        public AddReaderTypeWindow(AddReaderTypeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
