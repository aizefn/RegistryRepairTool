using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace RegistryRepairTool.Services
{
    public class RegistryService
    {
        private const string SettingsKey = @"Software\RegistryRepairTool";

        private const string ErrorsFilePath = "registry_errors.json";

        public void SaveErrorsToFile(List<RegistryError> errors)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(errors, options);
                File.WriteAllText(ErrorsFilePath, json);
            }
            catch { /* Логирование ошибок */ }
        }

        public List<RegistryError> LoadErrorsFromFile()
        {
            if (File.Exists(ErrorsFilePath))
            {
                string json = File.ReadAllText(ErrorsFilePath);
                return JsonSerializer.Deserialize<List<RegistryError>>(json) ?? new List<RegistryError>();
            }
            return new List<RegistryError>();
        }

     
        public List<RegistryError> ScanForErrors()
        {
            var errors = new List<RegistryError>();
            CheckInvalidStartupPaths(errors);
            CheckMissingDllReferences(errors);
            CheckObsoleteEntries(errors);

            SaveErrorsToFile(errors); // Сохраняем после сканирования
            return errors;
        }

        public void SaveFirstRun(bool isFirstRun)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SettingsKey))
            {
                key.SetValue("IsFirstRun", isFirstRun ? 0 : 1); // 0 = false, 1 = true (инвертировано для удобства)
            }
        }

        public bool LoadFirstRun()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(SettingsKey))
            {
                return key?.GetValue("IsFirstRun", 1)?.ToString() == "1"; // По умолчанию true (первый запуск)
            }
        }
        public List<string> FindInvalidPathEntries()
        {
            var errors = new List<string>();

            try
            {
                // Пример проверки несуществующего ключа
                using (var key = Registry.CurrentUser.OpenSubKey("Software\\NonexistentPath"))
                {
                    if (key == null)
                    {
                        errors.Add("Не найден ключ: HKEY_CURRENT_USER\\Software\\NonexistentPath");
                    }
                }

                // Дополнительные проверки можно добавить здесь
                // Например, проверка существования файлов по путям из реестра
            }
            catch
            {
                // Ошибки будут обрабатываться в MainViewModel
                throw;
            }

            return errors;
        }

        public bool IsAcceptedConfirmed()
        {
            // Проверяем и лог-файл, и реестр
            bool hasLogFile = File.Exists("acceptance.log");

            using (var key = Registry.CurrentUser.OpenSubKey(SettingsKey))
            {
                bool registryConfirmed = key?.GetValue("Accepted", 0)?.ToString() == "1";
                return registryConfirmed && hasLogFile;
            }
        }

        public void SaveAcceptance()
        {
            // Сохраняем в реестр
            using (var key = Registry.CurrentUser.CreateSubKey(SettingsKey))
            {
                key.SetValue("Accepted", 1);
                key.SetValue("AcceptanceDate", DateTime.Now.ToString("o"));
            }

            // Создаем лог-файл
            try
            {
                File.WriteAllText("acceptance.log", $"Accepted at {DateTime.Now}");
            }
            catch { /* Обработка ошибок */ }
        }
        
        private void CheckMissingDllReferences(List<RegistryError> errors)
        {
            // Проверка App Paths на отсутствующие DLL
            using (var appPaths = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"))
            {
                if (appPaths != null)
                {
                    foreach (var subKeyName in appPaths.GetSubKeyNames())
                    {
                        using (var subKey = appPaths.OpenSubKey(subKeyName))
                        {
                            var path = subKey?.GetValue("")?.ToString();
                            if (!string.IsNullOrEmpty(path) && !File.Exists(path))
                            {
                                errors.Add(new RegistryError
                                {
                                    ErrorName = "MissingDLL",
                                    Description = $"Отсутствует DLL/EXE: {Path.GetFileName(path)}",
                                    RegistryPath = $"HKLM\\SOFTWARE\\...\\App Paths\\{subKeyName}",
                                    Severity = ErrorSeverity.High
                                });
                            }
                        }
                    }
                }
            }
        }
        private void CheckObsoleteEntries(List<RegistryError> errors)
        {
            // Проверка устаревших записей от удаленных программ
            string[] uninstallKeys = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

            foreach (var keyPath in uninstallKeys)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null) continue;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            var displayName = subKey?.GetValue("DisplayName")?.ToString();
                            var installLocation = subKey?.GetValue("InstallLocation")?.ToString();

                            if (!string.IsNullOrEmpty(installLocation) && !Directory.Exists(installLocation))
                            {
                                errors.Add(new RegistryError
                                {
                                    ErrorName = "ObsoleteEntry",
                                    Description = $"Устаревшая запись: {displayName ?? subKeyName}",
                                    RegistryPath = $"HKLM\\{keyPath}\\{subKeyName}",
                                    Severity = ErrorSeverity.Medium
                                });
                            }
                        }
                    }
                }
            }
        }

        public bool TryFixError(RegistryError error)
        {
            try
            {
                switch (error.ErrorName)
                {
                    case "InvalidStartupPath":
                        return RemoveInvalidStartupEntry(error.RegistryPath);
                    // Добавьте другие типы ошибок
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveInvalidStartupEntry(string registryPath)
        {
            var parts = registryPath.Split('\\');
            var keyPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));
            var valueName = parts.Last();

            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue(valueName);
                    return true;
                }
            }
            return false;
        }

        private void CheckInvalidStartupPaths(List<RegistryError> errors)
        {
            string[] startupKeys = {
        @"Software\Microsoft\Windows\CurrentVersion\Run",
        @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
            };

            foreach (var keyPath in startupKeys)
            {
                using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    if (key == null) continue;

                    foreach (var valueName in key.GetValueNames())
                    {
                        var path = key.GetValue(valueName)?.ToString();
                        if (!File.Exists(path))
                        {
                            errors.Add(new RegistryError
                            {
                                ErrorName = "InvalidStartupPath",
                                Description = $"Несуществующий файл в автозагрузке: {path}",
                                RegistryPath = $"HKCU\\{keyPath}\\{valueName}"
                            });
                        }
                    }
                }
            }
        }

    }
}