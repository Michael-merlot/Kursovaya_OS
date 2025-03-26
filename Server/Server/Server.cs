using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class Server
    {
        private static readonly int port = 12345;

        public static void StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Сервер запущен на порту {port}");

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Новый клиент подключился");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
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
                Console.WriteLine("Ожидаем запрос от клиента...");
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получен запрос: " + request);

                string systemInfo = SystemInfo.GetSystemInfo();

                Console.WriteLine("Отправляем информацию клиенту...");
                byte[] data = Encoding.UTF8.GetBytes(systemInfo);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Информация отправлена клиенту.");

                Console.WriteLine("Ожидаем подтверждения от клиента...");
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string confirmation = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Получено подтверждение от клиента: " + confirmation);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Ошибка при обработке клиента: " + ex.Message);
            }
            finally
            {
                client.Close();
                Console.WriteLine("Клиент отключен");
            }
        }

    }
}
