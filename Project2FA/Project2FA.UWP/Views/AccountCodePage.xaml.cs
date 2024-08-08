using Prism.Ioc;
using Project2FA.Repository.Models;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using UNOversal.Services.Dialogs;
using CommunityToolkit.Mvvm.Input;
using Project2FA.Services;
using Windows.UI.Xaml.Controls.Primitives;

namespace Project2FA.UWP.Views
{
    public sealed partial class AccountCodePage : Page
    {
        private bool _menuKeyPressed = false;
        private string _tempNumber = string.Empty;
        AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;

        public AccountCodePage()
        {
            this.InitializeComponent();
            this.Loaded += AccountCodePage_Loaded;
        }

        private void AccountCodePage_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = ViewModel;
            if (SettingsService.Instance.IsProVersion)
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplatePro;
            }
            else
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
            }
            //App.ShellPageInstance.ShellViewInternal.Focus(FocusState.Unfocused);

        }

        /// <summary>
        /// Copy the 2fa code to clipboard and create a user dialog
        /// </summary>
        /// <param name="model"></param>
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
            catch (System.Exception)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = Strings.Resources.ErrorHandle;
                dialog.Content = Strings.Resources.ErrorClipboardTask;
                dialog.PrimaryButtonText = Strings.Resources.ButtonTextRetry;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
                {
                    await Copy2FACodeToClipboard(model);
                });
                dialog.SecondaryButtonText = Strings.Resources.ButtonTextCancel;
                await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
                return false;
            }
        }

        private void CreateTeachingTip(FrameworkElement element)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = element,
                Content = Strings.Resources.AccountCodePageCopyCodeTeachingTip,
                AutoCloseInterval = 1000,
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
                if(await Copy2FACodeToClipboard(model))
                {
                    CreateTeachingTip(sender as FrameworkElement);
                }
            }
        }

        /// <summary>
        /// Copy the 2fa code to clipboard when click with 'right click' and create a user dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TwoFACodeItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
                {
                    if(await Copy2FACodeToClipboard(model))
                    {
                        CreateTeachingTip(sender as FrameworkElement);
                    }
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.SetSuggestionList(sender.Text);
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

        /// <summary>
        /// Copy the account code from selected item via Tab and Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LV_AccountCollection_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (LV_AccountCollection.SelectedItem is TwoFACodeModel model)
                {
                    if (await Copy2FACodeToClipboard(model))
                    {
                        CreateTeachingTip(e.OriginalSource as FrameworkElement);
                    }
                }
            }
            if (e.KeyStatus.IsMenuKeyDown)
            {
                if ((int)e.Key >= 96 && (int)e.Key <= 105)
                {
                    switch (e.Key)
                    {
                        case Windows.System.VirtualKey.NumberPad0:
                        case Windows.System.VirtualKey.Number0:
                            _tempNumber += "0";
                            break;
                        case Windows.System.VirtualKey.NumberPad1:
                        case Windows.System.VirtualKey.Number1:
                            _tempNumber += "1";
                            break;
                        case Windows.System.VirtualKey.NumberPad2:
                        case Windows.System.VirtualKey.Number2:
                            _tempNumber += "2";
                            break;
                        case Windows.System.VirtualKey.NumberPad3:
                        case Windows.System.VirtualKey.Number3:
                            _tempNumber += "3";
                            break;
                        case Windows.System.VirtualKey.NumberPad4:
                        case Windows.System.VirtualKey.Number4:
                            _tempNumber += "4";
                            break;
                        case Windows.System.VirtualKey.NumberPad5:
                        case Windows.System.VirtualKey.Number5:
                            _tempNumber += "5";
                            break;
                        case Windows.System.VirtualKey.NumberPad6:
                        case Windows.System.VirtualKey.Number6:
                            _tempNumber += "6";
                            break;
                        case Windows.System.VirtualKey.NumberPad7:
                        case Windows.System.VirtualKey.Number7:
                            _tempNumber += "7";
                            break;
                        case Windows.System.VirtualKey.NumberPad8:
                        case Windows.System.VirtualKey.Number8:
                            _tempNumber += "8";
                            break;
                        case Windows.System.VirtualKey.NumberPad9:
                        case Windows.System.VirtualKey.Number9:
                            _tempNumber += "9";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_tempNumber))
                {
                    if (int.TryParse(_tempNumber, out int result))
                    {
                        if (result != 0 && result <= ViewModel.TwoFADataService.Collection.Count)
                        {
                            ViewModel.SelectedIndex = result - 1;
                        }
                        _tempNumber = string.Empty;
                    }
                    else
                    {
                        _tempNumber = string.Empty;
                    }
                }
            }
        }

        private void ABB_SearchFilter_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterIsActive = false;
            if (sender is AppBarButton abbtn)
            {
                //abbtn.Flyout ??= new Flyout();
                //abbtn.Flyout.
                //CategoryFilterFlyout categoryFlyout = new CategoryFilterFlyout();
                //FlyoutBase.SetAttachedFlyout(abbtn, (Flyout)Page.Resources["CategoryFilterFlyout"]);
                FlyoutBase.ShowAttachedFlyout(abbtn);
                ViewModel.SendCategoryFilterUpdatate();
            }
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SetSuggestionList(ViewModel.SearchedAccountLabel);
        }

        private void HLBTN_ProFeature(object sender, RoutedEventArgs e)
        {

        }

        private void MainGrid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //LV_AccountCollection.Focus(FocusState.Keyboard);
            //LV_AccountCollection.Focus(FocusState.Programmatic);
        }
    }
}