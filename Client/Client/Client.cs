using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Client
{
    public class Client
    {
        private static string serverIP = "127.0.0.1";
        private static int port = 12345; 
        private static TcpClient client;
        private static NetworkStream stream;

        public static void StartClient()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Выберите сервер для подключения (1 или 2), или введите 'exit' для выхода:");
                    string serverChoice = Console.ReadLine().ToLower();
                    if (serverChoice == "exit")
                    {
                        break;
                    }
                    else if (serverChoice == "1")
                    {
                        port = 12345; 
                    }
                    else if (serverChoice == "2")
                    {
                        port = 12346;
                    }
                    else
                    {
                        Console.WriteLine("Неверный выбор, попробуйте снова.");
                        continue;
                    }

                    client = new TcpClient(serverIP, port);
                    stream = client.GetStream();
                    Console.WriteLine($"Подключение к серверу на порту {port} успешно.");

                    while (true)
                    {
                        Console.WriteLine("Для запроса данных вручную введите 'update'. Для получения нагрузки видеокарты введите 'load'. Для скрытия окна сервера - 'hide'. Для выхода - 'exit'.");
                        string command = Console.ReadLine();
                        if (command.ToLower() == "update")
                        {
                            UpdateData();
                        }
                        else if (command.ToLower() == "load")
                        {
                            GetLoadInfo();
                        }
                        else if (command.ToLower() == "hide")
                        {
                            HideWindow();
                        }
                        else if (command.ToLower() == "exit")
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Неизвестная команда.");
                        }
                    }

                    client.Close();
                    Console.WriteLine("Соединение закрыто.");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Ошибка подключения к серверу: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private static void UpdateData()
        {
            try
            {
                string request = "update";
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Информация от сервера: " + response);

                string confirmation = "Данные получены";
                byte[] confirmationData = Encoding.UTF8.GetBytes(confirmation);
                stream.Write(confirmationData, 0, confirmationData.Length);
                Console.WriteLine("Подтверждение отправлено серверу.");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка при получении данных: " + ex.Message);
            }
        }

        private static void GetLoadInfo()
        {
            try
            {
                string request = "load";
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Изменения от сервера: " + response);

                string confirmation = "Подтверждение нагрузки получено";
                byte[] confirmationData = Encoding.UTF8.GetBytes(confirmation);
                stream.Write(confirmationData, 0, confirmationData.Length);
                Console.WriteLine("Подтверждение отправлено серверу.");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка при получении данных: " + ex.Message);
            }
        }
        private static void HideWindow()
        {
            try
            {
                int time;
                while (true)
                {
                    Console.WriteLine("Введите время скрытия (1000-10000 мс):");
                    if (int.TryParse(Console.ReadLine(), out time) && time >= 1000 && time <= 10000)
                        break;
                    Console.WriteLine("Ошибка! Введите число от 1000 до 10000");
                }

                string request = "hide " + time;
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Результат: " + response);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }
    }
}
