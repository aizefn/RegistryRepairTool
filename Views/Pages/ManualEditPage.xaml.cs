using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Views.Pages
{
    public partial class ManualEditPage : Page
    {
        public ManualEditPage()
        {
            InitializeComponent();
            DataContext = RegistryEditViewModel.Instance;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is RegistryEditViewModel vm)
            {
                vm.SelectedNode = e.NewValue as RegistryNode;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is RegistryEditViewModel vm)
            {
                vm.NavigateCommand.Execute(null);
            }
        }
    }
}