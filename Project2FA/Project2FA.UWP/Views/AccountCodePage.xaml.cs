using Prism.Ioc;
using Project2FA.Repository.Models;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using UNOversal.Services.Dialogs;
using CommunityToolkit.Mvvm.Input;
using Project2FA.Services;

namespace Project2FA.UWP.Views
{
    public sealed partial class AccountCodePage : Page
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
            if (SettingsService.Instance.IsProVersion)
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplatePro;
            }
            else
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
            }
            //LV_AccountCollection.TabIndex = 1;
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
                if (string.IsNullOrWhiteSpace(sender.Text) == false)
                {
                    try
                    {
                        List<string> _nameList = new List<string>();
                        foreach (TwoFACodeModel item in ViewModel.TwoFADataService.Collection)
                        {
                            _nameList.Add(item.Label);
                        }
                        // search the labels
                        List<string> listSuggestion = _nameList.Where(x => x.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase)).ToList();
                        if (listSuggestion.Count == 0)
                        {
                            listSuggestion.Add(Strings.Resources.AccountCodePageSearchNotFound);
                        }

                        // filter the selected categories
                        if (ViewModel.TwoFADataService.GlobalCategories != null)
                        {
                            var selectedGlobalCategories = ViewModel.TwoFADataService.GlobalCategories.Where(x => x.IsSelected == true);
                            if (selectedGlobalCategories.Any())
                            {
                                // filter where the models have the selected categories and the input label
                                ViewModel.TwoFADataService.ACVCollection.Filter = model => ((TwoFACodeModel)model).SelectedCategories.Where(sc => 
                                selectedGlobalCategories.Any(gc => gc.Guid == sc.Guid)).Any() && ((TwoFACodeModel)model).Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase);

                                // set suggetion where the models have the selected categories and the input label
                                var filteredCollection = ViewModel.TwoFADataService.Collection.Where(model => model.SelectedCategories.Where(sc =>
                                    selectedGlobalCategories.Any(gc => gc.Guid == sc.Guid)).Any() && model.Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase));
                                listSuggestion = listSuggestion.Where(ls => filteredCollection.Where(fc => fc.Label == ls).Any()).ToList();
                                sender.ItemsSource = listSuggestion;
                                //selectedGlobalCategories.Where(c => ViewModel.TwoFADataService.Collection.Where(x => x.SelectedCategories.Where(y => y.Guid == c.Guid).Any()).Any());
                                //ViewModel.TwoFADataService.ACVCollection.Filter = x => 
                                ////((TwoFACodeModel)x).Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase) ||
                                //selectedCategories.Where(c => ((TwoFACodeModel)x).SelectedCategories.Where(y => y.Guid == c.Guid).Any());

                                //works for categories
                                //ViewModel.TwoFADataService.ACVCollection.Filter = model => ((TwoFACodeModel)model).SelectedCategories.Where(sc =>
                                //selectedGlobalCategories.Any(gc => gc.Guid == sc.Guid)).Any();
                            }
                            else
                            {
                                ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x).Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase);
                                sender.ItemsSource = listSuggestion;
                            }
                        }
                        // no categories set
                        else
                        {
                            ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x).Label.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase);
                            sender.ItemsSource = listSuggestion;
                        }

                        
                    }
                    catch (System.Exception exc)
                    {
                        ViewModel.TwoFADataService.ACVCollection.Filter = null;
                        TrackingManager.TrackExceptionCatched(nameof(AutoSuggestBox_TextChanged),exc);
                    }
                    
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
        }
    }
}