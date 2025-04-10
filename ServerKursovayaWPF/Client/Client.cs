using System;
using System.Net.Sockets;
using System.Text;
using System.IO;


namespace Client
{
    public class Client
    {
        private static readonly string serverIP = "127.0.0.1";
        private static readonly int port = 12345;
        private static TcpClient client;
        private static NetworkStream stream;

        public static void StartClient()
        {
            try
            {
                client = new TcpClient(serverIP, port);
                stream = client.GetStream();

                Console.WriteLine("Для запроса данных вручную введите 'update'. Для получения нагрузки видеокарты введите 'load'. Для выхода - 'exit'.");
                while (true)
                {
                    string command = Console.ReadLine();
                    if (command.ToLower() == "update")
                    {
                        UpdateData();
                    }
                    else if (command.ToLower() == "load")
                    {
                        GetLoadInfo();
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
    }
}
