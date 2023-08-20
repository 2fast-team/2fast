using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project2FA.ViewModels;
using Prism;
using Windows.UI;
using Project2FA.Repository.Models;
using UNOversal.Services.Dialogs;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Navigation;

namespace Project2FA.UNO.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountCodePage : Page
    {
        public AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;
        public AccountCodePage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            this.Loaded += AccountCodePage_Loaded;
            DataContextChanged += (s, e) => Bindings.Update();
            App.ShellPageInstance.MainFrame.Navigated -= MainFrame_Navigated;
            App.ShellPageInstance.MainFrame.Navigated += MainFrame_Navigated;
        }

        private void AccountCodePage_Loaded(object sender, RoutedEventArgs e)
        {
#if ANDROID || IOS || MACCATALYST
            App.ShellPageInstance.ViewModel.SelectedIndex = 0;
#endif
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == Microsoft.UI.Xaml.Navigation.NavigationMode.Back)
            {
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                ViewModel.Initialize(new NavigationParameters());
            }
        }

        private void CreateTeachingTip(FrameworkElement element)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = element,
                Content = Strings.Resources.AccountCodePageCopyCodeTeachingTip,

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
                //if (await Copy2FACodeToClipboard(model))
                //{
                //    CreateTeachingTip(sender as FrameworkElement);
                //}
                CreateTeachingTip(sender as FrameworkElement);
            }
        }

        private async void BTN_EditItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                await ViewModel.EditAccountFromCollection(model);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text) == false)
                {
                    try
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
                    catch (System.Exception exc)
                    {
                        ViewModel.TwoFADataService.ACVCollection.Filter = null;
                        //TrackingManager.TrackException(nameof(AutoSuggestBox_TextChanged), exc);
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
                await ViewModel.ExportQRCode(model);
            }
        }

        private async void BTN_DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                await ViewModel.DeleteAccountFromCollection(model);   
            }
        }
        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    Bindings.Update();
        //    base.OnNavigatedFrom(e);
        //}
    }
}
