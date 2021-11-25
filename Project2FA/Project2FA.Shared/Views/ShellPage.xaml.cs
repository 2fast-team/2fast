#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace Project2FA.Views
{
    public sealed partial class ShellPage : ContentControl
    {
        public ShellPage()
        {
            this.InitializeComponent();
        }
    }
}
