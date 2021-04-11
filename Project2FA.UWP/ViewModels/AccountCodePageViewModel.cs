using Project2FA.Repository.Models;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Prism.Mvvm;
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

namespace Project2FA.UWP.ViewModels
{
    public class AccountCodePageViewModel : BindableBase, IConfirmNavigation
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

            AddAccountCommand = new DelegateCommand(async() =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                var dialog = new AddAccountContentDialog();
                await PageDialogService.ShowAsync(dialog);
            });

            RefreshCommand = new DelegateCommand(() =>
            {
                ReloadDatafileAndUpdateCollection();
            });

            LogoutCommand = new DelegateCommand(async() =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                await App.ShellPageInstance.NavigationService.NavigateAsync("/BlankPage");
                var loginPage = new LoginPage(true);
                Window.Current.Content = loginPage;
            });

            UndoDeleteCommand = new DelegateCommand(() =>
            {
                TwoFADataService.Collection.Add(TempDeletedTFAModel);
                TempDeletedTFAModel = null;
            });

            EditAccountCommand = new RelayCommand(EditAccountFromCollection);
            DeleteAccountCommand = new RelayCommand(DeleteAccountFromCollection);
            Copy2FACodeToClipboardCommand = new RelayCommand(Copy2FACodeToClipboard);

            if (TwoFADataService.Collection.Count != 0)
            {
                //TODO Navigation breaks the static collection with binding for the TeachingTip
                //The workaround is the reload of the collection
                ReloadDatafileAndUpdateCollection();

                //Old: only reset the time and calc the new totp
                //DataService.Instance.ResetCollection();
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
            //prevent the acccess 
            await TwoFADataService.CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < TwoFADataService.Collection.Count; i++)
            {
                if (TwoFADataService.Collection[i].Seconds == 0)
                {
                    TwoFADataService.Collection[i].Seconds = TwoFADataService.Collection[i].Period;
                    DataService.Instance.GenerateTOTP(i);
                }
                else
                {
                    TwoFADataService.Collection[i].Seconds--;
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
                var dialog = new ContentDialog();
                dialog.Title = Resources.DeleteAccountContentDialogTitle;
                var markdown = new MarkdownTextBlock
                {
                    Text = Resources.DeleteAccountContentDialogDescription
                };
                dialog.Content = markdown;
                dialog.PrimaryButtonText = Resources.Confirm;
                dialog.SecondaryButtonText = Resources.ButtonTextCancel;
                dialog.SecondaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                var result = await PageDialogService.ShowAsync(dialog);
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
            DataService.Instance.ReloadDatafile();
        }

        //detach the events
        public bool CanNavigate(INavigationParameters parameters)
        {
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
            return true;
        }

        public DataService TwoFADataService => DataService.Instance;

        public string Title 
        { 
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsModelDeleted
        {
            get => TempDeletedTFAModel != null;
        }

        public bool IsModelNotDeleted
        {
            get => TempDeletedTFAModel == null;
        }

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
