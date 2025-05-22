using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using RegistryRepairTool.Models;
using RegistryRepairTool.Services;
using RegistryRepairTool.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RegistryRepairTool.ViewModels
{
    public class AutoFixViewModel : ObservableObject
    {
        private static AutoFixViewModel _instance;
        private readonly RegistryService _registryService;
        private readonly SettingsModel _settings;
        public static AutoFixViewModel Instance => _instance ??= new AutoFixViewModel();
        public ObservableCollection<RegistryError> RegistryErrors { get; } = new();

        public ICommand ScanRegistryCommand { get; }
        public ICommand FixAllCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand FixSelectedCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand ClearAllErrorsCommand { get; }

        public int SelectedErrorsCount => RegistryErrors?.Count(e => e.IsSelected) ?? 0;
        public int FixedErrorsCount => RegistryErrors?.Count(e => e.IsFixed) ?? 0;
        
        private AutoFixViewModel()
        {
            _registryService = new RegistryService();
            _settings = SettingsViewModel.Instance?.Settings;
            if (_settings == null)
            {
                Debug.WriteLine("Settings are not initialized!");
                return;
            }
            // Загружаем сохраненные ошибки
            var savedErrors = _registryService.LoadErrorsFromFile();
            foreach (var error in savedErrors)
            {
                RegistryErrors.Add(error);
            }

            // Команды с проверкой прав
            ScanRegistryCommand = new RelayCommand(ScanRegistry);
            FixAllCommand = new RelayCommand(() => { if (CheckAdminRightsWithMessage()) FixAllErrors(); });
            SelectAllCommand = new RelayCommand(SelectAllErrors);
            FixSelectedCommand = new RelayCommand(
                () => { if (CheckAdminRightsWithMessage()) FixSelectedErrors(); },
                CanFixSelected);
            DeselectAllCommand = new RelayCommand(DeselectAllErrors);
            ClearAllErrorsCommand = new RelayCommand(ClearAllErrors);

        }

        public void InitializeAutoScan()
        {
            if (SettingsViewModel.Instance.Settings.AutoScanOnStartup)
            {
                ScanRegistry();
            }
        }
        private void DeselectAllErrors()
        {
            foreach (var error in RegistryErrors.Where(e => e.IsSelected))
            {
                error.IsSelected = false;
            }
            SelectAllButtonContent = "Выбрать все";
            OnPropertyChanged(nameof(SelectedErrorsCount));
            CommandManager.InvalidateRequerySuggested(); // Добавить эту строку
        }

        private void SelectAllErrors()
        {
            bool allSelected = RegistryErrors.All(e => e.IsSelected);
            foreach (var error in RegistryErrors)
            {
                error.IsSelected = !allSelected;
            }
            SelectAllButtonContent = allSelected ? "Выбрать все" : "Снять выделение";
            OnPropertyChanged(nameof(SelectedErrorsCount));
            CommandManager.InvalidateRequerySuggested(); // Добавить эту строку
        }


        // Добавьте метод для очистки
        public void ClearAllErrors()
        {
            RegistryErrors.Clear();
            _registryService.SaveErrorsToFile(new List<RegistryError>());
            OnPropertyChanged(nameof(SelectedErrorsCount));
            OnPropertyChanged(nameof(FixedErrorsCount));
        }
        private string _selectAllButtonContent = "Выбрать все";
        public string SelectAllButtonContent
        {
            get => _selectAllButtonContent;
            set => SetProperty(ref _selectAllButtonContent, value);
        }

        private void ScanRegistry()
        {
            RegistryErrors.Clear();
            var errors = _registryService.ScanForErrors();

            foreach (var error in errors)
            {
                RegistryErrors.Add(error);
                error.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(RegistryError.IsSelected) ||
                        e.PropertyName == nameof(RegistryError.IsFixed))
                    {
                        OnPropertyChanged(nameof(SelectedErrorsCount));
                        OnPropertyChanged(nameof(FixedErrorsCount));
                        CommandManager.InvalidateRequerySuggested(); // Добавить эту строку
                    }
                };
            }

            OnPropertyChanged(nameof(SelectedErrorsCount));
            OnPropertyChanged(nameof(FixedErrorsCount));
            CommandManager.InvalidateRequerySuggested(); // Добавить эту строку
                                                         // Применяем настройки автоматизации
                                                         // Применяем настройку звука
            if (_settings.PlaySoundOnScanComplete)
            {
                SystemSounds.Beep.Play();
            }
        }

        private void FixAllErrors()
        {
            foreach (var error in RegistryErrors.Where(e => !e.IsFixed))
            {
                if (_registryService.TryFixError(error))
                {
                    error.IsFixed = true;
                }
            }
            OnPropertyChanged(nameof(FixedErrorsCount));
        }




        private void FixSelectedErrors()
        {
            // Проверяем права администратора для HKLM ошибок
            bool needsAdmin = RegistryErrors.Any(e => e.IsSelected && !e.IsFixed && e.RegistryPath.StartsWith("HKLM"));
            if (needsAdmin && !_registryService.HasAdminRights())
            {
                System.Windows.MessageBox.Show(
                    "Для исправления системных ошибок реестра требуются права администратора.\n\n" +
                    "Пожалуйста, закройте программу и запустите её снова от имени администратора.",
                    "Требуются права администратора",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            var fixedErrors = new List<RegistryError>();

            foreach (var error in RegistryErrors.Where(e => e.IsSelected && !e.IsFixed).ToList())
            {
                Debug.WriteLine($"Попытка исправить: {error.RegistryPath}");
                if (_registryService.TryFixError(error))
                {
                    error.IsFixed = true;
                    error.IsSelected = false;
                    fixedErrors.Add(error);
                    Debug.WriteLine($"Успешно исправлено: {error.RegistryPath}");
                }
                else
                {
                    Debug.WriteLine($"Не удалось исправить: {error.RegistryPath}");
                }
            }

            // Обновляем файл ошибок
            if (fixedErrors.Any())
            {
                var currentErrors = _registryService.LoadErrorsFromFile();
                currentErrors.RemoveAll(e => fixedErrors.Any(f => f.RegistryPath == e.RegistryPath));
                _registryService.SaveErrorsToFile(currentErrors);
            }

            OnPropertyChanged(nameof(FixedErrorsCount));
            OnPropertyChanged(nameof(SelectedErrorsCount));
            // Применяем настройки уведомлений и звука
            if (_settings.ShowNotificationAfterFix)
            {
                MessageBox.Show("Исправление завершено!", "Уведомление",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (_settings.SaveLogsToFile)
            {
                SaveLogs();
            }
        }
        private void SaveLogs()
        {
            var settings = SettingsViewModel.Instance?.Settings;
            if (settings == null || !settings.SaveLogsToFile) return;

            string logDir = settings.UseDefaultLogPath
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RegistryLogs")
                : settings.CustomLogPath;

            if (string.IsNullOrEmpty(logDir)) return;

            string logFile = Path.Combine(logDir, $"RegistryFix_{DateTime.Now:yyyy-MM-dd}.log");

            try
            {
                Directory.CreateDirectory(logDir);
                File.AppendAllLines(logFile, RegistryErrors
                    .Where(e => e.IsFixed)
                    .Select(e => $"[{DateTime.Now}] {e.RegistryPath}"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения лога: {ex.Message}");
            }
        }

        private void PlayCompletionSound()
        {
            // Реализация воспроизведения звука
            SystemSounds.Beep.Play(); // Или кастомный звук
        }

        private void ShowNotification()
        {
            // Реализация показа уведомления
            MessageBox.Show("Исправление завершено!", "Уведомление",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void CheckAutoScan()
        {
            if (SettingsViewModel.Instance?.Settings?.AutoScanOnStartup == true)
            {
                ScanRegistry();
            }
        }

        private bool CheckAdminRightsWithMessage()
        {
            if (_registryService.HasAdminRights())
                return true;

            System.Windows.MessageBox.Show(
                "Для выполнения этой операции требуются права администратора.",
                "Недостаточно прав",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return false;
        }
        private bool CanFixSelected()
        {
            bool canExecute = RegistryErrors.Any(e => e.IsSelected && !e.IsFixed);
            Debug.WriteLine($"CanFixSelected: {canExecute}");
            return canExecute;
        }
    }
}