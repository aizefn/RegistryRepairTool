using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RegistryRepairTool.Models;
using RegistryRepairTool.Services;

namespace RegistryRepairTool.ViewModels
{
    public class ErrorListViewModel : ObservableObject
    {
        private readonly RegistryService _registryService = new();
        private readonly Window _window;
        private readonly AutoFixViewModel _autoFixViewModel;

        // Заменяем List на ObservableCollection
        public ObservableCollection<RegistryError> Errors { get; }

        public ICommand ClearCommand { get; }
        public ICommand CloseCommand { get; }

        public ErrorListViewModel(List<RegistryError> errors, Window window)
        {
            // Инициализируем ObservableCollection
            Errors = new ObservableCollection<RegistryError>(errors);
            _window = window;
            _autoFixViewModel = AutoFixViewModel.Instance;

            ClearCommand = new RelayCommand(ClearErrors);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void ClearErrors()
        {
            // Очищаем через AutoFixViewModel
            _autoFixViewModel.ClearAllErrors();

            // Очищаем локальную коллекцию
            Errors.Clear();

            // Явно уведомляем об изменении коллекции
            OnPropertyChanged(nameof(Errors));
        }

        private void CloseWindow()
        {
            _window.Close();
        }
    }
}