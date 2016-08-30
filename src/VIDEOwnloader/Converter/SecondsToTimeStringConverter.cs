using System;
using System.Globalization;
using System.Windows.Data;

namespace VIDEOwnloader.Converter
{
    public class SecondsToTimeStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var span = new TimeSpan(0, 0, value is int ? (int)value : 0);

            var formatted = string.Format("{0}{1}:{2:D2}",
                span.Hours > 0 ? string.Format("{0:0}:", span.Hours) : string.Empty,
                span.Hours > 0 ? string.Format("{0:D2}:", span.Minutes) : span.Minutes.ToString(),
                span.Seconds);

            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}