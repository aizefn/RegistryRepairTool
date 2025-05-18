using System.Windows;
using RegistryRepairTool.Services;
using RegistryRepairTool.Views.Pages;

namespace RegistryRepairTool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var registryService = new RegistryService();

            // Создаем главное окно, но пока не показываем
            MainWindow mainWindow = new MainWindow();

            if (!registryService.IsAcceptedConfirmed())
            {
                // Создаем окно приветствия
                var welcomeWindow = new Window
                {
                    Title = "Добро пожаловать",
                    Content = new WelcomePage(),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize
                };

                // Показываем модально окно приветствия
                welcomeWindow.ShowDialog();

                // Проверяем, было ли подтверждение
                if (registryService.IsAcceptedConfirmed())
                {
                    // Показываем главное окно
                    mainWindow.Show();
                }
                else
                {
                    // Закрываем приложение, если не подтвердили
                    Shutdown();
                    return;
                }
            }
            else
            {
                // Если уже подтверждено - показываем главное окно
                mainWindow.Show();
            }

        }
    }
}