using System;
using System.Windows;
using System.Threading.Tasks;

namespace ServerKursovayaWPF
{
    public partial class App : Application
    {
        public App()
        {
            // Запуск сервера при старте приложения
            Task.Run(() => Server.Server.StartServer());
        }
    }
}
