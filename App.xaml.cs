using System.Windows;

namespace EBookLibrary
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // показываем окно входа при запуске
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
    }
}