using System;
using System.ComponentModel;

namespace RegistryRepairTool.Services
{
    public enum ErrorSeverity { Low, Medium, High }

    public class RegistryError : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isFixed;


        public string ErrorName { get; set; }
        public string Description { get; set; }
        public string RegistryPath { get; set; }
        public ErrorSeverity Severity { get; set; }
        public string ErrorType { get; set; } // "Registry", "FileSystem", "Permission" и т.д.

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        private int _registryValue = 0;
        private double _minValue = 0;
        private double _maxValue = 100;
        public int RegistryValue
        {
            get => _registryValue;
            set
            {
                if (_registryValue != value)
                {
                    _registryValue = value;
                    OnPropertyChanged(nameof(RegistryValue));
                }
            }
        }
        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                if (_isFixed != value)
                {
                    _isFixed = value;
                    OnPropertyChanged(nameof(IsFixed));
                }
            }
        }
        public double MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue != value)
                {
                    _minValue = value;
                    OnPropertyChanged(nameof(MinValue));
                }
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = value;
                    OnPropertyChanged(nameof(MaxValue));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}