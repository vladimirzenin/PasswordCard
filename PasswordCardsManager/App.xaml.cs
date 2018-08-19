using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordCardsManager
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 1
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 2

            PasswordCardManager.Init();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {


            base.OnExit(e);
        }
    }
}
