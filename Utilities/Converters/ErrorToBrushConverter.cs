using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace RegistryRepairTool.Utilities.Converters
{
    public class ErrorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasError && hasError)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")); // Красный
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF")); // Белый
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
