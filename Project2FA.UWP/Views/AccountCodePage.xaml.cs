using Prism.Navigation;
using Prism.Ioc;
using Project2FA.Repository.Models;
using Project2FA.UWP.Controls;
using Project2FA.UWP.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Prism.Services.Dialogs;

namespace Project2FA.UWP.Views
{
    public sealed partial class AccountCodePage : Page, IConfirmNavigationAsync
    {
        AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;

        public AccountCodePage()
        {
            this.InitializeComponent();
            this.Loaded += AccountCodePage_Loaded;
        }

        private void AccountCodePage_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = ViewModel;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        /// <summary>
        /// Copy the 2fa code to clipboard and create a user dialog
        /// </summary>
        /// <param name="model"></param>
        private void Copy2FACodeToClipboard(TwoFACodeModel model)
        {
            DataPackage dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(model.TwoFACode);
            Clipboard.SetContent(dataPackage);
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
        private void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                CreateTeachingTip(sender as FrameworkElement);
                Copy2FACodeToClipboard(model);
            }
        }

        /// <summary>
        /// Copy the 2fa code to clipboard when click with 'right click' and create a user dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TwoFACodeItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
                {
                    CreateTeachingTip(sender as FrameworkElement);
                    Copy2FACodeToClipboard(model);
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text) == false)
                {
                    List<string> _nameList = new List<string>();
                    foreach (TwoFACodeModel item in ViewModel.TwoFADataService.Collection)
                    {
                        _nameList.Add(item.Label);
                    }
                    List<string> listSuggestion = _nameList.Where(x => x.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase)).ToList();
                    if (listSuggestion.Count == 0)
                    {
                        listSuggestion.Add(Strings.Resources.AccountCodePageSearchNotFound);
                    }
                    sender.ItemsSource = listSuggestion;
                    ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x).Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    sender.ItemsSource = null;
                    if (ViewModel.TwoFADataService.ACVCollection.Filter != null)
                    {
                        ViewModel.TwoFADataService.ACVCollection.Filter = null;
                    }
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string selectedItem = args.SelectedItem.ToString();
            if (selectedItem != Strings.Resources.AccountCodePageSearchNotFound)
            {
                ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x).Label == selectedItem;
            }
            else
            {
                sender.Text = string.Empty;
            }
        }

        async Task<bool> IConfirmNavigationAsync.CanNavigateAsync(INavigationParameters parameters)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            return !await dialogService.IsDialogRunning();
        }
    }
}