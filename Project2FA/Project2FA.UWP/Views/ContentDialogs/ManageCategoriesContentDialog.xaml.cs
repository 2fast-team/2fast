using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Linq;
using UNOversal.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using CommandBarFlyout = Microsoft.UI.Xaml.Controls.CommandBarFlyout;

#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif


namespace Project2FA.UWP.Views
{
    public sealed partial class ManageCategoriesContentDialog : ContentDialog
    {
        public ManageCategoriesContentDialogViewModel ViewModel => DataContext as ManageCategoriesContentDialogViewModel;

        private CommandBarFlyout _openedCommandBarFlyout;
        public ManageCategoriesContentDialog()
        {
            this.InitializeComponent();
            this.Loaded += ManageCategoriesContentDialog_Loaded;
        }

        private void ManageCategoriesContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.DataChanged = true;
        }

        private void CB_CategoryModel_Loaded(object sender, RoutedEventArgs e)
        {
            if ((FrameworkElement)sender is ComboBox cb)
            {
                if (cb.DataContext is CategoryModel model)
                {
                    ViewModel.SelectedComboBoxItem = ViewModel.IconSourceCollection.Where(x => x.UnicodeIndex == Convert.ToUInt32(model.UnicodeIndex)).FirstOrDefault();
                    //cb.SelectedItem = ViewModel.IconSourceCollection.Where(x => x.UnicodeIndex == Convert.ToUInt32(model.UnicodeIndex)).FirstOrDefault();
                }
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //FindAndCloseFlyout(sender);
            _openedCommandBarFlyout?.Hide();
            if (ViewModel.SelectedComboBoxItem != null)
            {
                if (sender is AppBarButton abtn && abtn.DataContext is CategoryModel model)
                {
                    model.UnicodeIndex = ViewModel.SelectedComboBoxItem.UnicodeIndex.ToString();
                    model.UnicodeString = ViewModel.SelectedComboBoxItem.UnicodeString;
                }
            }
        }

        private void BTN_ShowIcons_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                FlyoutBase.ShowAttachedFlyout(btn);
            }
        }
#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(CommandBarFlyout))]
#endif
        private void CommandBarFlyout_Opening(object sender, object e)
        {
            if (sender is Microsoft.UI.Xaml.Controls.CommandBarFlyout cmdf)
            {
                _openedCommandBarFlyout = cmdf;
            }
        }
    }
}
