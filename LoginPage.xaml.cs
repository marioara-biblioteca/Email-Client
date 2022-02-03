using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace EmailClientWpf
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private  async void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ImapService.Initialize(password.Password, username.Text);
            });          
            try
            {
                await ImapService.client.RunAsync();
                if (ImapService.client.IsAuthenticated())
                {
                    MainWindow.LoggedIn = true;
                    IdleClient.currentUser = username.Text;
                }
            }
            catch (MailKit.Security.AuthenticationException authex)
            {
            this.Dispatcher.Invoke(() =>
            {
                if (!MainWindow.LoggedIn)
                {
                   
                    MainWindow.MainFrame.Content = new LoginPage(); 
                }
            });
            }
            this.Dispatcher.Invoke(() =>
            {
                if (MainWindow.LoggedIn)
                    MainWindow.MainFrame.Content = new HomePage();
            });
        }
    }
}
