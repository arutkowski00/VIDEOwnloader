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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight.Views;

namespace VIDEOwnloader.Common
{
    public interface IFrameNavigationService : INavigationService
    {
        object Parameter { get; }
    }

    internal class FrameNavigationService : IFrameNavigationService, INotifyPropertyChanged
    {
        private readonly List<string> _historic;
        private readonly Dictionary<string, Uri> _pagesByKey;
        private string _currentPageKey;

        public FrameNavigationService()
        {
            _pagesByKey = new Dictionary<string, Uri>();
            _historic = new List<string>();
        }

        #region IFrameNavigationService Members

        public string CurrentPageKey
        {
            get { return _currentPageKey; }

            private set
            {
                if (_currentPageKey == value)
                    return;

                _currentPageKey = value;
                OnPropertyChanged("CurrentPageKey");
            }
        }

        public object Parameter { get; private set; }

        public void GoBack()
        {
            if (_historic.Count > 1)
            {
                _historic.RemoveAt(_historic.Count - 1);
                NavigateTo(_historic.Last(), null);
            }
        }

        public void NavigateTo(string pageKey)
        {
            NavigateTo(pageKey, null);
        }

        public virtual void NavigateTo(string pageKey, object parameter)
        {
            lock (_pagesByKey)
            {
                if (!_pagesByKey.ContainsKey(pageKey))
                    throw new ArgumentException(string.Format("No such page: {0} ", pageKey), "pageKey");

                var frame = GetDescendantFromName(Application.Current.MainWindow, "MainFrame") as Frame;

                if (frame != null)
                    frame.Source = _pagesByKey[pageKey];
                Parameter = parameter;
                _historic.Add(pageKey);
                CurrentPageKey = pageKey;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Configure(string key, Uri pageType)
        {
            lock (_pagesByKey)
            {
                if (_pagesByKey.ContainsKey(key))
                    _pagesByKey[key] = pageType;
                else
                    _pagesByKey.Add(key, pageType);
            }
        }

        private static FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            if (count < 1)
                return null;

            for (var i = 0; i < count; i++)
            {
                var frameworkElement = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (frameworkElement != null)
                {
                    if (frameworkElement.Name == name)
                        return frameworkElement;

                    frameworkElement = GetDescendantFromName(frameworkElement, name);
                    if (frameworkElement != null)
                        return frameworkElement;
                }
            }
            return null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}