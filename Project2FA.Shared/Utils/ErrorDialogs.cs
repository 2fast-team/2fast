using System;
using UNOversal.Ioc;
using Project2FA.Strings;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Dialogs;
using Project2FA.Services;
using Windows.Storage;
using Project2FA.Core;
using Project2FA.Repository.Models;
using UNOversal.Services.Secrets;
using Windows.Security.Authorization.AppCapabilityAccess;
using Windows.UI.Popups;


#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using WinUIWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Project2FA.Utils
{
    internal static class ErrorDialogs
    {
        /// <summary>
        /// Displays that the system time is not correct
        /// </summary>
        /// <returns></returns>
        internal static Task SystemTimeNotCorrectError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AccountCodePageWrongTimeTitle;
            dialog.Content = Resources.AccountCodePageWrongTimeContent;
            dialog.PrimaryButtonText = Resources.AccountCodePageWrongTimeBTN;
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:dateandtime"));
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.SecondaryButtonText = Resources.Confirm;
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        internal static async Task ShowUnauthorizedAccessError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.AuthorizationFileSystemContentDialogTitle;
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            dialog.Content = markdown;
#else
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            textBlock.TextWrapping = TextWrapping.WrapWholeWords;
            dialog.Content = textBlock;
#endif
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonText = Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
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
        internal static async Task ShowPasswordError()
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();

            ContentDialog dialog = new ContentDialog
            {
                Title = Resources.PasswordInvalidHeader,
                PrimaryButtonText = Resources.ChangePassword,
                PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style,

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                PrimaryButtonCommand = new RelayCommand(async () =>
                {
                    var dialog = new ChangeDatafilePasswordContentDialog();
                    var param = new DialogParameters();
                    param.Add("isInvalid", true);
                    var result = await dialogService.ShowDialogAsync(dialog, param);
                    
                    if (!dialog.ViewModel.PasswordChanged)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        ShowPasswordError();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }),
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

                SecondaryButtonText = Resources.CloseApp,
                SecondaryButtonCommand = new RelayCommand(() =>
                {
                    App.Current.Exit();
                })
            };
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif

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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ShowPasswordError();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        internal static Task UnauthorizedAccessDialog()
        {
            var dialog = new ContentDialog
            {
                Title = Strings.Resources.AuthorizationFileSystemContentDialogTitle
            };
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            var markdown = new MarkdownTextBlock
            {
                Text = Strings.Resources.AuthorizationFileSystemContentDialogDescription
            };
            dialog.Content = markdown;
#else
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            textBlock.TextWrapping = TextWrapping.WrapWholeWords;
            dialog.Content = textBlock;
#endif
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
            dialog.Style = App.Current.Resources[Constants.ContentDialogStyleName] as Style;
            dialog.PrimaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
            dialog.SecondaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                App.Current.Exit();
            });
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        internal static Task UnauthorizedAccessUseLocalFileDialog()
        {
            var dialog = new ContentDialog
            {
                Title = Strings.Resources.AuthorizationFileSystemContentDialogTitle
            };
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            var markdown = new MarkdownTextBlock
            {
                Text = Strings.Resources.AuthorizationFileSystemContentDialogDescription
            };
            dialog.Content = markdown;
#else
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            textBlock.TextWrapping = TextWrapping.WrapWholeWords;
            dialog.Content = textBlock;
#endif
            dialog.Style = App.Current.Resources[Constants.ContentDialogStyleName] as Style;
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                var result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                App.Current.Exit();
            });
            dialog.CloseButtonCommand = new RelayCommand(() =>
            {
                dialog.Hide();
            });
            dialog.CloseButtonText = Resources.ButtonTextCancel;
            dialog.PrimaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
            //dialog.SecondaryButtonText = Strings.Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            //dialog.SecondaryButtonCommand = new RelayCommand(() =>
            //{
            //    Prism.PrismApplicationBase.Current.Exit();
            //});
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            return dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        internal async static Task ShowUnexpectedError(Exception exc)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            var errorTextBlock = new MarkdownTextBlock()
            {
                Margin = new Thickness(0, 8, 0, 8)
            };
            errorTextBlock.Text = "#" + exc.Message  + "\n"
                + exc.Source + "\n"
                + exc.StackTrace + "\n"
                + exc.InnerException;
