using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Server // (или Server1 для другого проекта)
{
    public class LogServer
    {
        private static readonly string logServerIp = "127.0.0.1";
        private static readonly int logServerPort = 12347;
        private static readonly Channel<string> logChannel = Channel.CreateUnbounded<string>();
        private static TcpClient client;
        private static NetworkStream stream;
        private static bool isConnected = false;
        private static string serverId;
        private static readonly string localLogFilePath;

        static LogServer()
        {
            localLogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "local_log.txt");
        }

        public static void StartLogging(string serverIdentifier = null)
        {
            // Если идентификатор сервера не указан, используем имя пространства имен
            serverId = serverIdentifier ?? typeof(LogServer).Namespace;

            // Запускаем обработку канала
            Task.Run(async () => await ProcessLogChannel());

            // Подключаемся к серверу логирования
            Task.Run(() => ConnectToLogServer());

            // Логируем запуск
            LogEvent($"{serverId} логирование инициализировано");
        }

        private static void ConnectToLogServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect(logServerIp, logServerPort);
                stream = client.GetStream();

                // Отправляем идентификатор сервера
                byte[] idData = Encoding.UTF8.GetBytes(serverId);
                stream.Write(idData, 0, idData.Length);

                // Ждем подтверждения
                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Подключено к серверу логирования: {response}");
                isConnected = true;

                // Отправляем накопленные сообщения
                logChannel.Writer.TryWrite($"Подключено к серверу логирования как {serverId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к серверу логирования: {ex.Message}");
                LogLocally($"Ошибка подключения к серверу логирования: {ex.Message}");

                // Пытаемся переподключиться через некоторое время
                Task.Run(async () => {
                    await Task.Delay(5000); // 5 секунд
                    ConnectToLogServer();
                });
            }
        }

        private static async Task ProcessLogChannel()
        {
            while (true)
            {
                try
                {
                    // Ждем сообщение из канала
                    string message = await logChannel.Reader.ReadAsync();

                    if (isConnected)
                    {
                        try
                        {
                            // Отправляем на сервер логирования
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            stream.Write(data, 0, data.Length);

                            // Ждем подтверждения
                            byte[] buffer = new byte[8192];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка отправки лога на сервер: {ex.Message}");
                            isConnected = false;
                            LogLocally(message);

                            // Пытаемся переподключиться
                            Task.Run(() => ConnectToLogServer());
                        }
                    }
                    else
                    {
                        // Логируем локально, если нет подключения
                        LogLocally(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки канала логирования: {ex.Message}");
                    LogLocally($"Ошибка обработки канала логирования: {ex.Message}");
                }
            }
        }

        public static void LogEvent(string message)
        {
            // Отправляем сообщение в канал
            logChannel.Writer.TryWrite(message);

            // Также выводим в консоль для наглядности
            Console.WriteLine($"[ЛОГ] {message}");
        }

        private static void LogLocally(string message)
        {
            try
            {
                File.AppendAllText(localLogFilePath, $"{DateTime.Now}: [{serverId}] {message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи в локальный лог: {ex.Message}");
            }
        }
    }
}
