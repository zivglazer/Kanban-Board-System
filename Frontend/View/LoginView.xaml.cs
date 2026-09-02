using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using Frontend.Controllers;
using Frontend.Model;
using Frontend.ViewModel;

namespace Frontend.View
{
    public partial class LoginView : Window
    {
        private readonly LoginVM loginVM;

        public LoginView()
        {
            InitializeComponent();

            loginVM = new LoginVM();
            this.DataContext = loginVM;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            UserModel user = loginVM.Login(PasswordBox.Password);
            if (user != null)
            {
                var dashboard = new DashboardView(user);
                dashboard.Show();
                this.Close();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            UserModel user = loginVM.Register(PasswordBox.Password);
            if (user != null)
            {
                var dashboard = new DashboardView(user);
                dashboard.Show();
                this.Close();
            }
        }
    }
}
