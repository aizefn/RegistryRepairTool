using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegistryRepairTool.ViewModels;

    namespace RegistryRepairTool.Models
    {
        public class SettingsModel : ObservableObject
        {
            private bool _runAtStartup;
            private bool _autoScanOnStartup;
            private bool _showNotificationAfterFix;
            private bool _playSoundOnScanComplete;
            private bool _saveLogsToFile;
            private bool _useDefaultLogPath = true;
            private bool _useCustomLogPath;
            private string _customLogPath;
            private string _backupLocation;
            public bool RunAtStartup
            {
                get => _runAtStartup;
                set => SetProperty(ref _runAtStartup, value);
            }

            public bool AutoScanOnStartup
            {
                get => _autoScanOnStartup;
                set => SetProperty(ref _autoScanOnStartup, value);
            }

            public bool ShowNotificationAfterFix
            {
                get => _showNotificationAfterFix;
                set => SetProperty(ref _showNotificationAfterFix, value);
            }

            public bool PlaySoundOnScanComplete
            {
                get => _playSoundOnScanComplete;
                set => SetProperty(ref _playSoundOnScanComplete, value);
            }

            public bool SaveLogsToFile
            {
                get => _saveLogsToFile;
                set => SetProperty(ref _saveLogsToFile, value);
            }

            public bool UseDefaultLogPath
            {
                get => _useDefaultLogPath;
                set
                {
                    SetProperty(ref _useDefaultLogPath, value);
                    if (value) UseCustomLogPath = false;
                }
            }

            public bool UseCustomLogPath
            {
                get => _useCustomLogPath;
                set
                {
                    SetProperty(ref _useCustomLogPath, value);
                    if (value) UseDefaultLogPath = false;
                }
            }

            public string CustomLogPath
            {
                get => _customLogPath;
                set => SetProperty(ref _customLogPath, value);
            }

            public string BackupLocation
            {
                get => _backupLocation;
                set => SetProperty(ref _backupLocation, value);
            }
        }
    }
