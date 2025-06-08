using SE104_Library_Manager.ViewModels;
using System.Windows;

namespace SE104_Library_Manager.Views
{
    /// <summary>
    /// Interaction logic for AddDegreeWindow.xaml
    /// </summary>
    public partial class AddDegreeWindow : Window
    {
        public AddDegreeWindow(AddDegreeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
