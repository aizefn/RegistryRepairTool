using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using RegistryRepairTool.ViewModels;

public class SystemInfoViewModel : ObservableObject
{
    private string _windowsVersion = "Определение...";
    private string _systemType = "Определение...";
    private bool _hasError;

    public string WindowsVersion
    {
        get => _windowsVersion;
        set => SetProperty(ref _windowsVersion, value);
    }

    public string SystemType
    {
        get => _systemType;
        set => SetProperty(ref _systemType, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public SystemInfoViewModel()
    {
        LoadSystemInfo();
    }

    private void LoadSystemInfo()
    {
        try
        {
            WindowsVersion = GetRealWindowsVersion();
            SystemType = Environment.Is64BitOperatingSystem ? "64-битная система" : "32-битная система";
            HasError = false;

            Debug.WriteLine($"Определено: {WindowsVersion} | {SystemType}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при получении системной информации: {ex}");
            WindowsVersion = "Не удалось определить версию Windows";
            SystemType = "Не удалось определить разрядность";
            HasError = true;
        }
    }

    public string GetRealWindowsVersion()
    {
        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                if (key != null)
                {
                    // 1. Проверяем номер сборки (Windows 11 начинается с 22000)
                    if (int.TryParse(key.GetValue("CurrentBuild")?.ToString(), out int buildNumber))
                    {
                        if (buildNumber >= 22000) // Windows 11 начинается с этой сборки
                        {
                            string version = "Windows 11";

                            // Добавляем версию (22H2, 23H2 и т.д.)
                            var displayVersion = key.GetValue("DisplayVersion")?.ToString();
                            if (!string.IsNullOrEmpty(displayVersion))
                                version += $" {displayVersion}";

                            // Добавляем номер сборки
                            version += $" (Build {buildNumber}";

                            // Добавляем номер обновления (UBR)
                            var ubr = key.GetValue("UBR")?.ToString();
                            if (!string.IsNullOrEmpty(ubr))
                                version += $".{ubr}";

                            version += ")";
                            return version;
                        }
                    }

                    // 2. Проверяем наличие специфичных для Windows 11 ключей
                    var productName = key.GetValue("ProductName")?.ToString();
                    if (productName?.Contains("Windows 11") == true)
                        return productName;
                }
            }
        }
        catch { }

        // Альтернативные методы определения
        return GetWindowsVersionFallback();
    }

    private string GetWindowsVersionFallback()
    {
        // Метод 1: Через WMI
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject os in searcher.Get())
                {
                    string caption = os["Caption"].ToString();
                    if (caption.Contains("Windows 11"))
                        return caption;
                }
            }
        }
        catch { }

        // Метод 2: Через системные файлы
        try
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(
                Environment.SystemDirectory, "kernel32.dll"));

            if (fileVersion.FileMajorPart >= 10 && fileVersion.FileBuildPart >= 22000)
                return $"Windows 11 (Build {fileVersion.FileBuildPart})";
        }
        catch { }

        // Последний fallback
        return "Windows (версия не определена)";
    }
}