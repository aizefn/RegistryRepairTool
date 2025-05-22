using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Views.Pages
{
    public partial class ManualEditPage : Page
    {
        public ManualEditPage()
        {
            InitializeComponent();
            var vm = RegistryEditViewModel.Instance;
            DataContext = vm;
            Loaded += OnPageLoaded;

            // Подписываемся на событие показа уведомления
            PathTextBox.Focus(); // Устанавливаем фокус на TextBox при загрузке
        }
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnPageLoaded; // Отписываемся после первого вызова
            var vm = DataContext as RegistryEditViewModel;
            if (vm != null)
            {
                // Подписываемся на событие только после загрузки страницы
                vm.RequestShowCopyNotification += (s, args) => ShowCopyNotification();
            }
        }
        private void ShowCopyNotification()
        {
            Dispatcher.Invoke(() =>
            {
                var storyboard = new Storyboard();

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                Storyboard.SetTarget(fadeIn, CopyNotificationText);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));

                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(1.5),
                    Duration = TimeSpan.FromSeconds(0.5)
                };
                Storyboard.SetTarget(fadeOut, CopyNotificationText);
                Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));

                storyboard.Children.Add(fadeIn);
                storyboard.Children.Add(fadeOut);

                storyboard.Begin();
            });
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is RegistryEditViewModel vm)
            {
                vm.SelectedNode = e.NewValue as RegistryNode;
                // Обновляем текущий путь при выборе узла
                if (vm.SelectedNode != null)
                {
                    vm.CurrentPath = vm.SelectedNode.FullPath;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is RegistryEditViewModel vm)
            {
                vm.NavigateCommand.Execute(PathTextBox.Text);
                PathTextBox.Text = "";
                PathTextBox.Focus();
            }
        }

    }
}