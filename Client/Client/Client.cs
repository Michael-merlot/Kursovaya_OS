using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

                Timer timer = new Timer(UpdateData, null, 0, 5000);

                Console.WriteLine("Для запроса данных вручную введите 'update'. Для выхода - 'exit'.");
                while (true)
                {
                    string command = Console.ReadLine();
                    if (command.ToLower() == "update")
                    {
                        UpdateData(null);
                    }
                    else if (command.ToLower() == "exit")
                    {
                        break;
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

        private static bool isClientConnected = true;

        private static void UpdateData(object state)
        {
            try
            {
                if (!isClientConnected)
                {
                    Console.WriteLine("Клиент уже отключен, новые запросы не отправляются.");
                    return;
                }

                // Отправляем запрос серверу
                string request = "Запрос на получение системной информации";
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
                if (ex.Message.Contains("Unable to write data to the transport connection"))
                {
                    isClientConnected = false;
                    Console.WriteLine("Соединение с сервером потеряно.");
                    return;
                }
                Console.WriteLine("Ошибка при получении данных: " + ex.Message);
            }
        }

    }
}
