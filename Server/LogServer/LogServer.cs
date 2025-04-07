using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Channels;

namespace Server
{
    public class LogServer
    {
        private static readonly Channel<string> logChannel = Channel.CreateUnbounded<string>();

        public static void StartLogging()
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "server_log.txt");

            while (true)
            {
                string logMessage = logChannel.Reader.ReadAsync().Result;
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {logMessage}\n"); 
                Console.WriteLine("Лог записан: " + logMessage);
            }
        }


        public static void LogEvent(string message)
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "server_log.txt");

            Console.WriteLine($"Логируем событие: {message}");

            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}\n");
                Console.WriteLine($"Лог записан: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в лог файл: {ex.Message}");
            }
        }

    }
}
