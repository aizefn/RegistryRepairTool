using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RegistryRepairTool.Models;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Views.Pages
{
    public partial class HomePage : Page
    {
        public ObservableCollection<string> RecentErrors { get; }
        public ObservableCollection<string> OptimizationTips { get; }
        public ICommand ShowAllErrorsCommand { get; }
        public SystemInfoViewModel SystemInfo { get; } = new SystemInfoViewModel();

        public HomePage()
        {
            InitializeComponent();
            RecentErrors = new ObservableCollection<string>();
            OptimizationTips = new ObservableCollection<string>();
            ShowAllErrorsCommand = new RelayCommand(ShowAllErrors);

            DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            LoadRecentErrors();
            LoadOptimizationTips();
        }

        public void LoadRecentErrors()
        {
            RecentErrors.Clear();

            // Получаем ошибки из AutoFixViewModel
            var errors = AutoFixViewModel.Instance.RegistryErrors
                .OrderByDescending(e => e.Severity)
                .Take(2)
                .Select(e => $"{e.ErrorName}: {e.Description}");

            foreach (var error in errors)
            {
                RecentErrors.Add(error);
            }
        }

        private void LoadOptimizationTips()
        {
            OptimizationTips.Clear();
            OptimizationTips.Add("Регулярно создавайте резервные копии реестра");
            OptimizationTips.Add("Удаляйте неиспользуемые программы через 'Установку и удаление программ'");
            OptimizationTips.Add("Используйте функцию 'Автоисправление' для быстрого исправления ошибок");
        }

        private void ShowAllErrors()
        {
            var errors = AutoFixViewModel.Instance.RegistryErrors.ToList();
            var window = new ErrorListWindow(errors)
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
            LoadRecentErrors(); // Обновляем список после закрытия окна
        }
    }
}