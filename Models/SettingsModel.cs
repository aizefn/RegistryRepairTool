using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool.Models
{
    namespace RegistryRepairTool.Models
    {
        public class SettingsModel : ObservableObject
        {
            private bool _scanSystemRegistry = true;
            private bool _scanUserRegistry = true;
            private bool _createBackupBeforeFix = true;
            private string _backupLocation = "Backups";
            private bool _showDetailedLogs;

            public bool ScanSystemRegistry
            {
                get => _scanSystemRegistry;
                set => SetProperty(ref _scanSystemRegistry, value);
            }

            public bool ScanUserRegistry
            {
                get => _scanUserRegistry;
                set => SetProperty(ref _scanUserRegistry, value);
            }

            public bool CreateBackupBeforeFix
            {
                get => _createBackupBeforeFix;
                set => SetProperty(ref _createBackupBeforeFix, value);
            }

            public string BackupLocation
            {
                get => _backupLocation;
                set => SetProperty(ref _backupLocation, value);
            }

            public bool ShowDetailedLogs
            {
                get => _showDetailedLogs;
                set => SetProperty(ref _showDetailedLogs, value);
            }
            private bool _isFirstRun = true;
            public bool IsFirstRun
            {
                get => _isFirstRun;
                set => SetProperty(ref _isFirstRun, value);
            }
        }
    }
}
