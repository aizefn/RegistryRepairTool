using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RegistryRepairTool.Utilities.Converters
{
    public class BoolToFixedConverter : IValueConverter
    {
        public object TrueValue { get; set; } = "Исправлено";
        public object FalseValue { get; set; } = "Требует исправления";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}