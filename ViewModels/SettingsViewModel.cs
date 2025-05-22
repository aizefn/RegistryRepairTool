using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RegistryRepairTool.Models;
using RegistryRepairTool.Services;
using System.Windows.Forms;

namespace RegistryRepairTool.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly RegistryService _registryService;
        private readonly SettingsModel _settings;

        // Настройки привязки
        private bool _runAtStartup;
        private bool _autoScanOnStartup;
        private bool _showNotificationAfterFix;
        private bool _playSoundOnScanComplete;
        private bool _saveLogsToFile;
        private bool _useDefaultLogPath = true;
        private bool _useCustomLogPath;
        private string _customLogPath;
        private string _backupLocation;
        public SettingsModel Settings { get; }
        private static SettingsViewModel _instance;
        public static SettingsViewModel Instance => _instance ??= new SettingsViewModel();
        public SettingsViewModel()
        {
            _registryService = new RegistryService();
            _settings = _registryService.LoadAllSettings(); // Используем один экземпляр настроек
            Settings = _settings; // Убедитесь, что Settings ссылается на _settings

           

            LoadSettings();
            SaveCommand = new RelayCommand(SaveSettings);
            // Инициализация команд с явным указанием методов
            ResetToDefaultCommand = new RelayCommand(() => ResetToDefault());
            BrowseLogFolderCommand = new RelayCommand(() => BrowseLogFolder());
            BrowseBackupFolderCommand = new RelayCommand(() => BrowseBackupFolder());
        }

        public ICommand SaveCommand { get; }
        public ICommand ResetToDefaultCommand { get; }
        public ICommand BrowseLogFolderCommand { get; }
        public ICommand BrowseBackupFolderCommand { get; }

        #region Свойства для привязки

        public bool RunAtStartup
        {
            get => _runAtStartup;
            set => SetProperty(ref _runAtStartup, value);
        }

        public bool AutoScanOnStartup
        {
            get => _autoScanOnStartup;
            set => SetProperty(ref _autoScanOnStartup, value);
        }

        public bool ShowNotificationAfterFix
        {
            get => _showNotificationAfterFix;
            set => SetProperty(ref _showNotificationAfterFix, value);
        }

        public bool PlaySoundOnScanComplete
        {
            get => _playSoundOnScanComplete;
            set => SetProperty(ref _playSoundOnScanComplete, value);
        }

        public bool SaveLogsToFile
        {
            get => _saveLogsToFile;
            set => SetProperty(ref _saveLogsToFile, value);
        }

        public bool UseDefaultLogPath
        {
            get => _useDefaultLogPath;
            set
            {
                SetProperty(ref _useDefaultLogPath, value);
                if (value) UseCustomLogPath = false;
            }
        }

        public bool UseCustomLogPath
        {
            get => _useCustomLogPath;
            set
            {
                SetProperty(ref _useCustomLogPath, value);
                if (value) UseDefaultLogPath = false;
            }
        }

        public string CustomLogPath
        {
            get => _customLogPath;
            set => SetProperty(ref _customLogPath, value);
        }

        public string BackupLocation
        {
            get => _backupLocation;
            set => SetProperty(ref _backupLocation, value);
        }

        #endregion

        private void LoadSettings()
        {
            try
            {
                RunAtStartup = _registryService.LoadStartupSetting();
                AutoScanOnStartup = _registryService.GetRegistryValue("AutoScanOnStartup", 1) == 1;
                ShowNotificationAfterFix = _registryService.GetRegistryValue("ShowNotificationAfterFix", 1) == 1;
                PlaySoundOnScanComplete = _registryService.GetRegistryValue("PlaySoundOnScanComplete", 1) == 1;
                SaveLogsToFile = _registryService.GetRegistryValue("SaveLogsToFile", 1) == 1;
                UseDefaultLogPath = _registryService.GetRegistryValue("UseDefaultLogPath", 1) == 1;
                CustomLogPath = _registryService.GetRegistryValue("CustomLogPath", "").ToString();
                BackupLocation = _registryService.GetRegistryValue("BackupLocation", "Backups").ToString();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Обновляем модель текущими значениями
                _settings.RunAtStartup = RunAtStartup;
                _settings.AutoScanOnStartup = AutoScanOnStartup;
                _settings.ShowNotificationAfterFix = ShowNotificationAfterFix;
                _settings.PlaySoundOnScanComplete = PlaySoundOnScanComplete;
                _settings.SaveLogsToFile = SaveLogsToFile;
                _settings.UseDefaultLogPath = UseDefaultLogPath;
                _settings.CustomLogPath = CustomLogPath;
                _settings.BackupLocation = BackupLocation;

                // Сохраняем все настройки
                _registryService.SaveAllSettings(_settings); // Добавьте этот метод в RegistryService
                _registryService.SaveStartupSetting(RunAtStartup);

                System.Windows.MessageBox.Show("Настройки успешно сохранены!", "Сохранение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ResetToDefault()
        {
            if (System.Windows.MessageBox.Show("Сбросить все настройки к значениям по умолчанию?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                RunAtStartup = false;
                AutoScanOnStartup = true;
                ShowNotificationAfterFix = true;
                PlaySoundOnScanComplete = true;
                SaveLogsToFile = true;
                UseDefaultLogPath = true;
                CustomLogPath = "";
                BackupLocation = "Backups";

                // Сохраняем сброшенные настройки
                SaveSettings();
            }
        }

    

        private void BrowseLogFolder()
        {
            try
            {
                var dialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку для сохранения логов",
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CustomLogPath = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка выбора папки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseBackupFolder()
        {
            try
            {
                var dialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку для бэкапов",
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    BackupLocation = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка выбора папки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}