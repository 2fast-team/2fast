using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
#if HAS_WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace Project2FA.Uno.Core
{
    /// <summary>
    /// Helper class for MVVM.
    /// </summary>
    public static class MvvmHelpers
    {
        internal static void AutowireViewModel(object viewOrViewModel)
        {
            if (viewOrViewModel is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutowireViewModel(view) is null)
            {
                ViewModelLocator.SetAutowireViewModel(view, true);
            }
        }
    }
}
