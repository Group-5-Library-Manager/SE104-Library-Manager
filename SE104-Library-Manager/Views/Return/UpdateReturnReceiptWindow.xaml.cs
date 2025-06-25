using SE104_Library_Manager.ViewModels;
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

namespace SE104_Library_Manager.Views.Return
{
    /// <summary>
    /// Interaction logic for UpdateReturnReceiptWindow.xaml
    /// </summary>
    public partial class UpdateReturnReceiptWindow : Window
    {
        public UpdateReturnReceiptWindow(UpdateReturnReceiptViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
