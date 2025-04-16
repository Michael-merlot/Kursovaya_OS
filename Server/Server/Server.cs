using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Server
{
    public class Server
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static readonly int port = 12345;
        private static readonly int maxClients = 6;
        private static int currentClients = 0;
        private static string previousSystemInfo = "";
        private static string previousLoadInfo = "";

        public static void StartServer()
        {
            Task.Run(() => LogServer.StartLogging("Server1"));

            LogServer.LogEvent("Сервер 1 запущен на порту " + port);

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Сервер 1 запущен на порту {port}");

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
                        string systemInfo = SystemInfo.GetSystemInfo();

                        if (systemInfo != previousSystemInfo)
                        {
                            Console.WriteLine("Отправляем информацию клиенту...");
                            byte[] data = Encoding.UTF8.GetBytes(systemInfo);
                            stream.Write(data, 0, data.Length);
                            Console.WriteLine("Информация отправлена клиенту.");

                            LogServer.LogEvent($"Информация от сервера: {systemInfo}");

                            previousSystemInfo = systemInfo;
                        }
                        else
                        {
                            Console.WriteLine("Данные не изменились, ничего не отправляем.");
                            byte[] noUpdateData = Encoding.UTF8.GetBytes("Данные не изменились.");
                            stream.Write(noUpdateData, 0, noUpdateData.Length);
                        }
                    }
                    else if (request.ToLower() == "load")
                    {
                        string loadInfo = GetGPUInfo();

                        if (loadInfo != previousLoadInfo)
                        {
                            byte[] data = Encoding.UTF8.GetBytes(loadInfo);
                            stream.Write(data, 0, data.Length);
                            Console.WriteLine("Нагрузка на видеокарту отправлена клиенту.");

                            LogServer.LogEvent($"Информация о нагрузке: {loadInfo}");

                            previousLoadInfo = loadInfo;
                        }
                        else
                        {
                            byte[] noUpdateData = Encoding.UTF8.GetBytes("Нагрузка не изменилась.");
                            stream.Write(noUpdateData, 0, noUpdateData.Length);
                        }
                    }
                    else if (request.ToLower().StartsWith("hide"))
                    {
                        if (request.Length <= 5)
                        {
                            string error = "Ошибка: укажите время (например: hide 5000)";
                            byte[] date = Encoding.UTF8.GetBytes(error);
                            stream.Write(date, 0, date.Length);
                            Console.WriteLine(error);
                            continue;
                        }

                        string result = HandleHideCommand(request.Substring(5).Trim());
                        byte[] data = Encoding.UTF8.GetBytes(result);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine(result);
                        LogServer.LogEvent(result);
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

        private static string HandleHideCommand(string millisecondsText)
        {
            if (!int.TryParse(millisecondsText, out int milliseconds) || milliseconds < 1000 || milliseconds > 10000)
            {
                return "Ошибка: параметр должен быть числом от 1000 до 10000";
            }

            try
            {
                IntPtr window = GetConsoleWindow();
                if (window == IntPtr.Zero)
                    return "Ошибка: не удалось получить handle консольного окна";

                ShowWindow(window, SW_HIDE);
                Thread.Sleep(milliseconds);
                ShowWindow(window, SW_SHOW);

                return $"Успех: консоль скрыта на {milliseconds} мс";
            }
            catch (Exception ex)
            {
                return "Ошибка: " + ex.Message;
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static string GetGPUInfo()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "nvidia-smi",
                    Arguments = "--query-gpu=utilization.gpu,memory.used,memory.free --format=csv,noheader,nounits",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var parts = output.Trim().Split(',');

                string gpuUtilization = parts[0].Trim();
                string memoryUsed = parts[1].Trim();
                string memoryFree = parts[2].Trim();

                string formattedOutput = $"Нагрузка на GPU: {gpuUtilization}%\n" +
                                         $"Используемая память: {memoryUsed} MiB\n" +
                                         $"Свободная память: {memoryFree} MiB";

                return formattedOutput;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении информации о видеокарте: " + ex.Message);
                return "Ошибка при получении данных о видеокарте";
            }
        }
    }
}