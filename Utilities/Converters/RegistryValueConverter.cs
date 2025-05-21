using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RegistryRepairTool.Utilities.Converters
{
    public class RegistryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (value is string[] multiString)
            {
                return string.Join(Environment.NewLine, multiString);
            }

            if (value is byte[] binaryData)
            {
                return BitConverter.ToString(binaryData).Replace("-", " ");
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
