using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace ClientWPF
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => LogServerClient.StartLogging());
            LogServerClient.LogEvent("Приложение ClientWPF запущено");
            UpdateUI();
        }

        private void UpdateUI()
        {
            connectBtn.Content = isConnected ? "Отключиться" : "Подключиться";
            updateBtn.IsEnabled = isConnected;
            loadBtn.IsEnabled = isConnected;
            hideBtn.IsEnabled = isConnected;
        }

        private void UpdateStatus(string message)
        {
            statusText.Text = message;
            LogServerClient.LogEvent(message);
        }

        private async void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                Disconnect();
                return;
            }

            string serverIP = "127.0.0.1";
            int port = serverComboBox.SelectedIndex == 0 ? 12345 : 12346;

            UpdateStatus($"Подключение к серверу на порту {port}...");

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIP, port);
                stream = client.GetStream();
                isConnected = true;
                outputTextBox.AppendText($"Подключено к серверу {port}\n");
                UpdateStatus($"Подключено к серверу на порту {port}. Готов к работе.");
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Ошибка подключения: {ex.Message}\n");
                LogServerClient.LogEvent($"Ошибка подключения: {ex.Message}");
                UpdateStatus("Ошибка подключения к серверу");
            }
            finally
            {
                UpdateUI();
            }
        }

        private void Disconnect()
        {
            try
            {
                stream?.Close();
                client?.Close();
                outputTextBox.AppendText("Отключено от сервера\n");
                UpdateStatus("Отключено от сервера. Для начала работы подключитесь к серверу.");
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Ошибка отключения: {ex.Message}\n");
                LogServerClient.LogEvent($"Ошибка отключения: {ex.Message}");
            }
            finally
            {
                isConnected = false;
                UpdateUI();
            }
        }

        private async void SendCommand(string command, string parameters = "")
        {
            if (!isConnected)
            {
                outputTextBox.AppendText("Нет подключения к серверу\n");
                return;
            }

            try
            {
                string fullCommand = string.IsNullOrEmpty(parameters) ? command : $"{command} {parameters}";
                byte[] data = Encoding.UTF8.GetBytes(fullCommand);
                await stream.WriteAsync(data, 0, data.Length);
                LogServerClient.LogEvent($"Отправлена команда: {fullCommand}");

                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                outputTextBox.AppendText($"{DateTime.Now}: {response}\n");
                LogServerClient.LogEvent($"Получен ответ: {response}");
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Ошибка: {ex.Message}\n");
                LogServerClient.LogEvent($"Ошибка при отправке команды: {ex.Message}");
                Disconnect();
            }
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("update");
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("load");
        }

        private void HideBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HideWindowDialog();
            if (dialog.ShowDialog() == true)
            {
                SendCommand("hide", dialog.SelectedTime.ToString());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            LogServerClient.LogEvent("Приложение ClientWPF завершает работу");
            Disconnect();
            base.OnClosing(e);
        }
    }
}
