using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SilkShield_New
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginWindow = new View.MainWindow();
            loginWindow.Show();

            // Set the initial main window to the login window.
            // When this window is closed, the application will shut down.
            Application.Current.MainWindow = loginWindow;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
