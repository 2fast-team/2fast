using System;
using Prism.Ioc;
using Project2FA.Strings;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using Project2FA.Services;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using CommunityToolkit.WinUI.UI.Controls;
#endif

namespace Project2FA.Utils
{
    public static class ErrorDialogs
    {
        /// <summary>
        /// Displays that the system time is not correct
        /// </summary>
        /// <returns></returns>
        public static Task SystemTimeNotCorrectError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AccountCodePageWrongTimeTitle;
            dialog.Content = Resources.AccountCodePageWrongTimeContent;
            dialog.PrimaryButtonText = Resources.AccountCodePageWrongTimeBTN;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:dateandtime"));
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.SecondaryButtonText = Resources.Confirm;
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }
        public static async Task ShowUnauthorizedAccessError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AuthorizationFileSystemContentDialogTitle;
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            dialog.Content = markdown;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonText = Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                App.Current.Exit();
            });
            ContentDialogResult result = await dialogService.ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.None)
            {
                App.Current.Exit();
            }
        }

        /// <summary>
        /// Displays a wrong password error message and option to change the password
        /// </summary>
        public static async Task ShowPasswordError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();

            ContentDialog dialog = new ContentDialog
            {
                Title = Resources.PasswordInvalidHeader,
                PrimaryButtonText = Resources.ChangePassword,
                PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style,

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                PrimaryButtonCommand = new RelayCommand(async () =>
                {
                    var dialog = new ChangeDatafilePasswordContentDialog();
                    var param = new DialogParameters();
                    param.Add("isInvalid", true);
                    await dialogService.ShowDialogAsync(dialog, param);
                }),
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

                SecondaryButtonText = Resources.CloseApp,
                SecondaryButtonCommand = new RelayCommand(() =>
                {
                    App.Current.Exit();
                })
            };

#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.PasswordInvalidMessage;
            dialog.Content = markdown;
#else
            TextBlock txt = new TextBlock();
            txt.Text = Resources.PasswordInvalidMessage;
            dialog.Content = txt;
#endif

            ContentDialogResult result = await dialogService.ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.None)
            {
                ShowPasswordError();
            }
        }

        

        public static Task UnauthorizedAccessDialog()
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
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                App.Current.Exit();
            });
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public static Task UnauthorizedAccessUseLocalFileDialog()
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
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
            dialog.CloseButtonCommand = new RelayCommand(() =>
            {
                dialog.Hide();
            });
            dialog.CloseButtonText = Resources.ButtonTextCancel;
            dialog.PrimaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            //dialog.SecondaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            //dialog.SecondaryButtonCommand = new RelayCommand(() =>
            //{
            //    Prism.PrismApplicationBase.Current.Exit();
            //});
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public async static Task ShowUnexpectedError(Exception exc)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
            var errorTextBlock = new MarkdownTextBlock()
            {
                Margin = new Thickness(0, 8, 0, 8)
            };
            errorTextBlock.Text = "#" + exc.Message  + "\n"
                + exc.StackTrace + "\n"
                + exc.InnerException;
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
                dataPackage.SetText(errorTextBlock.Text);
                Clipboard.SetContent(dataPackage);
            };
            stackpanel.Children.Add(new TextBlock() { Text = Resources.ErrorHandleDescription, TextWrapping = TextWrapping.WrapWholeWords });
            stackpanel.Children.Add(clipboardButton);
            stackpanel.Children.Add(errorTextBlock);

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
#if WINDOWS_UWP
            feedbackHub.Click += async (s, e) =>
            {
                // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            };
#endif
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.Content = stackpanel;
            dialog.PrimaryButtonText = Resources.RestartApp;
            dialog.SecondaryButtonText = Resources.CloseApp;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await CoreApplication.RequestRestartAsync("Exception");
            });
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                App.Current.Exit();
            });
            var result = await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.None)
            {
                ShowUnexpectedError(exc);
            }
        }

        public async static Task ShowUnexpectedError(string errorDetail)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
            var errorMarkdownTextBlock = new MarkdownTextBlock()
            {
                Margin = new Thickness(0, 4, 0, 0)
            };
            errorMarkdownTextBlock.Text = errorDetail;
            //errorMarkdownTextBlock.IsReadOnly = true;
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
                dataPackage.SetText(errorMarkdownTextBlock.Text);
                Clipboard.SetContent(dataPackage);
            };
            stackpanel.Children.Add(new TextBlock()
            {
                Text = Resources.ErrorHandleDescriptionLastSession,
                TextWrapping = TextWrapping.WrapWholeWords }
            );
            stackpanel.Children.Add(clipboardButton);
            stackpanel.Children.Add(errorMarkdownTextBlock);

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
#if WINDOWS_UWP
                // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
#endif
            };
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.Content = stackpanel;
            dialog.PrimaryButtonText = Resources.Confirm;
            //dialog.SecondaryButtonText = Resources.CloseApp;
            //dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            //dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            //{
            //    await CoreApplication.RequestRestartAsync("Exception");
            //});
            //dialog.SecondaryButtonCommand = new RelayCommand(() =>
            //{
            //    Prism.PrismApplicationBase.Current.Exit();
            //});
            var result = await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
            SettingsService.Instance.UnhandledExceptionStr = string.Empty;
        }

        public async static Task<ContentDialogResult> WritingDatafileError(bool isNewFile)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.Error;
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = string.Format(Resources.WriteDatafileErrorDesc);
            dialog.Content = markdown;
            dialog.PrimaryButtonCommand = new RelayCommand(() =>
            {
                if (!isNewFile)
                {
                    DataService.Instance.WriteLocalDatafile();
                }
            });
            dialog.PrimaryButtonText = Resources.WriteDatafileErrorBTNRetry;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Resources.WriteDatafileErrorBTNCancel;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                // ReloadDatafile();
            });
            return await dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }
    }
}
