using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Project2FA.Controls;
using Project2FA.Repository.Models;
using Project2FA.UnoApp;
using Project2FA.ViewModels;
using UNOversal.Ioc;
using UNOversal.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace Project2FA.Uno.Views;

public sealed partial class AccountCodePage : Page
{
    public AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;
    public AccountCodePage()
    {
        this.InitializeComponent();
        // Refresh x:Bind when the DataContext changes.
        DataContextChanged += (s, e) => Bindings.Update();
        // Evcnt for the native back behavior which currently skips the framework extension
#if __ANDROID__ || __IOS__
        App.ShellPageInstance.MainFrame.Navigated -= MainFrame_Navigated;
        App.ShellPageInstance.MainFrame.Navigated += MainFrame_Navigated;
        PropertyChangedCallback callback = new PropertyChangedCallback(SelectedTabBarIndexChanged);
        //register an event for the changed selected index property of the TabBar
        MobileAutoSuggestBox.RegisterDisposablePropertyChangedCallback(VisibilityProperty, SelectedTabBarIndexChanged);
        //App.ShellPageInstance.ViewModel.TabBarIsVisible = true;
#endif
    }

    private void SelectedTabBarIndexChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var visibilty = (Visibility)args.NewValue;
        switch (visibilty)
        {
            case Visibility.Collapsed:
                var fadeOutStoryboard = new Storyboard();
                var fadeAnimation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(TimeSpan.FromSeconds(1.0)) };
                Storyboard.SetTarget(fadeAnimation, MobileAutoSuggestBox);
                Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
                fadeOutStoryboard.Children.Add(fadeAnimation);
                fadeOutStoryboard.Begin();
                break;
            case Visibility.Visible:
                var fadeInStoryboard = new Storyboard();
                var fadeInAnimation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(TimeSpan.FromSeconds(1.0)) };
                Storyboard.SetTarget(fadeInAnimation, MobileAutoSuggestBox);
                Storyboard.SetTargetProperty(fadeInAnimation, "Opacity");
                fadeInStoryboard.Children.Add(fadeInAnimation);
                fadeInStoryboard.Begin();
                MobileAutoSuggestBox.Focus(FocusState.Programmatic);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Detects the native back behavior which currently skips the framework extension
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        App.ShellPageInstance.ViewModel.TabBarIsVisible = true;
        if (e.NavigationMode == Microsoft.UI.Xaml.Navigation.NavigationMode.Back)
        {
            ViewModel.Initialize(new NavigationParameters());

            //set current index for TabBar on smartphones
#if __ANDROID__ || __IOS__
            if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            {
                switch (uIElement)
                {
                    case AccountCodePage:
                        App.ShellPageInstance.ViewModel.SelectedIndex = 0;
                        break;
                    case SettingPage:
                        App.ShellPageInstance.ViewModel.SelectedIndex = 2;
                        break;
                    default:
                        break;
                }
            }
#endif
        }
    }

    private async Task<bool> Copy2FACodeToClipboard(TwoFACodeModel model)
    {
        try
        {
            DataPackage dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(model.TwoFACode);
            Clipboard.SetContent(dataPackage);
            return true;
        }
        catch (System.Exception exc)
        {
            //ContentDialog dialog = new ContentDialog();
            //dialog.Title = Strings.Resources.ErrorHandle;
            //dialog.Content = Strings.Resources.ErrorClipboardTask;
            //dialog.PrimaryButtonText = Strings.Resources.ButtonTextRetry;
            //dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            //dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
            //{
            //    await Copy2FACodeToClipboard(model);
            //});
            //dialog.SecondaryButtonText = Strings.Resources.ButtonTextCancel;
            //await App.Current.Container.Resolve<ILoggingService>().LogException(exc, SettingsService.Instance.LoggingSetting);
            //await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
            return false;
        }
    }

    private void CreateTeachingTip(FrameworkElement element)
    {
        TeachingTip teachingTip = new TeachingTip
        {
            Target = element,
            Content = Strings.Resources.AccountCodePageCopyCodeTeachingTip,
            IsLightDismissEnabled = true,
            BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
            IsOpen = true,
        };
        MainGrid.Children.Add(teachingTip);
    }

    /// <summary>
    /// Copies the current generated TOTP of the entry into the clipboard
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
        {
            if (await Copy2FACodeToClipboard(model))
            {
                CreateTeachingTip(sender as FrameworkElement);
            }
        }
    }

    private async void BTN_EditItem_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
        {
            await ViewModel.EditAccountCommandTask(model);
        }
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.SetSuggestionList(sender.Text,true);
        }
    }

    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (args.SelectedItem is TwoFACodeModel item)
        {
            if (item.Label != Strings.Resources.AccountCodePageSearchNotFound)
            {
                ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x) == item;
                ViewModel.SearchedAccountLabel = item.Label;
            }
            else
            {
                ViewModel.SearchedAccountLabel = string.Empty;
                ViewModel.TwoFADataService.ACVCollection.Filter = null;
            }
        }
        else
        {
            ViewModel.SearchedAccountLabel = string.Empty;
            ViewModel.TwoFADataService.ACVCollection.Filter = null;
        }
    }

    private async void BTN_AddAccountManual_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.AddAccountManual();
    }

    private async void BTN_AddAccountCamera_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.AddAccountWithCamera();
    }

    private async void BTN_ShareItem_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
        {
            await ViewModel.ExportQRCodeCommandTask(model);
        }
    }
    private void ABB_ShowActions_Click(object sender, RoutedEventArgs e)
    {
        if (sender is AppBarButton abbtn)
        {
            FlyoutBase.ShowAttachedFlyout(abbtn);
            //ViewModel.SendCategoryFilterUpdatate();
        }
    }

    private async void BTN_DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
        {
            await ViewModel.DeleteAccountCommandTask(model);   
        }
    }

    private async void BTN_SetFavourite_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button mfi && mfi.DataContext is TwoFACodeModel model)
        {
            await ViewModel.SetFavouriteCommandTask(model);
        }
    }

    private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSuggestionList(ViewModel.SearchedAccountLabel, false);
    }

    private void BTN_ShowCode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TwoFACodeModel model)
        {
            ViewModel.HideOrShowTOTPCodeCommandTask(model);
        }
    }

    private async void MFI_DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi && mfi.DataContext is TwoFACodeModel model)
        {
            await ViewModel.DeleteAccountCommandTask(model);
        }
    }

    private async void MFI_EditAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi && mfi.DataContext is TwoFACodeModel model)
        {
            await ViewModel.EditAccountCommandTask(model);
        }
    }

    private async void MFI_ExportAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi && mfi.DataContext is TwoFACodeModel model)
        {
            await ViewModel.ExportQRCodeCommandTask(model);
        }
    }
}
