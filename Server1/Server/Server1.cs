using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Server1
{
    public class Server1
    {
        private static readonly int port = 12346;  // порт для сервера 2
        private static readonly int maxClients = 5;  // максимум 5 клиентов
        private static int currentClients = 0;  // текущее количество клиентов

        public static void StartServer()
        {
            Task.Run(() => LogServer.StartLogging());

            LogServer.LogEvent("Сервер 2 запущен на порту " + port);

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Сервер 2 запущен на порту {port}");

            while (true)
            {
                try
                {
                    if (currentClients >= maxClients)
                    {
                        Console.WriteLine("Максимальное количество клиентов подключено.");
                        Thread.Sleep(1000);
                        continue;
                    }

                    TcpClient client = listener.AcceptTcpClient();
                    currentClients++;
                    LogServer.LogEvent("Новый клиент подключился");
                    Console.WriteLine("Новый клиент подключился");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    LogServer.LogEvent("Ошибка при принятии клиента: " + ex.Message);
                    Console.WriteLine("Ошибка при принятии клиента: " + ex.Message);
                }
            }
        }

        private static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[8192];
            int bytesRead;

            try
            {
                while (true)
                {
                    Console.WriteLine("Ожидаем команду от клиента...");
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Получен запрос от клиента: " + request);

                    if (request.ToLower() == "update")
                    {
                        string physicalMemoryUsage = GetPhysicalMemoryUsage();
                        string virtualMemoryUsage = GetVirtualMemoryUsage();
                        string result = $"{physicalMemoryUsage}\n{virtualMemoryUsage}";

                        Console.WriteLine("Отправляем информацию клиенту...");
                        byte[] data = Encoding.UTF8.GetBytes(result);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine("Информация отправлена клиенту.");

                        LogServer.LogEvent($"Информация от сервера 2: {result}");
                    }
                    else if (request.ToLower() == "exit")
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Неизвестная команда от клиента.");
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка при обработке клиента: " + ex.Message);
                LogServer.LogEvent($"Ошибка при обработке клиента: {ex.Message}");
            }
            finally
            {
                client.Close();
                currentClients--;
                Console.WriteLine("Клиент отключен. Текущий счетчик клиентов: " + currentClients);
                LogServer.LogEvent("Клиент отключен");
            }
        }

        private static string GetPhysicalMemoryUsage()
        {
            try
            {
                string command = "wmic OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"Вывод команды WMIC:\n{output}");

                string freeMemoryLine = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                            .FirstOrDefault(line => line.StartsWith("FreePhysicalMemory="));
                string totalMemoryLine = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                             .FirstOrDefault(line => line.StartsWith("TotalVisibleMemorySize="));

                if (freeMemoryLine == null || totalMemoryLine == null)
                {
                    return "Ошибка: не удалось получить данные о физической памяти";
                }

                string freeMemory = freeMemoryLine.Split('=')[1].Trim();
                string totalMemory = totalMemoryLine.Split('=')[1].Trim();

                double freeMemInMB = double.Parse(freeMemory) / 1024;
                double totalMemInMB = double.Parse(totalMemory) / 1024;

                double usedMem = totalMemInMB - freeMemInMB;
                double usagePercentage = (usedMem / totalMemInMB) * 100;

                return $"Процент использования физической памяти: {usagePercentage:0.00}%";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении данных о физической памяти: " + ex.Message);
                return "Ошибка при получении данных о физической памяти: " + ex.Message;
            }
        }

        private static string GetVirtualMemoryUsage()
        {
            try
            {
                string command = "wmic OS get FreeVirtualMemory,TotalVirtualMemorySize /Value";
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"Вывод команды WMIC:\n{output}");

                string freeMemoryLine = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                            .FirstOrDefault(line => line.StartsWith("FreeVirtualMemory="));
                string totalMemoryLine = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                             .FirstOrDefault(line => line.StartsWith("TotalVirtualMemorySize="));

                if (freeMemoryLine == null || totalMemoryLine == null)
                {
                    return "Ошибка: не удалось получить данные о виртуальной памяти";
                }

                string freeMemory = freeMemoryLine.Split('=')[1].Trim();
                string totalMemory = totalMemoryLine.Split('=')[1].Trim();

                double freeMemInMB = double.Parse(freeMemory) / 1024;
                double totalMemInMB = double.Parse(totalMemory) / 1024;

                double usedMem = totalMemInMB - freeMemInMB;
                double usagePercentage = (usedMem / totalMemInMB) * 100;

                return $"Процент использования виртуальной памяти: {usagePercentage:0.00}%";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении данных о виртуальной памяти: " + ex.Message);
                return "Ошибка при получении данных о виртуальной памяти: " + ex.Message;
            }
        }

    }
}
