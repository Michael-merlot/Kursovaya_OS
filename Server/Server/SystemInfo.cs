using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace Server
{
    public static class SystemInfo
    {
        public static string GetSystemInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== Системная информация ===");
            sb.AppendLine($"Время: {DateTime.Now}\n");

            // Информация о процессоре
            sb.AppendLine("[Процессор]");
            sb.AppendLine(GetCpuInfo().Trim());

            // Информация о памяти
            sb.AppendLine("\n[Память]");
            sb.AppendLine(GetMemoryInfo().Trim());

            // Информация о видеокарте
            sb.AppendLine("\n[Видеокарта]");
            sb.AppendLine(GetGpuInfo().Trim());

            // Информация о системе
            sb.AppendLine("\n[Система]");
            sb.AppendLine(GetSystemDetails().Trim());

            // Информация о дисках
            sb.AppendLine("\n[Диски]");
            sb.AppendLine(GetDiskInfo().Trim());

            return sb.ToString();
        }

        private static string GetCpuInfo()
        {
            try
            {
                var sb = new StringBuilder();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var cpuInfo = ExecuteCommand("wmic cpu get caption,deviceid,name,numberofcores,maxclockspeed,status,numberoflogicalprocessors /format:list");
                    sb.AppendLine(cpuInfo);

                    var cpuLoad = ExecuteCommand("wmic cpu get loadpercentage /format:list");
                    sb.AppendLine(cpuLoad);
                }
                else
                {
                    var cpuInfo = ExecuteCommand("lscpu");
                    sb.AppendLine(cpuInfo);

                    var cpuLoad = ExecuteCommand("top -bn1 | grep 'Cpu(s)'");
                    sb.AppendLine(cpuLoad);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        private static string GetMemoryInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var memInfo = ExecuteCommand("wmic OS get FreePhysicalMemory,TotalVisibleMemorySize,TotalVirtualMemorySize,FreeVirtualMemory /format:list");
                    return memInfo;
                }
                else
                {
                    return ExecuteCommand("free -m");
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        private static string GetGpuInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var gpuInfo = ExecuteCommand("wmic path win32_VideoController get name,adapterram,driverversion,adapterdactype,videoprocessor /format:list");
                    return gpuInfo;
                }
                else
                {
                    return ExecuteCommand("nvidia-smi") ?? "Информация о GPU недоступна";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        private static string GetSystemDetails()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ExecuteCommand("systeminfo | findstr /B /C:\"OS Name\" /C:\"OS Version\" /C:\"System Manufacturer\" /C:\"System Model\"");
                }
                else
                {
                    return ExecuteCommand("uname -a") + "\n" + ExecuteCommand("lsb_release -a");
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        private static string GetDiskInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ExecuteCommand("wmic diskdrive get model,size,interfaceType /format:list") + "\n" +
                           ExecuteCommand("wmic logicaldisk get size,freespace,caption /format:list");
                }
                else
                {
                    return ExecuteCommand("df -h") + "\n" + ExecuteCommand("lsblk");
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        private static string ExecuteCommand(string command)
        {
            try
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {command}" : $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Убираем лишние пустые строки для Windows
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    output = output.Replace("\r\r", "\r"); // Убираем дублированные \r
                    string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    var cleanedLines = new List<string>();

                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            cleanedLines.Add(line.Trim());
                        }
                    }

                    output = string.Join("\n", cleanedLines);
                }

                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}