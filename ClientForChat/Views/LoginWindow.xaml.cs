using ClientForChat.Data;
using ClientForChat.Services;
using ClientForChat.Views;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientForChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService = new ApiService();
        private readonly TokenService _tokenService = new TokenService();
        private readonly SelfUserDatabaseService _selfService = new SelfUserDatabaseService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = LoginField.Text;  
            var password = PasswordField.Password;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username cannot be empty");
                return;
            }

            var token = await _apiService.LoginAsync(username, password);

            if (token == true)
            {
                MessageBox.Show("Token taken: " + _tokenService.GetToken());
                _selfService.SaveSelfUser(username);
                MessangerWindow messangerWindow = new MessangerWindow();
                messangerWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Cannot acces token.");
            }
        }
    }
}