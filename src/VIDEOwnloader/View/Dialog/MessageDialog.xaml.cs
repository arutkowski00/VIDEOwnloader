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

        public static readonly DependencyProperty IconKindProperty = DependencyProperty.Register(
            "IconKind", typeof(PackIconKind), typeof(MessageDialog), new PropertyMetadata(default(PackIconKind)));


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

        public PackIconKind IconKind
        {
            get { return (PackIconKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }

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