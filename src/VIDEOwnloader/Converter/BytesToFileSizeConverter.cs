using System;
using System.Globalization;
using System.Windows.Data;

namespace VIDEOwnloader.Converter
{
    public class BytesToSizeStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value is int ? (int)value : 0;

            var mBytes = bytes/1048576D;

            if (mBytes < 1024)
                return string.Format("{0:F2} MB", mBytes);
            return string.Format("{0:F2} GB", mBytes/1024);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}