using System.Windows;

namespace ShopApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }
            var user = Database.Authenticate(login, password);
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль.");
                return;
            }
            if (user.Phone == "89123180236")
            {
                var adminWin = new AdminWindow(user);
                adminWin.Show();
            }
            else
            {
                var userWin = new UserWindow(user);
                userWin.Show();
            }
            this.Close();
        }
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            var regWin = new RegisterWindow();
            regWin.ShowDialog();
        }
    }
}