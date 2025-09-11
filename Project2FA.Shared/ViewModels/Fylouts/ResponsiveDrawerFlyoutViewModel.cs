#if __UNO_WINUI__

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;

#else
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Project2FA.Uno.Views;
using Project2FA.UnoApp;

#endif

namespace Project2FA.ViewModels
{
    public class ResponsiveDrawerFlyoutViewModel
    {

    }


}
#endif