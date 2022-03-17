
using System.Threading;
using Windows.Foundation;
#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif


namespace Project2FA.Uno.Core.Dialogs
{
    public static class DialogExtensions
    {
        public static IAsyncOperation<ContentDialogResult> ShowAsync(this ContentDialog dialog, CancellationToken token)
        {
            token.Register(() => dialog.Hide());
            return dialog.ShowAsync();
        }
    }
}
