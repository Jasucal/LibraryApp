using LibraryApp;
using System.Windows;

namespace Zachet
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoginWindow login = new LoginWindow();
            login.Show();
        }
    }
}