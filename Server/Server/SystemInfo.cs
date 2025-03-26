using System;
using System.Diagnostics;

namespace Server
{
    public class SystemInfo
    {
        public static string GetSystemInfo()
        {
            string cpuInfo = GetCommandOutput("lscpu");
            string memoryInfo = GetCommandOutput("free -h");
            string gpuInfo = GetCommandOutput("nvidia-smi");

            return $"Процессор:\n{cpuInfo}\n\nПамять:\n{memoryInfo}\n\nВидеокарта:\n{gpuInfo}";
        }

        private static string GetCommandOutput(string command)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{command}\"", 
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(processStartInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
            catch (Exception ex)
            {
                return $"Ошибка при выполнении команды: {ex.Message}";
            }
        }
    }
}
