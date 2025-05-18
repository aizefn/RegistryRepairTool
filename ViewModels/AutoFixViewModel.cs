using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RegistryRepairTool.Models;
using RegistryRepairTool.Services;
using RegistryRepairTool.Utilities;

namespace RegistryRepairTool.ViewModels
{
    public class AutoFixViewModel : ObservableObject
    {
        private static AutoFixViewModel _instance;
        private readonly RegistryService _registryService;

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

            // Загружаем сохраненные ошибки при инициализации
            var savedErrors = _registryService.LoadErrorsFromFile();
            foreach (var error in savedErrors)
            {
                RegistryErrors.Add(error);
            }

            ScanRegistryCommand = new RelayCommand(ScanRegistry);
            FixAllCommand = new RelayCommand(FixAllErrors);
            SelectAllCommand = new RelayCommand(SelectAllErrors);
            FixSelectedCommand = new RelayCommand(FixSelectedErrors, CanFixSelected);
            DeselectAllCommand = new RelayCommand(DeselectAllErrors);
            ClearAllErrorsCommand = new RelayCommand(ClearAllErrors);

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
        private void DeselectAllErrors()
        {
            foreach (var error in RegistryErrors.Where(e => e.IsSelected))
            {
                error.IsSelected = false;
            }
            SelectAllButtonContent = "Выбрать все";
            OnPropertyChanged(nameof(SelectedErrorsCount));
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
                    }
                };
            }

            OnPropertyChanged(nameof(SelectedErrorsCount));
            OnPropertyChanged(nameof(FixedErrorsCount));
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

        private void SelectAllErrors()
        {
            bool allSelected = RegistryErrors.All(e => e.IsSelected);
            foreach (var error in RegistryErrors)
            {
                error.IsSelected = !allSelected;
            }
            SelectAllButtonContent = allSelected ? "Выбрать все" : "Снять выделение";
            OnPropertyChanged(nameof(SelectedErrorsCount));
        }


        private void FixSelectedErrors()
        {
            foreach (var error in RegistryErrors.Where(e => e.IsSelected && !e.IsFixed))
            {
                if (_registryService.TryFixError(error))
                {
                    error.IsFixed = true;
                    error.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(FixedErrorsCount));
            OnPropertyChanged(nameof(SelectedErrorsCount));
        }

        private bool CanFixSelected()
        {
            return RegistryErrors.Any(e => e.IsSelected && !e.IsFixed);
        }
    }
}