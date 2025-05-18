using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RegistryRepairTool
{
    public class ErrorColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text.Contains("Ошибка") || text.Contains("Недостаточно прав"))
                    return Brushes.Red;
                if (text.Contains("Реестр чист"))
                    return Brushes.Green;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}