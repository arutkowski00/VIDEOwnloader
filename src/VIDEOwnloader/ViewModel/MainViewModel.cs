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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MaterialDesignThemes.Wpf;
using VIDEOwnloader.View.Dialog;

namespace VIDEOwnloader.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private AboutDialog _aboutDialog;
        private RelayCommand _openAboutDialogCommand;

        public RelayCommand OpenAboutDialogCommand
            => _openAboutDialogCommand ?? (_openAboutDialogCommand = new RelayCommand(OpenAboutDialog));

        private async void OpenAboutDialog()
        {
            await DialogHost.Show(_aboutDialog ?? (_aboutDialog = new AboutDialog()), "RootDialog");
        }
    }
}