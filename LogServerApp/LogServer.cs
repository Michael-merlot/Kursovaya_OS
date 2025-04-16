using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LogServerApp
{
    public class LogServer
    {
        private static readonly int port = 12347; // Отдельный порт для сервера логирования
        private static readonly Dictionary<string, string> logFilePaths = new Dictionary<string, string>();

        public static void Start()
        {
            Console.WriteLine("Сервер логирования инициализируется...");
            StartServer();
        }

        private static void StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Сервер логирования запущен на порту {port}");

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Новый сервер подключился к серверу логирования");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при принятии подключения сервера: " + ex.Message);
                }
            }
        }

        private static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[8192];
            int bytesRead;
            string serverId = null;

            try
            {
                // Первое сообщение - идентификатор сервера
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                serverId = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Сервер идентифицирован как: {serverId}");

                // Создаем путь к файлу лога для этого сервера
                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serverId}_log.txt");
                lock (logFilePaths)
                {
                    logFilePaths[serverId] = logFilePath;
                }

                // Логируем подключение сервера
                string connectionMessage = $"Сервер {serverId} подключился к серверу логирования";
                LogToFile(serverId, connectionMessage);
                Console.WriteLine(connectionMessage);

                // Отправляем подтверждение
                byte[] ackData = Encoding.UTF8.GetBytes("Подключено к серверу логирования");
                stream.Write(ackData, 0, ackData.Length);

                // Обрабатываем сообщения лога
                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    LogToFile(serverId, message);

                    // Отправляем подтверждение
                    stream.Write(ackData, 0, ackData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сервера {serverId}: {ex.Message}");
                if (serverId != null)
                {
                    LogToFile(serverId, $"Ошибка: {ex.Message}");
                }
            }
            finally
            {
                client.Close();
                if (serverId != null)
                {
                    LogToFile(serverId, $"Сервер {serverId} отключился от сервера логирования");
                    Console.WriteLine($"Сервер {serverId} отключился");
                }
            }
        }

        private static void LogToFile(string serverId, string message)
        {
            try
            {
                string logFilePath;
                lock (logFilePaths)
                {
                    if (!logFilePaths.TryGetValue(serverId, out logFilePath))
                    {
                        logFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{serverId}_log.txt");
                        logFilePaths[serverId] = logFilePath;
                    }
                }

                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}\n");
                Console.WriteLine($"Лог записан ({serverId}): {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в файл лога для {serverId}: {ex.Message}");
            }
        }
    }
}
