using System.Windows;
using System.Windows.Controls.Primitives;

namespace VIDEOwnloader.Controls
{
    public class PauseButton : ToggleButton
    {
        static PauseButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PauseButton),
                new FrameworkPropertyMetadata(typeof(PauseButton)));
        }
    }
}