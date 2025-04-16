using System;

namespace LogServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Запуск сервера логирования...");
            LogServer.Start();
            Console.ReadLine(); // Держим консоль открытой
        }
    }
}
