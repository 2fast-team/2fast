using Project2FA.Repository.Models;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Prism.Mvvm;
using Prism.Ioc;
using Template10.Services.Dialog;
using Project2FA.UWP.Services;
using Project2FA.Core.Utils;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Project2FA.UWP.Views;
using Prism.Commands;
using Project2FA.UWP.Strings;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Prism.Navigation;
using Prism.Logging;
using System.Threading.Tasks;

namespace Project2FA.UWP.ViewModels
{
    public class AccountCodePageViewModel : BindableBase, IConfirmNavigationAsync
    {
        private DispatcherTimer _dispatcherTOTPTimer;
        private DispatcherTimer _dispatcherTimerDeletedModel;
        private IDialogService PageDialogService { get; }
        private ILoggerFacade Logger { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand Copy2FACodeToClipboardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UndoDeleteCommand { get; }

        private int _deletedTFAModelSeconds;
        private string _title;
        private TwoFACodeModel _tempDeletedTFAModel;

        public AccountCodePageViewModel(IDialogService dialogService, ILoggerFacade loggerFacade)
        {
            PageDialogService = dialogService;
            Logger = loggerFacade;
            Title = "Accounts";

            _dispatcherTOTPTimer = new DispatcherTimer();
            _dispatcherTOTPTimer.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTOTPTimer.Tick += TOTPTimer;

            _dispatcherTimerDeletedModel = new DispatcherTimer();
            _dispatcherTimerDeletedModel.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTimerDeletedModel.Tick += TimerDeletedModel;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            AddAccountCommand = new DelegateCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                AddAccountContentDialog dialog = new AddAccountContentDialog();
                await PageDialogService.ShowAsync(dialog);
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            RefreshCommand = new DelegateCommand(() =>
            {
                ReloadDatafileAndUpdateCollection();
            });

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            LogoutCommand = new DelegateCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                // clear the navigation stack
                await App.ShellPageInstance.NavigationService.NavigateAsync("/BlankPage");
                LoginPage loginPage = new LoginPage(true);
                Window.Current.Content = loginPage;
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            UndoDeleteCommand = new DelegateCommand(() =>
            {
                TwoFADataService.Collection.Add(TempDeletedTFAModel);
                TempDeletedTFAModel = null;
            });

            EditAccountCommand = new RelayCommand(EditAccountFromCollection);
            DeleteAccountCommand = new RelayCommand(DeleteAccountFromCollection);
            Copy2FACodeToClipboardCommand = new RelayCommand(Copy2FACodeToClipboard);

            if (DataService.Instance.Collection.Count != 0)
            {
                //only reset the time and calc the new totp
                DataService.Instance.ResetCollection();
            }

            _dispatcherTOTPTimer.Start();
        }


        /// <summary>
        /// Timer for delete the temp model after 30 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDeletedModel(object sender, object e)
        {
            if (DeletedTFAModelSeconds > 0)
            {
                DeletedTFAModelSeconds--;
            }
            else
            {
                _dispatcherTimerDeletedModel.Stop();
                TempDeletedTFAModel = null;
            }
        }

        /// <summary>
        /// Creates a timer for every collection entry to show the duration of the generated TOTP code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TOTPTimer(object sender, object e)
        {
            bool success = true;
            //prevent the acccess 
            await TwoFADataService.CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < TwoFADataService.Collection.Count; i++)
            {
                if (TwoFADataService.Collection[i].Seconds == 0)
                {
                    TwoFADataService.Collection[i].Seconds = TwoFADataService.Collection[i].Period;
                    success = await DataService.Instance.GenerateTOTP(i);
                }
                else
                {
                    TwoFADataService.Collection[i].Seconds--;
                }
                if (!success)
                {
                    // TODO error message
                    return;
                }
            }
            TwoFADataService.CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Copies the current TOTP code from a specific entry in the collection to the clipboard
        /// </summary>
        /// <param name="parameter"></param>
        private void Copy2FACodeToClipboard(object parameter)
        {
            if (parameter is TwoFACodeModel model)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(model.TwoFACode);
                Clipboard.SetContent(dataPackage);
            }
        }

        /// <summary>
        /// Starts the dialog to edit an existing account
        /// </summary>
        /// <param name="parameter"></param>
        private void EditAccountFromCollection(object parameter)
        {
            if (parameter is TwoFACodeModel model)
            {
                PageDialogService.ShowAsync(new EditAccountContentDialog(model));
            }
        }

        /// <summary>
        /// Deletes an account from the collection
        /// </summary>
        /// <param name="parameter"></param>
        private async void DeleteAccountFromCollection(object parameter)
        {
            if (parameter is TwoFACodeModel model)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = Resources.DeleteAccountContentDialogTitle;
                var markdown = new MarkdownTextBlock
                {
                    Text = Resources.DeleteAccountContentDialogDescription
                };
                dialog.Content = markdown;
                dialog.PrimaryButtonText = Resources.Confirm;
                dialog.SecondaryButtonText = Resources.ButtonTextCancel;
                dialog.SecondaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                ContentDialogResult result = await PageDialogService.ShowAsync(dialog);
                if (result == ContentDialogResult.Primary)
                {
                    TempDeletedTFAModel = model;
                    TwoFADataService.Collection.Remove(model);
                }
            }
        }

        /// <summary>
        /// Reloads the datafile from the filesystem and updates the collection
        /// </summary>
        public void ReloadDatafileAndUpdateCollection()
        {
            TwoFADataService.Collection.Clear();
            TwoFADataService.ReloadDatafile();
        }

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            //detach the events
            if (_dispatcherTOTPTimer.IsEnabled)
            {
                _dispatcherTOTPTimer.Stop();
            }
            _dispatcherTOTPTimer.Tick -= TOTPTimer;
            if (_dispatcherTimerDeletedModel.IsEnabled)
            {
                _dispatcherTimerDeletedModel.Stop();
            }
            _dispatcherTimerDeletedModel.Tick -= TimerDeletedModel;
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            return !await dialogService.IsDialogRunning();
        }

        public DataService TwoFADataService => DataService.Instance;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsModelDeleted => TempDeletedTFAModel != null;

        public bool IsModelNotDeleted => TempDeletedTFAModel == null;

        public TwoFACodeModel TempDeletedTFAModel 
        {
            get => _tempDeletedTFAModel;
            set
            {
                if(SetProperty(ref _tempDeletedTFAModel, value))
                {
                    RaisePropertyChanged(nameof(IsModelDeleted));
                    RaisePropertyChanged(nameof(IsModelNotDeleted));
                    DeletedTFAModelSeconds = 30;
                    if (_tempDeletedTFAModel != null)
                    {
                        _dispatcherTimerDeletedModel.Start();
                    }
                }
            }
        }

        public int DeletedTFAModelSeconds 
        {
            get => _deletedTFAModelSeconds;
            set => SetProperty(ref _deletedTFAModelSeconds, value);
        }
    }
}
