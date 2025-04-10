using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace WpfClient
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private Process serverProcess;

        public MainWindow()
        {
            InitializeComponent();
            StartClient();
            StartServerProcess(); // Запускаем серверный процесс
        }

        private void StartClient()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 12345); // Подключение к серверу
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                LogTextBox.Text = "Ошибка подключения: " + ex.Message;
            }
        }

        private void StartServerProcess()
        {
            try
            {
                // Запускаем сервер в отдельном процессе
                serverProcess = new Process();
                serverProcess.StartInfo.FileName = "dotnet";
                serverProcess.StartInfo.Arguments = @"..\Server\Server.csproj";

                serverProcess.StartInfo.RedirectStandardOutput = true;
                serverProcess.StartInfo.UseShellExecute = false;
                serverProcess.StartInfo.CreateNoWindow = true;

                // Событие для получения вывода из консоли
                serverProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogTextBox.Text += e.Data + Environment.NewLine; // Добавляем вывод в TextBox
                        });
                    }
                };

                serverProcess.Start();
                serverProcess.BeginOutputReadLine(); // Начинаем захват вывода
            }
            catch (Exception ex)
            {
                LogTextBox.Text = "Ошибка при запуске сервера: " + ex.Message;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToServer("update");
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToServer("load");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToServer("exit");
            client.Close();
            Application.Current.Shutdown(); // Закрытие приложения
        }

        private void SendCommandToServer(string command)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(command);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                LogTextBox.Text = response;
            }
            catch (IOException ex)
            {
                LogTextBox.Text = "Ошибка: " + ex.Message;
            }
        }
    }
}
