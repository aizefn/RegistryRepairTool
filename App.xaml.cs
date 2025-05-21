using System.Windows;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using RegistryRepairTool.Services;
using RegistryRepairTool.Views.Pages;

namespace RegistryRepairTool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            // Устанавливаем DPI awareness
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            // Set the theme
            var primaryColor = SwatchHelper.Lookup[MaterialDesignColor.DeepPurple];
            var accentColor = SwatchHelper.Lookup[MaterialDesignColor.Lime];
            ITheme theme = Theme.Create(new MaterialDesignLightTheme(), primaryColor, accentColor);
            Resources.SetTheme(theme);

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
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}