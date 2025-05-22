using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.Win32;
using RegistryRepairTool.Models;

namespace RegistryRepairTool.Services
{
    public class RegistryService
    {
        private const string SettingsKey = @"Software\RegistryRepairTool";
        private const string RegistryKeyPath = @"SOFTWARE\RegistryRepairTool";

        private const string ErrorsFilePath = "registry_errors.json";
        public T GetRegistryValue<T>(string valueName, T defaultValue)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key == null) return defaultValue;

                    var value = key.GetValue(valueName);
                    return value != null ? (T)Convert.ChangeType(value, typeof(T)) : defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        // Добавить в класс RegistryService
        public void SaveAllSettings(SettingsModel settings)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SettingsKey))
            {
                key.SetValue("RunAtStartup", settings.RunAtStartup ? 1 : 0);
                key.SetValue("AutoScanOnStartup", settings.AutoScanOnStartup ? 1 : 0);
                key.SetValue("ShowNotificationAfterFix", settings.ShowNotificationAfterFix ? 1 : 0);
                key.SetValue("PlaySoundOnScanComplete", settings.PlaySoundOnScanComplete ? 1 : 0);
                key.SetValue("SaveLogsToFile", settings.SaveLogsToFile ? 1 : 0);
                key.SetValue("UseDefaultLogPath", settings.UseDefaultLogPath ? 1 : 0);
                key.SetValue("CustomLogPath", settings.CustomLogPath ?? "");
                key.SetValue("BackupLocation", settings.BackupLocation ?? "Backups");
            }

            // Сохраняем настройки автозагрузки отдельно
            SetStartup(settings.RunAtStartup);
        }

        public SettingsModel LoadAllSettings()
        {
            var settings = new SettingsModel();

            using (var key = Registry.CurrentUser.OpenSubKey(SettingsKey))
            {
                if (key != null)
                {
                    settings.RunAtStartup = key.GetValue("RunAtStartup", 0).ToString() == "1";
                    settings.AutoScanOnStartup = key.GetValue("AutoScanOnStartup", 1).ToString() == "1";
                    settings.ShowNotificationAfterFix = key.GetValue("ShowNotificationAfterFix", 1).ToString() == "1";
                    settings.PlaySoundOnScanComplete = key.GetValue("PlaySoundOnScanComplete", 1).ToString() == "1";
                    settings.SaveLogsToFile = key.GetValue("SaveLogsToFile", 1).ToString() == "1";
                    settings.UseDefaultLogPath = key.GetValue("UseDefaultLogPath", 1).ToString() == "1";
                    settings.CustomLogPath = key.GetValue("CustomLogPath", "").ToString();
                    settings.BackupLocation = key.GetValue("BackupLocation", "Backups").ToString();
                }
            }

            // Проверяем настройки автозагрузки
            settings.RunAtStartup = LoadStartupSetting();

            return settings;
        }

        public void SetRegistryValue(string valueName, object value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    key.SetValue(valueName, value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка записи в реестр: {ex.Message}");
            }
        }

        // Управление автозагрузкой
        public void SetStartup(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        key.SetValue("RegistryRepairTool",
                            Process.GetCurrentProcess().MainModule.FileName);
                    }
                    else
                    {
                        key.DeleteValue("RegistryRepairTool", false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка настройки автозагрузки: {ex.Message}");
            }
        }
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
        public void SaveStartupSetting(bool runAtStartup)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (runAtStartup)
                {
                    key.SetValue("RegistryRepairTool", Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    key.DeleteValue("RegistryRepairTool", false);
                }
            }
        }

        public bool LoadStartupSetting()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                return key?.GetValue("RegistryRepairTool") != null;
            }
        }

        public List<RegistryError> ScanForErrors()
        {
            var errors = new List<RegistryError>();

            // Загружаем ранее исправленные ошибки
            var fixedErrors = LoadErrorsFromFile().Where(e => e.IsFixed).ToList();

            CheckInvalidStartupPaths(errors);
            CheckMissingDllReferences(errors);
            CheckObsoleteEntries(errors);

            // Исключаем ошибки, которые были исправлены
            errors.RemoveAll(e => fixedErrors.Any(f =>
                f.RegistryPath == e.RegistryPath &&
                f.ErrorName == e.ErrorName));

            SaveErrorsToFile(errors);
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




        public bool TryFixError(RegistryError error)
        {
            if (error == null)
            {
                Debug.WriteLine("Ошибка: передан null");
                return false;
            }

            // Логирование попытки исправления
            Debug.WriteLine($"Попытка исправить {error.ErrorName} в {error.RegistryPath}");

            // Проверка прав для HKLM
            if (error.RegistryPath.StartsWith("HKLM") && !HasAdminRights())
            {
                Debug.WriteLine("Требуются права администратора для HKLM");
                return false;
            }

            try
            {
                bool result = false;

                switch (error.ErrorType)
                {
                    case "Registry":
                        result = HandleRegistryError(error);
                        break;
                    case "FileSystem":
                        result = HandleFileSystemError(error);
                        break;
                }

                if (result)
                {
                    // Проверяем, действительно ли ошибка исправлена
                    bool stillExists = VerifyErrorStillExists(error);
                    Debug.WriteLine($"Результат проверки: {(stillExists ? "ошибка осталась" : "ошибка исправлена")}");

                    if (!stillExists)
                    {
                        error.IsFixed = true;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Исключение при исправлении: {ex.Message}");
                return false;
            }
        }
        private bool HandleFileSystemError(RegistryError error)
        {
            // Заглушка для обработки файловых ошибок
            // Можно реализовать позже
            return false;
        }
        // Изменяем с private на public
        public bool HasAdminRights()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyErrorStillExists(RegistryError error)
        {
            try
            {
                switch (error.ErrorName)
                {
                    case "MissingDLL":
                        return CheckAppPathExists(error.RegistryPath);

                    case "ObsoleteEntry":
                        return CheckUninstallEntryExists(error.RegistryPath);

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckAppPathExists(string registryPath)
        {
            var parts = registryPath.Split('\\');
            string subKeyName = parts.Last();
            string parentPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));

            // Check main registry
            using (var parentKey = Registry.LocalMachine.OpenSubKey(parentPath))
            {
                if (parentKey?.GetSubKeyNames().Contains(subKeyName) == true)
                    return true;
            }

            // Check Wow6432Node
            using (var parentKey = Registry.LocalMachine.OpenSubKey(
                parentPath.Replace("SOFTWARE\\", "SOFTWARE\\Wow6432Node\\")))
            {
                if (parentKey?.GetSubKeyNames().Contains(subKeyName) == true)
                    return true;
            }

            return false;
        }

        private bool CheckUninstallEntryExists(string registryPath)
        {
            var parts = registryPath.Split('\\');
            using (var key = Registry.LocalMachine.OpenSubKey(
                string.Join("\\", parts.Skip(1))))
            {
                return key != null;
            }
        }

        private void CheckMissingDllReferences(List<RegistryError> errors)
        {
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
                                    ErrorType = "Registry",
                                    Description = $"Отсутствует DLL/EXE: {Path.GetFileName(path)}",
                                    RegistryPath = $"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\{subKeyName}",
                                    Severity = ErrorSeverity.High
                                });
                            }
                        }
                    }
                }
            }
        }

        private bool HandleRegistryError(RegistryError error)
        {
            try
            {
                switch (error.ErrorName)
                {
                    case "MissingDLL":
                        return RemoveAppPathEntry(error.RegistryPath);

                    case "InvalidStartupPath":
                        return RemoveStartupEntry(error.RegistryPath);

                    case "ObsoleteEntry":
                        return RemoveUninstallEntry(error.RegistryPath);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling {error.ErrorName}: {ex.Message}");
                return false;
            }
        }

        private bool RemoveAppPathEntry(string registryPath)
        {
            try
            {
                // Example registryPath: "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\appname.exe"
                var parts = registryPath.Split('\\');
                if (parts.Length < 2) return false;

                string subKeyName = parts.Last();
                string parentPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));

                // Delete from main registry
                bool mainDeleted = DeleteRegistrySubKey(Registry.LocalMachine, parentPath, subKeyName);

                // Delete from Wow6432Node if exists (for 32-bit apps on 64-bit system)
                bool wowDeleted = DeleteRegistrySubKey(
                    Registry.LocalMachine,
                    parentPath.Replace("SOFTWARE\\", "SOFTWARE\\Wow6432Node\\"),
                    subKeyName);

                return mainDeleted || wowDeleted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing AppPath entry: {ex.Message}");
                return false;
            }
        }

        private bool DeleteRegistrySubKey(RegistryKey rootKey, string parentPath, string subKeyName)
        {
            try
            {
                using (var parentKey = rootKey.OpenSubKey(parentPath, true))
                {
                    if (parentKey == null) return false;

                    parentKey.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteRegistryValue(RegistryKey rootKey, string keyPath, string valueName)
        {
            try
            {
                using (var key = rootKey?.OpenSubKey(keyPath, true))
                {
                    if (key == null) return false;

                    if (key.GetValue(valueName) == null) return false;

                    key.DeleteValue(valueName);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveUninstallEntry(string registryPath)
        {
            try
            {
                var parts = registryPath.Split('\\');
                if (parts.Length < 2) return false;

                string fullPath = string.Join("\\", parts.Skip(1));
                Registry.LocalMachine.DeleteSubKeyTree(fullPath, throwOnMissingSubKey: false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing Uninstall entry: {ex.Message}");
                return false;
            }
        }

        private bool RemoveStartupEntry(string registryPath)
        {
            try
            {
                var parts = registryPath.Split('\\');
                if (parts.Length < 2) return false;

                string valueName = parts.Last();
                string keyPath = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));

                using (var key = Registry.CurrentUser.OpenSubKey(keyPath, true))
                {
                    if (key == null) return false;

                    key.DeleteValue(valueName, throwOnMissingValue: false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing startup entry: {ex.Message}");
                return false;
            }
        }


        // Обновите методы создания ошибок, чтобы они включали ErrorType:
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
                                ErrorType = "Registry",
                                Description = $"Несуществующий файл в автозагрузке: {path}",
                                RegistryPath = $"HKCU\\{keyPath}\\{valueName}",
                                Severity = ErrorSeverity.High
                            });
                        }
                    }
                }
            }
        }

      

        private void CheckObsoleteEntries(List<RegistryError> errors)
        {
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
                                    ErrorType = "Registry",
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
    }
}