using System.Collections.Generic;
using System.Windows;
using RegistryRepairTool.Services;
using RegistryRepairTool.ViewModels;
using RegistryRepairTool.Views.Pages;

namespace RegistryRepairTool.Views
{
    public partial class ErrorListWindow : Window
    {
        public ErrorListWindow(List<RegistryError> errors)
        {
            InitializeComponent();
            DataContext = new ErrorListViewModel(errors, this);

            this.Closed += (s, e) =>
            {
                if (Owner?.Content is HomePage homePage)
                {
                    homePage.LoadRecentErrors();
                }
            };
        }


    }
}