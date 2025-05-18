using RegistryRepairTool.ViewModels;
using System.Windows.Controls;

namespace RegistryRepairTool.Views.Pages
{
    public partial class AutoFixPage : Page
    {
        public AutoFixPage()
        {
            InitializeComponent();
            DataContext = AutoFixViewModel.Instance; // Используем Singleton-экземпляр
        }

    }
}