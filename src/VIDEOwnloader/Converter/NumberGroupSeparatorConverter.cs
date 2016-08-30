using System;
using System.Globalization;
using System.Windows.Data;

namespace VIDEOwnloader.Converter
{
    public class NumberGroupSeparatorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nfi = new CultureInfo(string.Empty).NumberFormat;
            nfi.NumberGroupSeparator = " ";
            if (value is int)
                return ((int)value).ToString("#,##0", nfi);
            if (value is uint)
                return ((uint)value).ToString("#,##0", nfi);
            if (value is short)
                return ((short)value).ToString("#,##0", nfi);
            if (value is long)
                return ((long)value).ToString("#,##0", nfi);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}