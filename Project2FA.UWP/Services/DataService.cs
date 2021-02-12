using Prism.Mvvm;
using Project2FA.Repository.Models;
using System;
using System.Collections.ObjectModel;
using Prism.Ioc;
using Template10.Services.Secrets;
using Template10.Services.File;
using Project2FA.Core.Services.JSON;
using Windows.Storage;
using System.Collections.Specialized;
using OtpNet;
using Windows.UI.Xaml.Controls;
using Template10.Services.Dialog;
using Prism.Commands;
using Prism.Logging;
using Project2FA.Core.Utils;
using Windows.UI.Xaml;
using System.Threading;
using System.Security.Cryptography;
using Project2FA.UWP.Views;
using Project2FA.UWP.Strings;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.ApplicationModel.Core;
using System.IO;
using Microsoft.Toolkit.Uwp.UI;
using Windows.ApplicationModel.DataTransfer;
using Project2FA.UWP.Utils;

namespace Project2FA.UWP.Services
{
    public class DataService : BindableBase, IDisposable
    {
        public SemaphoreSlim CollectionAccessSemaphore { get; } = new SemaphoreSlim(1, 1);

        private ISecretService SecretService { get; }
        private IDialogService DialogService { get; }
        private ILoggerFacade Logger { get; }
        private IFileService FileService { get; }
        private bool _initialization, _errorOccurred;

        private INewtonsoftJSONService _newtonsoftJSONService { get; }

        public AdvancedCollectionView ACVCollection { get; }
        public ObservableCollection<TwoFACodeModel> Collection { get; } = new ObservableCollection<TwoFACodeModel>();
        private bool _emptyAccountCollectionTipIsOpen;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static DataService Instance { get; } = new DataService();

        /// <summary>
        /// Private Constructor
        /// </summary>
        private DataService()
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            FileService = App.Current.Container.Resolve<IFileService>();
            Logger = App.Current.Container.Resolve<ILoggerFacade>();
            DialogService = App.Current.Container.Resolve<IDialogService>();
            _newtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            ACVCollection = new AdvancedCollectionView(Collection, true);
            ACVCollection.SortDescriptions.Add(new SortDescription("Label", SortDirection.Ascending));
            Collection.CollectionChanged += Accounts_CollectionChanged;
            CheckDatafile();
        }

