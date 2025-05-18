using System.Windows;
using System.Windows.Controls;
using RegistryRepairTool.Services;
using RegistryRepairTool.Views.Dialogs;

namespace RegistryRepairTool.Views.Pages
{
    public partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AcceptDialog { Owner = Window.GetWindow(this) };

            if (dialog.ShowDialog() == true)
            {
                // Сохраняем подтверждение
                var registryService = new RegistryService();
                registryService.SaveAcceptance();

                // Закрываем окно приветствия
                Window.GetWindow(this).Close();
            }
        }
    }
}