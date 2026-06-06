using System.Windows;
using EBookLibrary.Services;

namespace EBookLibrary.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService databaseService;
        private readonly AuthService authService;

        public LoginWindow()
        {
            InitializeComponent();
            databaseService = new DatabaseService();
            authService = new AuthService(databaseService);

            // если нет пользователей, открываем окно регистрации
            if (!authService.HasAnyUser())
            {
                var registerWindow = new RegisterWindow(authService);
                registerWindow.ShowDialog();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Заполните все поля";
                return;
            }

            var user = authService.Login(login, password);
            if (user != null)
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorMessage.Text = "Неверный логин или пароль";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(authService);
            registerWindow.ShowDialog();
        }
    }
}