#else
            var errorTextBlock = new TextBlock()
            {
                Margin = new Thickness(0, 8, 0, 8),
                TextWrapping = TextWrapping.WrapWholeWords
            };
            errorTextBlock.Text = exc.Message
                + exc.StackTrace
                + exc.InnerException;
#endif
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
                Content = "Github issues"
            };
            githubButton.Click += async (s, e) =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/2fast-team/2fast/issues"));
            };
            var feedbackHub = new Button
            {
                Margin = new Thickness(4, 0, 0, 0),
                Content = "Github dicussions"
            };
#if WINDOWS_UWP
            feedbackHub.Click += async (s, e) =>
            {
                // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
                //var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                //await launcher.LaunchAsync();
                await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/2fast-team/2fast/discussions"));
            };
#endif
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Style = App.Current.Resources[Constants.ContentDialogStyleName] as Style;
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ShowUnexpectedError(exc);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async static Task ShowUnexpectedError(string errorDetail)
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            var errorMarkdownTextBlock = new MarkdownTextBlock()
            {
                Margin = new Thickness(0, 4, 0, 0)
            };
            errorMarkdownTextBlock.Text = errorDetail;
#else
            var errorMarkdownTextBlock = new TextBlock()
            {
                Margin = new Thickness(0, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            errorMarkdownTextBlock.Text = errorDetail;
#endif
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
            var buttonStackPanel = new StackPanel();
            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(githubButton);
            //buttonStackPanel.Children.Add(feedbackHub);
            stackpanel.Children.Add(buttonStackPanel);

            dialog.Style = App.Current.Resources[Constants.ContentDialogStyleName] as Style;
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

        internal async static Task<ContentDialogResult> WritingDatafileError(bool isNewFile)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.Error;
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
#else
            TextBlock markdown = new TextBlock();
            markdown.TextWrapping = TextWrapping.Wrap;
#endif
            markdown.Text = string.Format(Resources.WriteDatafileErrorDesc);
            dialog.Content = markdown;
            dialog.PrimaryButtonCommand = new RelayCommand(() =>
            {
                if (!isNewFile)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    DataService.Instance.WriteLocalDatafile();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            });
            dialog.PrimaryButtonText = Resources.WriteDatafileErrorBTNRetry;
            dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
            dialog.SecondaryButtonText = Resources.WriteDatafileErrorBTNCancel;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                // ReloadDatafile();
            });
            return await dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        internal async static Task SecretKeyError(string label)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.Error;
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
#else
            TextBlock markdown = new TextBlock();
            markdown.TextWrapping = TextWrapping.Wrap;
