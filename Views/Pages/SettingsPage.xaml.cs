using System.Windows;
using System.Windows.Controls;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();

            Loaded += OnSettingsPageLoaded;
            Unloaded += OnSettingsPageUnloaded;
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.SetCompactSize(); // Уменьшаем окно
            }
        }

        private void OnSettingsPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.RestoreOriginalSize(); // Восстанавливаем исходный размер
            }
        }
    }
}