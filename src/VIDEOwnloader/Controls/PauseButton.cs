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

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace VIDEOwnloader.Controls
{
    public class PauseButton : ToggleButton
    {
        public static readonly DependencyProperty PauseCommandProperty = DependencyProperty.Register(
            "PauseCommand", typeof(ICommand), typeof(PauseButton), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty PauseCommandParameterProperty = DependencyProperty.Register(
            "PauseCommandParameter", typeof(object), typeof(PauseButton), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty UnpauseCommandParameterProperty = DependencyProperty.Register(
            "UnpauseCommandParameter", typeof(object), typeof(PauseButton), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty UnpauseCommandProperty = DependencyProperty.Register(
            "UnpauseCommand", typeof(ICommand), typeof(PauseButton), new PropertyMetadata(default(ICommand)));

        static PauseButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PauseButton),
                new FrameworkPropertyMetadata(typeof(PauseButton)));
        }

        public PauseButton()
        {
            Checked += PauseButton_Checked;
            Unchecked += PauseButton_Unchecked;
        }

        public object UnpauseCommandParameter
        {
            get { return GetValue(UnpauseCommandParameterProperty); }
            set { SetValue(UnpauseCommandParameterProperty, value); }
        }

        public object PauseCommandParameter
        {
            get { return GetValue(PauseCommandParameterProperty); }
            set { SetValue(PauseCommandParameterProperty, value); }
        }

        public ICommand PauseCommand
        {
            get { return (ICommand)GetValue(PauseCommandProperty); }
            set { SetValue(PauseCommandProperty, value); }
        }

        public ICommand UnpauseCommand
        {
            get { return (ICommand)GetValue(UnpauseCommandProperty); }
            set { SetValue(UnpauseCommandProperty, value); }
        }

        private void PauseButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PauseCommand == null) return;

            var parameter = PauseCommandParameter ?? DataContext;
            if (PauseCommand.CanExecute(parameter))
                PauseCommand.Execute(parameter);
        }

        private void PauseButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (UnpauseCommand == null) return;

            var parameter = UnpauseCommandParameter ?? DataContext;
            if (UnpauseCommand.CanExecute(parameter))
                UnpauseCommand.Execute(parameter);
        }
    }
}