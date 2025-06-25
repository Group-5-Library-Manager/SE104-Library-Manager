using SE104_Library_Manager.ViewModels.Return;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SE104_Library_Manager.Views.Return
{
    /// <summary>
    /// Interaction logic for AddPenaltyReceiptWindow.xaml
    /// </summary>
    public partial class AddPenaltyReceiptWindow : Window
    {
        public AddPenaltyReceiptWindow(AddPenaltyReceiptViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
