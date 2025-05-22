using System;
using System.Windows;
using System.Windows.Controls;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Views.Pages
{
    public partial class AutoFixPage : Page
    {
        public AutoFixPage()
        {
            InitializeComponent();
            DataContext = AutoFixViewModel.Instance;

            Loaded += (s, e) =>
            {
                if (SettingsViewModel.Instance?.Settings?.AutoScanOnStartup == true)
                {
                    AutoFixViewModel.Instance.InitializeAutoScan();
                }
            };
        }
       
    }
}