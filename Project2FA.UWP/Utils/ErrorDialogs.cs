using Microsoft.Toolkit.Uwp.UI.Controls;
using Prism.Commands;
using System;
using Prism.Ioc;
using Template10.Services.Dialog;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Project2FA.UWP.Strings;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Project2FA.UWP.Services;

namespace Project2FA.UWP.Utils
{
    public static class ErrorDialogs
    {
        public async static void UnauthorizedAccessDialog()
        {
            var dialog = new ContentDialog
            {
                Title = Strings.Resources.AuthorizationFileSystemContentDialogTitle
            };
            var markdown = new MarkdownTextBlock
            {
                Text = Strings.Resources.AuthorizationFileSystemContentDialogDescription
            };
            dialog.Content = markdown;
            dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                Prism.PrismApplicationBase.Current.Exit();
            });
            dialog.PrimaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            await dialogService.ShowAsync(dialog);
        }

        public async static void ShowUnexpectedError(Exception exc)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
            var errorTextBox = new TextBox()
            {
                Margin = new Thickness(0, 4, 0, 0)
            };
            errorTextBox.Text = exc.Message + "\n"
                + exc.StackTrace + "\n"
                + exc.InnerException;
            errorTextBox.IsReadOnly = true;
            var stackpanel = new StackPanel();
            var clipboardButton = new Button();
            clipboardButton.Margin = new Thickness(0, 10, 0, 0);
            clipboardButton.Content = Resources.ErrorCopyToClipboard;
            clipboardButton.Click += (s, e) =>
            {
                DataPackage dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                dataPackage.SetText(errorTextBox.Text);
                Clipboard.SetContent(dataPackage);
            };
            stackpanel.Children.Add(new TextBlock() { Text = Resources.ErrorHandleDescription, TextWrapping = TextWrapping.WrapWholeWords });
            stackpanel.Children.Add(clipboardButton);
            stackpanel.Children.Add(errorTextBox);

            var githubButton = new Button
            {
                Content = "Github"
            };
            githubButton.Click += async (s, e) =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/2fast-team/2fast/issues"));
            };
            var feedbackHub = new Button
            {
                Margin = new Thickness(4, 0, 0, 0),
                Content = "Feedback-Hub"
            };
            feedbackHub.Click += async (s, e) =>
            {
                // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            };
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Content = stackpanel;
            dialog.PrimaryButtonText = Resources.RestartApp;
            dialog.SecondaryButtonText = Resources.CloseApp;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                await CoreApplication.RequestRestartAsync("Exception");
            });
            dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });
            var result = await App.Current.Container.Resolve<IDialogService>().ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                ShowUnexpectedError(exc);
            }
        }

        public async static void ShowUnexpectedError(string errorDetail)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
            var errorTextBox = new TextBox()
            {
                Margin = new Thickness(0, 4, 0, 0)
            };
            errorTextBox.Text = errorDetail;
            errorTextBox.IsReadOnly = true;
            var stackpanel = new StackPanel();
            var clipboardButton = new Button();
            clipboardButton.Margin = new Thickness(0, 10, 0, 0);
            clipboardButton.Content = Resources.ErrorCopyToClipboard;
            clipboardButton.Click += (s, e) =>
            {
                DataPackage dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                dataPackage.SetText(errorTextBox.Text);
                Clipboard.SetContent(dataPackage);
            };
            stackpanel.Children.Add(new TextBlock() { Text = Resources.ErrorHandleDescriptionLastSession, TextWrapping = TextWrapping.WrapWholeWords });
            stackpanel.Children.Add(clipboardButton);
            stackpanel.Children.Add(errorTextBox);

            var githubButton = new Button
            {
                Content = "GitHub"
            };
            githubButton.Click += async (s, e) =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/2fast-team/2fast/issues"));
            };
            var feedbackHub = new Button
            {
                Margin = new Thickness(4, 0, 0, 0),
                Content = "Feedback-Hub"
            };
            feedbackHub.Click += async (s, e) =>
            {
                // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            };
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Content = stackpanel;
            dialog.PrimaryButtonText = Resources.Confirm;
            //dialog.SecondaryButtonText = Resources.CloseApp;
            //dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            //dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            //{
            //    await CoreApplication.RequestRestartAsync("Exception");
            //});
            //dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            //{
            //    Prism.PrismApplicationBase.Current.Exit();
            //});
            var result = await App.Current.Container.Resolve<IDialogService>().ShowAsync(dialog);
            SettingsService.Instance.UnhandledExceptionStr = string.Empty;
        }
    }
}
