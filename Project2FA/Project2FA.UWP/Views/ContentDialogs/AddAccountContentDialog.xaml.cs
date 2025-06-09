using Microsoft.UI.Xaml.Controls;
using Project2FA.Extensions;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System;
using UNOversal.Extensions;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class AddAccountContentDialog : ContentDialog
    {
        public AddAccountContentDialogViewModel ViewModel => DataContext as AddAccountContentDialogViewModel;

        public AddAccountContentDialog()
        {
            this.InitializeComponent();

            //register an event for the changed Tag property of the input textbox (for validation)
            TB_AddAccountContentDialogSecretKey.RegisterPropertyChangedCallback(TagProperty, TBTagChangedCallback);
            // register the change of the input
            MainPivot.RegisterPropertyChangedCallback(TagProperty, PivotItemChangedCallback);
            Loaded += AddAccountContentDialog_Loaded;
        }

        private void PivotItemChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == Pivot.TagProperty)
            {
                if (((Pivot)sender).Tag is string tag)
                {
                    if (tag == "PI_AccountInput")
                    {
                        RemovePivotItems(PI_AccountInput);
                        tag = string.Empty;
                    }
                }
            }
        }

        private void AddAccountContentDialog_Loaded(object sender, RoutedEventArgs e)
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

            // remove PivotItems
            RemovePivotItems(null);

            // initialize the camera control
            ViewModel.MediaPlayerElementControl = CameraPlayerElement;
        }

        private void RemovePivotItems(UIElement element)
        {
            switch (element)
            {
                case PivotItem item when item.Name == nameof(PI_AccountInput):
                    if (!MainPivot.Items.Contains(PI_AccountInput))
                    {
                        MainPivot.Items.Add(PI_AccountInput);
                    }
                    if (MainPivot.Items.Contains(PI_ScanQRCodeCamera))
                    {
                        MainPivot.Items.Remove(PI_ScanQRCodeCamera);
                    }
                    if(ViewModel.LastPivotItemName == "PI_ScanQRCodeCamera")
                    {
                        ViewModel.SelectedPivotIndex = 1;
                    }
                    ViewModel.LastPivotItemName = nameof(PI_AccountInput);
                    break;
                case PivotItem item when item.Name == nameof(PI_ScanQRCodeCamera):
                    if (MainPivot.Items.Contains(PI_AccountInput))
                    {
                        MainPivot.Items.Remove(PI_AccountInput);
                    }
                    if (!MainPivot.Items.Contains(PI_ScanQRCodeCamera))
                    {
                        MainPivot.Items.Add(PI_ScanQRCodeCamera);
                    }
                    ViewModel.LastPivotItemName = nameof(PI_ScanQRCodeCamera);
                    break;
                default:
                    if (MainPivot.Items.Contains(PI_AccountInput))
                    {
                        MainPivot.Items.Remove(PI_AccountInput);
                    }
                    if (MainPivot.Items.Contains(PI_ScanQRCodeCamera))
                    {
                        MainPivot.Items.Remove(PI_ScanQRCodeCamera);
                    }
                    ViewModel.LastPivotItemName = string.Empty;
                    break;
            }
        }

        private void TBTagChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == TextBlock.TagProperty)
            {
                if (((TextBox)sender).Tag is string tag)
                {
                    if (tag == "ValidationError")
                    {
                        AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
                        {
                            Target = TB_AddAccountContentDialogSecretKey as FrameworkElement,
                            Subtitle = Strings.Resources.AddAccountCodeContentDialogInputSecretKeyHelp,
                            AutoCloseInterval = 3000,
                            IsLightDismissEnabled = false,
                            BorderThickness = new Thickness(2,2,2,2),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(255,255, 28,32)),
                            IsOpen = true,
                        };
                        RootGrid.Children.Add(teachingTip);
                    }
                }
            }
        }

        private void HLBTN_QRCodeInfo(object sender, RoutedEventArgs e)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = sender as FrameworkElement,
                MaxWidth = 400,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogQRCodeHelp,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true
                //HeroContent = new Image
                //{
                //    Source = new BitmapImage(new Uri("ms-appx:///Assets/Tutorials/2fast_qrcode_scan.gif", UriKind.Absolute)),
                //    MaxWidth = 400,
                //    MaxHeight = 300
                //}
            };
            RootGrid.Children.Add(teachingTip);
        }

        private void HLBTN_SecretKeyInfo(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogInputSecretKeyHelp,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.SearchAccountFonts(sender.Text);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is FontIdentifikationModel selectedItem)
            {
                if (selectedItem.Name != Strings.Resources.AccountCodePageSearchNotFound)
                {
                    ViewModel.AccountIconName = selectedItem.Name;
                }
                else
                {
                    sender.Text = string.Empty;
                }
            }
        }

        //private void BTN_EditAccountIcon_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!MainPivot.Items.Contains(PI_AccountInput))
        //    {
        //        MainPivot.Items.Add(PI_AccountInput);
        //        MainPivot.Items.Remove(PI_ImportAccountBackup);
        //    }
        //}

        private void REB_Notes_TextChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.Model.Notes = Toolbar.Formatter?.Text;
        }

        private void BTN_Expertsettings_Help_Click(object sender, RoutedEventArgs e)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogExpertSettingsHelp,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            RootGrid.Children.Add(teachingTip);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TwoFACodeModel model)
            {
                if (model.IsEnabled)
                {
                    model.IsChecked = !model.IsChecked;
                }
            }
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchAccountFonts(ViewModel.AccountIconName);
        }

        private void BTN_SearchIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                FlyoutBase.ShowAttachedFlyout(btn);
            }
        }

        private void HLBTN_CategoryInfo(object sender, RoutedEventArgs e)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = sender as FrameworkElement,
                MaxWidth = 400,
                Subtitle = Strings.Resources.EditAccountContentDialogAccountCategoryInfoText,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true
            };
            RootGrid.Children.Add(teachingTip);
        }

        /// <summary>
        /// Is triggered when the element is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TokenView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is CategoryModel model)
            {
                model.IsSelected = !model.IsSelected;
            }
        }

        private void SettingsExpander_Expanded(object sender, System.EventArgs e)
        {
            SV_AccountInput.ScrollToElement(sender as FrameworkElement);
        }

        private void BTN_ManualInput_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                RemovePivotItems(PI_AccountInput);
                if (MainPivot.Items.Count > 1)
                {
                    ViewModel.SelectedPivotIndex = 1;
                }
            }
        }

        private void BTN_QRCodeCameraScan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                RemovePivotItems(PI_ScanQRCodeCamera);
                if (MainPivot.Items.Count > 1)
                {
                    ViewModel.SelectedPivotIndex = 1;
                }
            }
        }

        private void BTN_QRCodeScan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                RemovePivotItems(PI_AccountInput);
                if (MainPivot.Items.Count > 1)
                {
                    ViewModel.SelectedPivotIndex = 1;
                }
            }
        }
    }
}
