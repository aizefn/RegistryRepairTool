using Microsoft.Win32;
using System;
using System.Windows;

namespace RegistryRepairTool.Views
{
    public partial class ValueEditDialog : Window
    {
        public string ValueName { get; set; }
        public object Value { get; set; }
        public RegistryValueKind ValueKind { get; set; }

        public ValueEditDialog()
        {
            InitializeComponent();
            DataContext = this;
            ValueKind = RegistryValueKind.String;
        }

        public ValueEditDialog(string name, object value, RegistryValueKind kind) : this()
        {
            ValueName = name;
            Value = value;
            ValueKind = kind;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (ValueKind)
                {
                    case RegistryValueKind.DWord:
                        Value = Convert.ToInt32(Value);
                        break;
                    case RegistryValueKind.QWord:
                        Value = Convert.ToInt64(Value);
                        break;
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        Value = Value.ToString();
                        break;
                    case RegistryValueKind.MultiString:
                        Value = ((string)Value).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        break;
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка преобразования значения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }
        }
    }
}