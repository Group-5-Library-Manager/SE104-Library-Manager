using SE104_Library_Manager.ViewModels.Account;
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

namespace SE104_Library_Manager.Views.Account
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow(ChangePasswordViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void oldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(DataContext !=null && (DataContext as ChangePasswordViewModel) != null)
            {
                (DataContext as ChangePasswordViewModel).OldPassword = ((PasswordBox)sender).Password;
            }
            
        }

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && (DataContext as ChangePasswordViewModel) != null)
            {
                (DataContext as ChangePasswordViewModel).NewPassword = ((PasswordBox)sender).Password;
            }
        }

        private void confirmNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && (DataContext as ChangePasswordViewModel) != null)
            {
                (DataContext as ChangePasswordViewModel).ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
