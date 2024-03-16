using Project2FA.Extensions;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class ManageCategoriesContentDialog : ContentDialog
    {
        public ManageCategoriesContentDialogViewModel ViewModel => DataContext as ManageCategoriesContentDialogViewModel;
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

        private void CB_CategoryModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((FrameworkElement)sender is ComboBox cb)
            {
                if (cb.DataContext is CategoryModel model)
                {
                    var newValue = (FontIdentifikationModel)cb.SelectedItem;
                    model.UnicodeIndex = newValue.UnicodeIndex.ToString();
                    model.UnicodeString = newValue.UnicodeString;
                }
            }

        }

        private void CB_CategoryModel_Loaded(object sender, RoutedEventArgs e)
        {
            if ((FrameworkElement)sender is ComboBox cb)
            {
                if (cb.DataContext is CategoryModel model)
                {
                    cb.SelectedItem = ViewModel.IconSourceCollection.Where(x => x.UnicodeIndex == Convert.ToUInt32(model.UnicodeIndex)).FirstOrDefault();
                }
            }
        }

        private void FindAndCloseFlyout(object sender)
        {
            var parent = Page ?? sender as DependencyObject;
            while (parent != null)
            {
                if (parent is FlyoutPresenter)
                {
                    ((parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                    break;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FindAndCloseFlyout(sender);
        }
    }
}
