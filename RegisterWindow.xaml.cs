using System.Windows;
using EBookLibrary.Services;

namespace EBookLibrary.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService authService;

        public RegisterWindow(AuthService authService)
        {
            InitializeComponent();
            this.authService = authService;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Заполните все поля";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage.Text = "Пароли не совпадают";
                return;
            }

            if (password.Length < 4)
            {
                ErrorMessage.Text = "Пароль должен быть не менее 4 символов";
                return;
            }

            bool success = authService.Register(login, password);
            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                ErrorMessage.Text = "Пользователь с таким логином уже существует";
            }
        }
    }
}