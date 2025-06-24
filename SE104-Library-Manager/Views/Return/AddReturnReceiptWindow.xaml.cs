using SE104_Library_Manager.ViewModels.Return;
using System.Windows;

namespace SE104_Library_Manager.Views.Return
{
    /// <summary>
    /// Interaction logic for AddReturnReceiptWindow.xaml
    /// </summary>
    public partial class AddReturnReceiptWindow : Window
    {
        public AddReturnReceiptWindow(AddReturnReceiptViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}