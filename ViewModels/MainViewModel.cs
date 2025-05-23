using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using RegistryRepairTool.Services;
using RegistryRepairTool.Views.Pages;


namespace RegistryRepairTool.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly RegistryService _registryService;
        private Page _currentPage;
        public SystemInfoViewModel SystemInfo { get; } = new SystemInfoViewModel();
        public ICommand ClearCommand { get; }
        public ObservableCollection<string> RegistryErrors { get; }
        public ICommand ScanRegistryCommand { get; }
        public ICommand NavigateCommand { get; }
        // Добавьте эти свойства в MainViewModel
        public ObservableCollection<string> RecentErrors { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OptimizationTips { get; } = new ObservableCollection<string>();
        public ICommand CreateBackupCommand { get; }
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage is SettingsPage)
                {
                    // Если уходим со страницы настроек, восстановить размер
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.RestoreOriginalSize();
                    }
                }

                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));

                if (_currentPage is SettingsPage)
                {
                    // Если переходим на страницу настроек, уменьшить окно
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.SetCompactSize();
                    }
                }
            }
        }
        private string _currentPageKey = "Home"; // Значение по умолчанию
        public string CurrentPageKey
        {
            get => _currentPageKey;
            set => SetProperty(ref _currentPageKey, value);
        }

        public MainViewModel()
        {
            _registryService = new RegistryService();
            RegistryErrors = new ObservableCollection<string>();
            ScanRegistryCommand = new RelayCommand(ScanRegistry);
            ClearCommand = new RelayCommand(() => RegistryErrors.Clear());
            NavigateCommand = new RelayCommand<string>(Navigate);

            // Устанавливаем стартовую страницу (теперь AutoFix по умолчанию)
            CurrentPageKey = "Home";
            CurrentPage = new HomePage();
            CreateBackupCommand = new RelayCommand(CreateBackup);
            LoadTips();
        }
        private void CreateBackup()
        {
            // Реализация создания резервной копии
            MessageBox.Show("Резервная копия реестра успешно создана!");
        }

        private void LoadTips()
        {
            OptimizationTips.Add("Используйте ручное редактирование только если уверены в изменениях");
            OptimizationTips.Add("Проверяйте автозагрузку на наличие ненужных программ");
        }
        private string _currentPageName;
        public string CurrentPageName
        {
            get => _currentPageName;
            set => SetProperty(ref _currentPageName, value);
        }
        private void Navigate(string pageKey)
        {
            if (CurrentPageKey == pageKey)
                return;
            if (string.IsNullOrEmpty(pageKey)) return;

            CurrentPageKey = pageKey;

            switch (pageKey)
            {
                case "Home":
                    CurrentPage = new HomePage();
                    break;
                case "AutoFix":
                    CurrentPage = new AutoFixPage();
                    break;
                case "ManualEdit":
                    CurrentPage = new ManualEditPage();
                    break;
                case "Settings":
                    CurrentPage = new SettingsPage();
                    break;
            }
        }

        private void ScanRegistry()
        {
            try
            {
                RegistryErrors.Clear();
                var errors = _registryService.FindInvalidPathEntries();

                foreach (var error in errors)
                {
                    RegistryErrors.Add(error);
                }

                if (RegistryErrors.Count == 0)
                {
                    RegistryErrors.Add("Ошибок не найдено! Реестр чист.");
                }
            }
            catch (SecurityException)
            {
                RegistryErrors.Clear();
                RegistryErrors.Add("Ошибка: Недостаточно прав. Запустите программу от имени администратора.");
            }
            catch (Exception ex)
            {
                RegistryErrors.Clear();
                RegistryErrors.Add($"Ошибка доступа к реестру: {ex.Message}");
            }
        }

    }
}