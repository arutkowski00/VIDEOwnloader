#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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