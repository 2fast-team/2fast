using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarButtons;
using Microsoft.UI.Xaml.Controls;
using Project2FA.Repository.Models;
using Project2FA.UWP.Controls;
using Project2FA.UWP.Extensions;
using Project2FA.UWP.Services;
using Project2FA.UWP.Services.Enums;
using Project2FA.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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

            // remove custom PivotItems
            MainPivot.Items.Remove(PI_ImportAccountBackup);
            MainPivot.Items.Remove(PI_ScanQRCodeCamera);

            // initialize the camera control
            ViewModel.MediaPlayerElementControl = CameraPlayerElement;
        }

        public void BTN_QRCodeScan_Click(object sender, RoutedEventArgs e)
        {
            QRCodeScanTip.IsOpen = true;
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

        private void PivotItemChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if(dp == Pivot.TagProperty)
            {
                if (((Pivot)sender).Tag is string tag)
                {
                    if (tag == "ImportBackupAccounts")
                    {
                        MainPivot.Items.Add(PI_ImportAccountBackup);
                        if (MainPivot.Items.Contains(PI_AccountInput))
                        {
                            MainPivot.Items.Remove(PI_AccountInput);
                        }
                        if (MainPivot.Items.Contains(PI_ScanQRCodeCamera))
                        {
                            MainPivot.Items.Remove(PI_ScanQRCodeCamera);
                        }
                    }
                    if (tag == "NormalInputAccount")
                    {
                        MainPivot.Items.Add(PI_AccountInput);
                        if (MainPivot.Items.Contains(PI_ImportAccountBackup))
                        {
                            MainPivot.Items.Remove(PI_ImportAccountBackup);
                        }
                        if (MainPivot.Items.Contains(PI_ScanQRCodeCamera))
                        {
                            MainPivot.Items.Remove(PI_ScanQRCodeCamera);
                        }
                    }
                    if (tag == "CameraInputAccount")
                    {
                        MainPivot.Items.Add(PI_ScanQRCodeCamera);
                        if (MainPivot.Items.Contains(PI_ImportAccountBackup))
                        {
                            MainPivot.Items.Remove(PI_ImportAccountBackup);
                        }
                        if (MainPivot.Items.Contains(PI_AccountInput))
                        {
                            MainPivot.Items.Remove(PI_AccountInput);
                        }
                    }
                    ViewModel.SelectedPivotIndex = 1;
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
                IsOpen = true,
                HeroContent = new Image
                {
                    Source = new BitmapImage(new Uri("ms-appx:///Assets/Tutorials/2fast_qrcode_scan.gif", UriKind.Absolute)),
                    MaxWidth = 400,
                    MaxHeight = 300
                }
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

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text) == false && sender.Text.Length >= 2)
                {
                    List<string> _nameList = new List<string>();
                    foreach (IconNameModel item in ViewModel.IconNameCollectionModel.Collection)
                    {
                        _nameList.Add(item.Name);
                    }
                    List<string> listSuggestion = _nameList.Where(x => x.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase)).ToList();
                    if (listSuggestion.Count == 0)
                    {
                        listSuggestion.Add(Strings.Resources.AccountCodePageSearchNotFound);
                    }
                    sender.ItemsSource = listSuggestion;
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string selectedItem = args.SelectedItem.ToString();
            if (selectedItem != Strings.Resources.AccountCodePageSearchNotFound)
            {
                ViewModel.Model.AccountIconName = selectedItem;
                ViewModel.AccountIconName = selectedItem;
                ViewModel.LoadIconSVG();
            }
            else
            {
                sender.Text = string.Empty;
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

        private void Root_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                model.IsChecked = !model.IsChecked;
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TwoFACodeModel model)
            {
                model.IsChecked = !model.IsChecked;
            }
        }
    }
}
