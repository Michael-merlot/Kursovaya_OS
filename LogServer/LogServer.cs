using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace LogServer
{
    class LogServer
    {
        private const string LOG_DIRECTORY = "Logs";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Сервер логирования запускается...");

            // Создание директории для логов
            if (!Directory.Exists(LOG_DIRECTORY))
                Directory.CreateDirectory(LOG_DIRECTORY);

            Console.WriteLine("Сервер логирования запущен. Ожидание подключений...");

            // Запускаем обработку для обоих серверов параллельно
            Task server1Task = HandleServerLogsAsync("Server1");
            Task server2Task = HandleServerLogsAsync("Server2");

            // Ждем завершения обоих задач (в нормальной работе они никогда не завершатся)
            await Task.WhenAll(server1Task, server2Task);
        }

        static async Task HandleServerLogsAsync(string serverName)
        {
            string logFilePath = Path.Combine(LOG_DIRECTORY, $"{serverName}_log.txt");

            while (true)
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream(
                        $"LogPipe_{serverName}",
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Message))
                    {
                        Console.WriteLine($"Ожидание подключения от {serverName}...");
                        await pipeServer.WaitForConnectionAsync();
                        Console.WriteLine($"{serverName} подключен к серверу логирования");

                        // Логируем подключение сервера
                        await LogMessageAsync(logFilePath, $"{DateTime.Now}: {serverName} подключен к серверу логирования");

                        using (var reader = new StreamReader(pipeServer, Encoding.UTF8))
                        {
                            while (pipeServer.IsConnected)
                            {
                                string message = await reader.ReadLineAsync();
                                if (message == null) break;

                                // Записываем сообщение в лог-файл
                                await LogMessageAsync(logFilePath, $"{DateTime.Now}: {message}");
                                Console.WriteLine($"Лог от {serverName}: {message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку и продолжаем работу
                    await LogMessageAsync(logFilePath, $"{DateTime.Now}: ОШИБКА: {ex.Message}");
                    Console.WriteLine($"Ошибка при обработке логов {serverName}: {ex.Message}");

                    // Небольшая пауза перед повторным подключением
                    await Task.Delay(1000);
                }
            }
        }

        static async Task LogMessageAsync(string filePath, string message)
        {
            try
            {
                // Добавляем сообщение в файл
                await File.AppendAllTextAsync(filePath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи в лог-файл: {ex.Message}");
            }
        }
    }
}
