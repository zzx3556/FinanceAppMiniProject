using System;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FinanceApp.Converters;

public class StringToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string type)
        {
            return type == "支出" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
        }
        
        if (value is decimal balance)
        {
            return balance >= 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }
        
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}