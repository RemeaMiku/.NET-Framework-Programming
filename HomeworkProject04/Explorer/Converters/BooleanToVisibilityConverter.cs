using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Explorer.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    #region Public Methods

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        throw new ArgumentException($"{nameof(value)} is not a valid Boolean value.", nameof(value));
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible;
        throw new ArgumentException($"{nameof(value)} is not a valid Visibility value.", nameof(value));
    }

    #endregion Public Methods
}