#endif
            markdown.Text = string.Format(Resources.ErrorGenerateTOTPCode, label);
            dialog.Content = markdown;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonCommand = new RelayCommand(async () =>
            {
                await CoreApplication.RequestRestartAsync("NullableSecretKey");
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            dialog.PrimaryButtonText = Resources.RestartApp;
            dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
            dialog.SecondaryButtonText = Resources.Confirm;
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                //Prism.PrismApplicationBase.Current.Exit();
            });
            await dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        /// <summary>
        /// Displays a FileNotFoundException message and the option for factory reset or correcting the path
        /// </summary>
        internal async static Task ShowFileOrFolderNotFoundError(DBDatafileModel datafileModel = null)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            // check if the app has file system access to load the file
            var checkFileSystemAccess = AppCapability.Create(Constants.BroadFileSystemAccessName).CheckAccess();
            if (checkFileSystemAccess == AppCapabilityAccessStatus.Allowed)
            {
                if (datafileModel != null && datafileModel.IsWebDAV)
                {
                    // TODO WebDav case
                }
                // disable shell navigation
                App.ShellPageInstance.ViewModel.NavigationIsAllowed = false;
                //Logger.Log("no datafile found", Category.Exception, Priority.High);
                bool selectedOption = false;

                ContentDialog dialog = new ContentDialog();
                dialog.Closed += Dialog_Closed;
                dialog.Title = Resources.ErrorHandle;
                StackPanel stackPanel = new StackPanel();
#if !WINDOWS_UWP
                dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
                MarkdownTextBlock markdown = new MarkdownTextBlock();
#else
                TextBlock markdown = new TextBlock();
                markdown.TextWrapping = TextWrapping.Wrap;
#endif
                markdown.Text = Resources.ExceptionDatafileNotFound;
                stackPanel.Children.Add(markdown);

                Button changePathBTN = new Button();
                changePathBTN.Margin = new Thickness(0, 10, 0, 0);
                changePathBTN.Content = Resources.ChangeDatafilePath;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                changePathBTN.Command = new RelayCommand(async () =>
                {
                    selectedOption = true;
                    dialog.Hide();
                    ContentDialogResult result = await dialogService.ShowDialogAsync(new UpdateDatafileContentDialog(), new DialogParameters());
                    if (result == ContentDialogResult.Primary)
                    {
                        DataService.Instance.ErrorResolved();
                        // allow shell navigation
                        App.ShellPageInstance.ViewModel.NavigationIsAllowed = true;
                        await DataService.Instance.ReloadDatafile();
                    }
                    if (result == ContentDialogResult.None)
                    {
                        await ShowFileOrFolderNotFoundError();
                    }
                });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
                stackPanel.Children.Add(changePathBTN);

                Button factoryResetBTN = new Button();
                factoryResetBTN.Margin = new Thickness(0, 10, 0, 10);
                factoryResetBTN.Content = Resources.FactoryReset;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                factoryResetBTN.Command = new RelayCommand(async () =>
                {
                    ISecretService _secretService = App.Current.Container.Resolve<ISecretService>();
                    DBPasswordHashModel passwordHash = await App.Repository.Password.GetAsync();
                    //delete password in the secret vault
                    _secretService.Helper.RemoveSecret(Constants.ContainerName, passwordHash.Hash);
                    // reset data and restart app
                    await ApplicationData.Current.ClearAsync();
                    await CoreApplication.RequestRestartAsync("Factory reset");
                });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

                stackPanel.Children.Add(factoryResetBTN);

                dialog.Content = stackPanel;
                dialog.PrimaryButtonText = Resources.CloseApp;
                dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
                dialog.PrimaryButtonCommand = new RelayCommand(() =>
                {
                    App.Current.Exit();
                });

                async void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
                {
                    if (!(Window.Current.Content is ShellPage))
                    {
                        App.Current.Exit();
                    }
                    else
                    {
                        if (!selectedOption)
                        {
                            await dialogService.ShowDialogAsync(dialog, new DialogParameters());
                        }
                    }
                }
                await dialogService.ShowDialogAsync(dialog, new DialogParameters());
            }
            else
            {
                await UnauthorizedAccessUseLocalFileDialog();
            }
        }

        internal async static Task ShowLogNotFound()
        {
            var dialog = new ContentDialog
            {
                Title = Strings.Resources.SettingsPageNoLogDialogTitle,
                Content = Strings.Resources.SettingsPageNoLogDialogContent,
                PrimaryButtonText = Strings.Resources.Confirm
            };
#if !WINDOWS_UWP
                dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif

            await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
        }

        internal static Task WritingFatalRestoreError()
        {
            // TODO generate
            throw new NotImplementedException();
        }

        internal async static Task CorruptDataFileError()
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle,
                Content = Resources.FileActivationPageViewModelCorruptDatafile,
                PrimaryButtonText = Strings.Resources.Confirm
            };
#if !WINDOWS_UWP
                dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif

            await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
        }

        internal async static Task EmptyDataFileError()
        {
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle,
                Content = Resources.FileActivationPageViewModelEmptyDatafile,
                PrimaryButtonText = Strings.Resources.Confirm
            };
#if !WINDOWS_UWP
                dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif

            await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
        }

        internal async static Task QRReadError()
        {
            MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogQRCodeContentError, Strings.Resources.Error);
            await dialog.ShowAsync();
        }
    }
}
