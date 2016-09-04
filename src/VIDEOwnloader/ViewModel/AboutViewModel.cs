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
using System.Diagnostics;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using VIDEOwnloader.Properties;

namespace VIDEOwnloader.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private RelayCommand<Uri> _navigateUriCommand;

        public string TitleText => _assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public string VersionText => _assembly.GetName().Version.ToString();

        public string CopyrightText => _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

        public string DescriptionText => _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public string LicenseText => Resources.LicenseText;

        public RelayCommand<Uri> NavigateUriCommand
            => _navigateUriCommand ?? (_navigateUriCommand = new RelayCommand<Uri>(NavigateUri));

        private void NavigateUri(Uri uri)
        {
            Process.Start(uri.ToString());
        }
    }
}