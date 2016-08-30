using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VIDEOwnloader.Converter
{
    internal class MultiVisibilityConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
                if (!(value is Visibility) ||
                    (value is Visibility && ((Visibility)value != Visibility.Visible)))
                    return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}