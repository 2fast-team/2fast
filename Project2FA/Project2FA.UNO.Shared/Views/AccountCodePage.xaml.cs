using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project2FA.ViewModels;
using Windows.UI;
using Project2FA.Repository.Models;
using UNOversal.Navigation;
using Microsoft.UI.Xaml.Media.Animation;

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
            DataContextChanged += (s, e) => Bindings.Update();
            // Evcnt for the native back behavior which currently skips the framework extension
#if ANDROID || IOS
            App.ShellPageInstance.MainFrame.Navigated -= MainFrame_Navigated;
            App.ShellPageInstance.MainFrame.Navigated += MainFrame_Navigated;
            PropertyChangedCallback callback = new PropertyChangedCallback(SelectedTabBarIndexChanged);
            //register an event for the changed selected index property of the TabBar
            MobileAutoSuggestBox.RegisterDisposablePropertyChangedCallback(VisibilityProperty, SelectedTabBarIndexChanged);
            App.ShellPageInstance.ViewModel.TabBarIsVisible = true;
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
#if ANDROID || IOS
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
                ViewModel.SetSuggestionList(sender, args);
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
    }
}
