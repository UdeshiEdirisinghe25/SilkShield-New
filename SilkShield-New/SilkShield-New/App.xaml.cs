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
            var loginWindow = new View.LoginPage();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }

}
