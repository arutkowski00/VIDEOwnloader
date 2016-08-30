using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace VIDEOwnloader.View.Dialog
{
    /// <summary>
    ///     Interaction logic for NewDownloadDialog.xaml
    /// </summary>
    public partial class NewDownloadDialog
    {
        public NewDownloadDialog()
        {
            InitializeComponent();

            if (!ViewModelBase.IsInDesignModeStatic)
                Background = Brushes.Transparent;
        }
    }
}