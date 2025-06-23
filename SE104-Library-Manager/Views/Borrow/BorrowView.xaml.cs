using SE104_Library_Manager.ViewModels.Borrow;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SE104_Library_Manager.Views.Borrow
{
    /// <summary>
    /// Interaction logic for BorrowView.xaml
    /// </summary>
    public partial class BorrowView : UserControl
    {
        public BorrowView(BorrowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
