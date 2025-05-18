using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RegistryRepairTool.Models.RegistryRepairTool.Models;

namespace RegistryRepairTool.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly SettingsModel _settings;

        public SettingsViewModel()
        {
            _settings = new SettingsModel();

            SaveCommand = new RelayCommand(SaveSettings);
            CancelCommand = new RelayCommand(CancelChanges);
            BrowseFolderCommand = new RelayCommand(BrowseBackupFolder);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public bool ScanSystemRegistry
        {
            get => _settings.ScanSystemRegistry;
            set => _settings.ScanSystemRegistry = value;
        }

        // Добавьте остальные свойства аналогично

        private void SaveSettings()
        {
            // Сохранение настроек в конфиг файл или реестр
            // Например: Properties.Settings.Default.Save();
        }

        private void CancelChanges()
        {
            // Восстановление настроек
        }

        private void BrowseBackupFolder()
        {
            // Логика выбора папки для бэкапов
        }
    }
}
