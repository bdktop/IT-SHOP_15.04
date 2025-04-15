using System.Windows;

namespace ShopApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }
            var newUser = new User { Email = email, Phone = phone, Login = login, PasswordHash = password };
            bool success = Database.RegisterUser(newUser);
            if (!success)
                MessageBox.Show("Пользователь с таким телефоном уже существует.");
            else
            {
                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.");
                this.Close();
            }
        }
    }
}