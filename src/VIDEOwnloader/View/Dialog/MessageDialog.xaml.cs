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
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace VIDEOwnloader.View.Dialog
{
    public partial class MessageDialog
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(string), typeof(MessageDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(MessageDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty DialogButtonProperty = DependencyProperty.Register(
            "DialogButton", typeof(MessageDialogButton), typeof(MessageDialog),
            new PropertyMetadata(default(MessageDialogButton), DialogButtonChanged));

        public MessageDialog()
        {
            InitializeComponent();
        }

        public MessageDialogButton DialogButton
        {
            get { return (MessageDialogButton)GetValue(DialogButtonProperty); }
            set { SetValue(DialogButtonProperty, value); }
        }

        public MessageDialogResult DialogResult { get; private set; }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Cancel;
        }

        private void CloseButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Close;
        }

        private static void DialogButtonChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var dialog = dependencyObject as MessageDialog;
            dialog?.InitDialogButtons();
        }

        private void IgnoreButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Ignore;
        }

        private void InitDialogButtons()
        {
            ButtonsPanel.Children.Clear();

            if ((DialogButton == MessageDialogButton.Ok) ||
                (DialogButton == MessageDialogButton.OkCancel))
            {
                var okButton = new Button
                {
                    Content = "OK",
                    IsDefault = true
                };
                okButton.Click += OkButtonOnClick;
                ButtonsPanel.Children.Add(okButton);
            }

            if ((DialogButton == MessageDialogButton.YesNo) ||
                (DialogButton == MessageDialogButton.YesNoCancel))
            {
                var yesButton = new Button
                {
                    Content = "YES",
                    IsDefault = true
                };
                yesButton.Click += YesButtonOnClick;
                ButtonsPanel.Children.Add(yesButton);

                var noButton = new Button
                {
                    Content = "NO",
                    IsCancel = true
                };
                noButton.Click += NoButtonOnClick;
                ButtonsPanel.Children.Add(noButton);
            }

            if ((DialogButton == MessageDialogButton.RetryCancel) ||
                (DialogButton == MessageDialogButton.RetryIgnoreCancel))
            {
                var retryButton = new Button
                {
                    Content = "RETRY",
                    IsDefault = true
                };
                retryButton.Click += RetryButtonOnClick;
                ButtonsPanel.Children.Add(retryButton);
            }

            if (DialogButton == MessageDialogButton.RetryIgnoreCancel)
            {
                var ignoreButton = new Button
                {
                    Content = "IGNORE"
                };
                ignoreButton.Click += IgnoreButtonOnClick;
                ButtonsPanel.Children.Add(ignoreButton);
            }

            if ((DialogButton == MessageDialogButton.OkCancel) ||
                (DialogButton == MessageDialogButton.YesNoCancel) ||
                (DialogButton == MessageDialogButton.RetryCancel) ||
                (DialogButton == MessageDialogButton.RetryIgnoreCancel))
            {
                var cancelButton = new Button
                {
                    Content = "CANCEL",
                    IsCancel = true
                };
                cancelButton.Click += CancelButtonOnClick;
                ButtonsPanel.Children.Add(cancelButton);
            }

            if (DialogButton == MessageDialogButton.Close)
            {
                var closeButton = new Button
                {
                    Content = "CLOSE",
                    IsDefault = true
                };
                closeButton.Click += CloseButtonOnClick;
                ButtonsPanel.Children.Add(closeButton);
            }

            foreach (Button button in ButtonsPanel.Children)
            {
                button.SetResourceReference(StyleProperty, "MaterialDesignFlatButton");
                button.Click += (sender, args) => DialogHost.CloseDialogCommand.Execute(DialogResult, null);
            }
        }

        private void NoButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.No;
        }

        private void OkButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Ok;
        }

        private void RetryButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Retry;
        }

        private void YesButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = MessageDialogResult.Yes;
        }
    }

    public enum MessageDialogButton
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel,
        RetryCancel,
        RetryIgnoreCancel,
        Close
    }

    public enum MessageDialogResult
    {
        Ok,
        Cancel,
        Yes,
        No,
        Retry,
        Ignore,
        Close
    }
}