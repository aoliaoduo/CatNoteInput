using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CatNoteInput;

public sealed class BooleanNegationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return DependencyProperty.UnsetValue;
    }
}