        /// <summary>
        /// Create TOTP code for collection initialization and write the datafile, if an item added or removed from the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Remove:
                    // normal mode
                    if (_initialization == false)
                    {
                        WriteLocalDatafile();
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            ResetCollection();
                        }
                    }
                    // if the initialization is active
                    else
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            InitializeItem((sender as ObservableCollection<TwoFACodeModel>).Count -1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reset the TOTP code
        /// </summary>
        public async void ResetCollection()
        {
            await CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < Collection.Count; i++)
            {
                InitializeItem(i);
            }
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Reset the period and regnerate the TOTP code
        /// </summary>
        /// <param name="i">The index of the item in the collection</param>
        private void InitializeItem(int i)
        {
            Collection[i].Seconds = Collection[i].Period;
            GenerateTOTP(i);
        }

        /// <summary>
        /// Reloads the datafile in the local database
        /// </summary>
        public async void ReloadDatafile()
        {
            await CollectionAccessSemaphore.WaitAsync();
            CheckDatafile();
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// 
        /// </summary>
        private async void CheckDatafile()
        {
            var dbDatafile = await App.Repository.Datafile.GetAsync();
            if (dbDatafile.IsWebDAV)
            {
                //TODO Webdav implementation
            }
            else
            {
                CheckLocalDatafile(dbDatafile);
            }
        }

        /// <summary>
        /// Checks and reads the current local datafile
        /// </summary>
        /// <param name="dbDatafile"></param>
        private async void CheckLocalDatafile(DBDatafileModel dbDatafile)
        {
            try
            {
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path);
                if (await FileService.FileExistsAsync(dbDatafile.Name, folder))
                {
                    var dbHash = await App.Repository.Password.GetAsync();
                    // prevent write of the datafile
                    _initialization = true;
                    try
                    {
                        string datafileStr = await FileService.ReadStringAsync(dbDatafile.Name, folder);
                        if (!string.IsNullOrEmpty(datafileStr))
                        {
                            // read the iv for AES
                            DatafileModel datafile = _newtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            var iv = datafile.IV;

                            datafile = _newtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                                            SecretService.Helper.ReadSecret(Constants.ContainerName,dbHash.Hash),
                                                            iv,
                                                            datafileStr);
                            deserializeCollection = datafile.Collection;
                        }

                    }
                    catch (Exception)
                    {
                        ShowPasswordError();
                    }
                }
                // file not found case
                else
                {
                    ShowFileOrFolderNotFoundError();
                }

                if (deserializeCollection != null)
                {
                    Collection.AddRange(deserializeCollection);
                    if (Collection.Count == 0)
                    {
                        // if no error has occured
                        if (!_errorOccurred)
                        {
                            EmptyAccountCollectionTipIsOpen = true;
                        }
                    }
                    else
                    {
                        if (EmptyAccountCollectionTipIsOpen)
                        {
                            EmptyAccountCollectionTipIsOpen = false;
                        }
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exc)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (exc is UnauthorizedAccessException)
                {
                    ShowUnauthorizedAccessError();
                }
                else if(exc is FileNotFoundException)
                {
                    ShowFileOrFolderNotFoundError();
                }
                else
                {
                    ShowUnexpectedError(exc);
                }

            }
            // writing the data file is activated again
            _initialization = false;
        }

        /// <summary>
        /// Deletes the datafile from storage
        /// </summary>
        public async void DeleteLocalDatafile()
        {
            var dbDatafile = await App.Repository.Datafile.GetAsync();
            var storageFile = await (await StorageFolder.GetFolderFromPathAsync(dbDatafile.Path)).GetFileAsync(dbDatafile.Name);

            if (storageFile != null)
            {
                await storageFile.DeleteAsync();
            }
        }

        private void ErrorResolved()
        {
            _errorOccurred = false;
            // allow shell navigation
            App.ShellPageInstance.NavigationIsAllowed = true;
        }

        private async void ShowUnexpectedError(Exception exc)
        {
            _errorOccurred = true;
            var dialog = new ContentDialog
            {
                Title = Resources.ErrorHandle
            };
            var errorTextBox = new TextBox() {
                Margin = new Thickness(0, 4, 0, 0)
            };
            errorTextBox.Text = exc.Message + "\n"
                + exc.StackTrace + "\n"
                + exc.InnerException;
            errorTextBox.IsReadOnly = true;
            //errorTextBox.TextWrapping = TextWrapping.WrapWholeWords;
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
            stackpanel.Children.Add(new TextBlock() { Text = Resources.ErrorHandleDescription, TextWrapping = TextWrapping.WrapWholeWords});
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
            dialog.PrimaryButtonCommand = new DelegateCommand( async() =>
            {
                await CoreApplication.RequestRestartAsync("Exception");
            });
            dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });
            var result = await DialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                ShowUnexpectedError(exc);
            }
        }

        private async void ShowUnauthorizedAccessError()
        {
            _errorOccurred = true;
            var dialog = new ContentDialog();
            dialog.Title = Resources.AuthorizationFileSystemContentDialogTitle;
            var markdown = new MarkdownTextBlock();
            markdown.Text = Resources.AuthorizationFileSystemContentDialogDescription;
            dialog.Content = markdown;
            dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                Prism.PrismApplicationBase.Current.Exit();
            });
            dialog.PrimaryButtonText = Resources.AuthorizationFileSystemContentDialogPrimaryBTN;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.SecondaryButtonText = Resources.AuthorizationFileSystemContentDialogSecondaryBTN;
            dialog.SecondaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });
            var result = await DialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                Prism.PrismApplicationBase.Current.Exit();
            }
        }

        /// <summary>
        /// Displays a wrong password error message and option to change the password
        /// </summary>
        private async void ShowPasswordError()
        {
            _errorOccurred = true;
            var dialog = new ContentDialog
            {
                Title = Resources.PasswordInvalidHeader,
                Content = Resources.PasswordInvalidMessage,
                PrimaryButtonText = Resources.ChangePassword,
                PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style,
                PrimaryButtonCommand = new DelegateCommand(async () =>
                {
                    var result = await DialogService.ShowAsync(new ChangeDatafilePasswordContentDialog(true));
                }),
                SecondaryButtonText = Resources.CloseApp,
                SecondaryButtonCommand = new DelegateCommand(() =>
                {
                    Prism.PrismApplicationBase.Current.Exit();
                })
            };

            var result = await DialogService.ShowAsync(dialog);
            if (result == ContentDialogResult.None)
            {
                ShowPasswordError();
            }
        }

        /// <summary>
        /// Displays a FileNotFoundException message and the option for factory reset or correcting the path
        /// </summary>
        private async void ShowFileOrFolderNotFoundError()
        {
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            try
            {
                //TODO current workaround: check permission to the file system (broadFileSystemAccess)
                string path = @"C:\Windows\explorer.exe";
                var file = await StorageFile.GetFileFromPathAsync(path);
            }
            catch (UnauthorizedAccessException)
            {
                ErrorDialogs.UnauthorizedAccessDialog();
            }
            _errorOccurred = true;
            // disable shell navigation
            App.ShellPageInstance.NavigationIsAllowed = false;
            Logger.Log("no datafile found", Category.Exception, Priority.High);
            bool selectedOption = false;

            var dialog = new ContentDialog();
            dialog.Closed += Dialog_Closed;
            dialog.Title = Resources.ErrorHandle;
            var markdown = new MarkdownTextBlock();
            markdown.Text = Resources.ExceptionDatafileNotFound;
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(markdown);

            var changePathBTN = new Button();
            changePathBTN.Margin = new Thickness(0, 10, 0, 0);
            changePathBTN.Content = Resources.ChangeDatafilePath;
            changePathBTN.Command = new DelegateCommand(async() =>
            {
                selectedOption = true;
                dialog.Hide();
                var result = await dialogService.ShowAsync(new UpdateDatafileContentDialog());
                if (result == ContentDialogResult.Primary)
                {
                    ErrorResolved();
                    CheckDatafile();
                }
                if (result == ContentDialogResult.None)
                {
                    ShowFileOrFolderNotFoundError();
                }
                
            });
            stackPanel.Children.Add(changePathBTN);

            var factoryResetBTN = new Button();
            factoryResetBTN.Margin = new Thickness(0, 10, 0, 10);
            factoryResetBTN.Content = Resources.FactoryReset;
            factoryResetBTN.Command = new DelegateCommand(async () =>
            {
                var passwordHash = await App.Repository.Password.GetAsync();
                //delete password in the secret vault
                SecretService.Helper.RemoveSecret(Constants.ContainerName, passwordHash.Hash);
                // reset data and restart app
                await ApplicationData.Current.ClearAsync();
                await CoreApplication.RequestRestartAsync("Factory reset");
            });
            stackPanel.Children.Add(factoryResetBTN);

            dialog.Content = stackPanel;
            dialog.PrimaryButtonText = Resources.CloseApp;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            dialog.PrimaryButtonCommand = new DelegateCommand(() =>
            {
                Prism.PrismApplicationBase.Current.Exit();
            });

            async void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
            {
                if (!(Window.Current.Content is ShellPage))
                {
                    Prism.PrismApplicationBase.Current.Exit();
                }
                else
                {
                    if (!selectedOption)
                    {
                        await dialogService.ShowAsync(dialog);
                    }
                }
            }

            await dialogService.ShowAsync(dialog);
        }

        /// <summary>
        /// Writes the current accounts into the datafile
        /// </summary>
        public async void WriteLocalDatafile()
        {
            var dbHash = await App.Repository.Password.GetAsync();
            var datafileDB = await App.Repository.Datafile.GetAsync();
            var iv = new AesManaged().IV;

            await CollectionAccessSemaphore.WaitAsync();

            DatafileModel file = new DatafileModel() { IV = iv, Collection = Collection };

            var folder = await StorageFolder.GetFolderFromPathAsync(datafileDB.Path);
            await FileService.WriteStringAsync(
                    datafileDB.Name,
                    _newtonsoftJSONService.SerializeEncrypt(SecretService.Helper.ReadSecret(Constants.ContainerName, dbHash.Hash),
                    iv, 
                    file),
                    folder);
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Generates a TOTP code for the i'th entry of a collection
        /// </summary>
        /// <param name="i"></param>
        public void GenerateTOTP(int i)
        {
            try
            {
                var totp = new Totp(Collection[i].SecretByteArray, Collection[i].Period, Collection[i].HashMode, Collection[i].TotpSize);
                Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// If collection is empty, bool is true => open teaching tip to add a new account
        /// </summary>
        public bool EmptyAccountCollectionTipIsOpen
        {
            get => _emptyAccountCollectionTipIsOpen;
            set => SetProperty(ref _emptyAccountCollectionTipIsOpen, value);
        }  

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CollectionAccessSemaphore?.Dispose();
            }
        }
    }
}
