using RegistryRepairTool.Views.Pages;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RegistryRepairTool.ViewModels
{
    public class NavigationVM : INotifyPropertyChanged
    {
        private string _currentPageKey = "Home";
        private Page _currentPage;

        // Текущая активная страница (для привязки во Frame)
        public Page CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Ключ текущей страницы (для подсветки активной кнопки)
        public string CurrentPageKey
        {
            get => _currentPageKey;
            private set
            {
                if (_currentPageKey != value)
                {
                    _currentPageKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateCommand { get; }

        public NavigationVM()
        {
            // Инициализация стартовой страницы
            NavigateCommand = new RelayCommand<string>(Navigate);
            Navigate("Home"); // Стартовая страница
        }

        private void Navigate(string page)
        {
            CurrentPageKey = page;
            CurrentPage = page switch
            {
                "Home" => new HomePage(),
                "AutoFix" => new AutoFixPage(),
                "ManualEdit" => new ManualEditPage(),
                _ => new AutoFixPage() // fallback
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}