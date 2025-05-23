using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace RegistryRepairTool.Utilities.Converters
{
    public class ErrorToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF44336")) :
                Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ErrorToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                Brushes.White :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ErrorToIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                new SolidColorBrush(Colors.White) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(parameter as string ?? "#FF2196F3"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ErrorToIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                PackIconKind.AlertCircle :
                PackIconKind.ClockOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ErrorToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                "Последняя проверка: ошибка!" :
                "Последняя проверка: успешно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
    public class ErrorToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasError && hasError) ?
                new SolidColorBrush(Colors.White) :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}