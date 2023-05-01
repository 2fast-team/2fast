using Microsoft.Toolkit.Uwp.UI;
using Microsoft.UI.Xaml.Controls;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Project2FA.UWP.Views
{
    public sealed partial class TutorialPage : Page
    {
        public TutorialPageViewModel ViewModel => DataContext as TutorialPageViewModel;
        public TutorialPage()
        {
            this.InitializeComponent();
            this.Loaded += TutorialPage_Loaded;
        }

        private void TutorialPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        private async Task CreateTeachingTip(FrameworkElement element, string content)
        {
            var control = MainGrid.FindDescendant(nameof(TeachingTip));
            if (control != null)
            {
                var tooltip = (control as TeachingTip);
                if (tooltip.IsOpen)
                {
                    tooltip.IsOpen = false;
                    await Task.Delay(500);
                }
                tooltip.Content = content;
                tooltip.Target = element;
                tooltip.IsOpen = true;

            }
            else
            {
                TeachingTip teachingTip = new TeachingTip
                {
                    Target = element,
                    Name = nameof(TeachingTip),
                    Content = content,
                    IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource { Symbol = Symbol.Help },
                    BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                    IsOpen = true,
                };
                MainGrid.Children.Add(teachingTip);
            }
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        private void AccountCodePageItemMoreBTN_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, string.Empty);
            }
        }

        private void BTN_SetFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, string.Empty);
            }
        }

        private void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, string.Empty);
            }
        }

        private void BTN_ShowCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, string.Empty);
            }
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
}
