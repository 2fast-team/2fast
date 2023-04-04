using Project2FA.Repository.Models;
using System;
using System.Windows.Input;
using Project2FA.Strings;
using System.Threading.Tasks;
using Project2FA.Helpers;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Core.Messenger;
using CommunityToolkit.Mvvm.Messaging;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using UNOversal.Logging;
using Project2FA.Services;
using Prism.Ioc;
using Windows.ApplicationModel.DataTransfer;
using Project2FA.Core;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.UI.Controls;
using WinUIWindow = Windows.UI.Xaml.Window;
#else
using CommunityToolkit.WinUI.UI.Controls;
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
using WinUIWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class AccountCodePageViewModel : ObservableRecipient, IConfirmNavigationAsync, IInitialize
    {
        private DispatcherTimer _dispatcherTOTPTimer;
        private DispatcherTimer _dispatcherTimerDeletedModel;
        private IDialogService DialogService { get; }
        private ILoggerFacade Logger { get; }
        private INavigationService NavigationService { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UndoDeleteCommand { get; }
        public ICommand ExportAccountCommand { get; }
        public ICommand SetFavouriteCommand { get; }
        public ICommand HideOrShowTOTPCodeCommand {get;}
        public ICommand CopyCodeToClipboardCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        private bool _datafileUpdated;
        private bool _datafileWebDAVUpToDate;
        private bool _datafileWebDAVUpdated;
        private bool _datafileNoInternetConnection;

        private bool _codeVisibilityOptionEnabled;
        private string _title;


        public AccountCodePageViewModel(IDialogService dialogService, ILoggerFacade loggerFacade, INavigationService navigationService)
        {
            DialogService = dialogService;
            Logger = loggerFacade;
            NavigationService = navigationService;

            _dispatcherTOTPTimer = new DispatcherTimer();
            _dispatcherTOTPTimer.Interval = new TimeSpan(0, 0, 0, 1); //every second

            _dispatcherTimerDeletedModel = new DispatcherTimer();
            _dispatcherTimerDeletedModel.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTimerDeletedModel.Tick -= TimerDeletedModel;
            _dispatcherTimerDeletedModel.Tick += TimerDeletedModel;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            AddAccountCommand = new RelayCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                AddAccountContentDialog dialog = new AddAccountContentDialog();
                var result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());
                if (result == ContentDialogResult.None)
                {
#if WINDOWS_UWP
                    await dialog.ViewModel.CleanUpCamera();
#endif
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            RefreshCommand = new AsyncRelayCommand(ReloadDatafileAndUpdateCollection);

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            LogoutCommand = new RelayCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                // clear the navigation stack
                await App.ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                if (TwoFADataService.ActivatedDatafile != null)
                {
                    FileActivationPage fileActivationPage = new FileActivationPage();
                    WinUIWindow.Current.Content = fileActivationPage;
                }
                else
                {
                    LoginPage loginPage = new LoginPage(true);
                    WinUIWindow.Current.Content = loginPage;
                }

            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            UndoDeleteCommand = new RelayCommand(() =>
            {
                _dispatcherTimerDeletedModel.Stop();
                TwoFADataService.RestoreDeletedModel();
                OnPropertyChanged(nameof(IsAccountDeleted));
                OnPropertyChanged(nameof(IsAccountNotDeleted));
            });

            ExportAccountCommand = new AsyncRelayCommand<TwoFACodeModel>(ExportQRCode);

            EditAccountCommand = new AsyncRelayCommand<TwoFACodeModel>(EditAccountFromCollection);
            HideOrShowTOTPCodeCommand = new RelayCommand<TwoFACodeModel>(HideOrShowTOTPCode);
            DeleteAccountCommand = new AsyncRelayCommand<TwoFACodeModel>(DeleteAccountFromCollection);
            SetFavouriteCommand = new AsyncRelayCommand<TwoFACodeModel>(SetFavouriteForModel);
            if (TwoFADataService.TempDeletedTFAModel != null)
            {
                _dispatcherTimerDeletedModel.Start();
            }

            CopyCodeToClipboardCommand = new AsyncRelayCommand<TwoFACodeModel>(CopyCodeToClipboardCommandTask);
            NavigateToSettingsCommand = new AsyncRelayCommand(NavigateToSettingsCommandTask);


            //register the messenger calls
            Messenger.Register<AccountCodePageViewModel, DatafileWriteStatusChangedMessage>(this, (r, m) => r.DatafileUpdated = m.Value);
            Messenger.Register<AccountCodePageViewModel, WebDAVStatusChangedMessage>(this, (r, m) =>
            {
                switch (m.Value)
                {
                    case Repository.Models.Enums.WebDAVStatus.Success:
                        break;
                    case Repository.Models.Enums.WebDAVStatus.NoInternet:
                        DatafileNoInternetConnection = true;
                        break;
                    case Repository.Models.Enums.WebDAVStatus.Failed:
                        break;
                    case Repository.Models.Enums.WebDAVStatus.ServerMaintanance:
                        break;
                    case Repository.Models.Enums.WebDAVStatus.Updated:
                        DatafileWebDAVUpdated = true;
                        break;
                    case Repository.Models.Enums.WebDAVStatus.UptoDate:
                        DatafileWebDAVUpToDate = true;
                        break;
                    default:
                        break;
                }
            });


        }

        private async Task NavigateToSettingsCommandTask()
        {
            await NavigationService.NavigateAsync(nameof(SettingPage));
        }

        private async Task StartTOTPLogic()
        {
            if (DataService.Instance.ActivatedDatafile != null)
            {
                await DataService.Instance.StartService();
            }
            else
            {
                if (DataService.Instance.Collection.Count != 0)
                {
                    //only reset the time and calc the new totp
                    await DataService.Instance.ResetCollection();
                }
                else
                {
                    await DataService.Instance.StartService();
                }
            }

            _dispatcherTOTPTimer.Tick -= TOTPTimer;
            _dispatcherTOTPTimer.Tick += TOTPTimer;
            _dispatcherTOTPTimer.Start(); // the event for the set of seconds and calculating the totp code
        }


        /// <summary>
        /// Timer for delete the temp model after 30 seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDeletedModel(object sender, object e)
        {
            if (TwoFADataService.TempDeletedTFAModel.Seconds > 0)
            {
                TwoFADataService.TempDeletedTFAModel.Seconds--;
            }
            else
            {
                _dispatcherTimerDeletedModel.Stop();
                TwoFADataService.TempDeletedTFAModel = null;
                OnPropertyChanged(nameof(IsAccountDeleted));
                OnPropertyChanged(nameof(IsAccountNotDeleted));
            }
        }

        /// <summary>
        /// Creates a timer for every collection entry to show the duration of the generated TOTP code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TOTPTimer(object sender, object e)
        {
            await TOTPTimerTask();
        }

        private async Task TOTPTimerTask()
        {
            //prevent the acccess for other Threads
            //await TwoFADataService.CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < TwoFADataService.Collection.Count; i++)
            {
                TwoFADataService.Collection[i].Seconds -= TwoFADataService.TOTPEventStopwatch.Elapsed.TotalSeconds; // elapsed time (seconds) from the last event call
                if (Convert.ToInt32(TwoFADataService.Collection[i].Seconds) <= 0)
                {
                    await DataService.Instance.GenerateTOTP(i);
                }
            }
            TwoFADataService.TOTPEventStopwatch.Restart(); // reset the added time from the stopwatch => time+ / event
            //TwoFADataService.CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Set the favourite status for an account
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private async Task SetFavouriteForModel(object parameter)
        {
            if (parameter is TwoFACodeModel model)
            {
                model.IsFavourite = !model.IsFavourite;
                await TwoFADataService.WriteLocalDatafile();
                if (!string.IsNullOrWhiteSpace(model.AccountIconName))
                {
                    await SVGColorHelper.GetSVGIconWithThemeColor(model, model.AccountIconName);
                }
            }
        }

        /// <summary>
        /// Copy the 2fa code to clipboard and create a user dialog
        /// </summary>
        /// <param name="model"></param>
        private async Task<bool> CopyCodeToClipboardCommandTask(TwoFACodeModel model)
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
                dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
                dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
                {
                    await CopyCodeToClipboardCommandTask(model);
                });
                dialog.SecondaryButtonText = Strings.Resources.ButtonTextCancel;
                await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
                return false;
            }

        }

        /// <summary>
        /// Show or hide the TOTP code
        /// </summary>
        /// <param name="obj"></param>
        private void HideOrShowTOTPCode(TwoFACodeModel obj)
        {
            obj.HideTOTPCode = !obj.HideTOTPCode;
        }

        /// <summary>
        /// Starts the dialog to edit an existing account
        /// </summary>
        /// <param name="parameter"></param>
#if WINDOWS_UWP
        private async Task EditAccountFromCollection(TwoFACodeModel model)
#else
        public async Task EditAccountFromCollection(TwoFACodeModel model)
#endif
        {
#if __IOS__ || __ANDROID__
            var param = new NavigationParameters();
            param.Add("model", model);
            await NavigationService.NavigateAsync(nameof(EditAccountPage), param);
#else
            var dialog = new EditAccountContentDialog();
            dialog.Style = App.Current.Resources[Project2FA.Core.Constants.ContentDialogStyleName] as Style;
            var param = new DialogParameters();
            param.Add("model", model);
            await DialogService.ShowDialogAsync(dialog, param);
#endif
        }

#if __IOS__ || __ANDROID__
        public async Task AddAccountManual()
        {
            await NavigateToAddAccountPage(true);
        }

        public async Task AddAccountWithCamera()
        {
            await NavigateToAddAccountPage(false);
        }

        private async Task NavigateToAddAccountPage(bool isManualInput)
        {
            var param = new NavigationParameters();
            param.Add("isManualInput", isManualInput);
            await NavigationService.NavigateAsync(nameof(AddAccountPage), param);
        }
#endif

        /// <summary>
        /// Deletes an account from the collection
        /// </summary>
        /// <param name="parameter"></param>
#if WINDOWS_UWP
        private async Task DeleteAccountFromCollection(TwoFACodeModel model)
#else
        public async Task DeleteAccountFromCollection(TwoFACodeModel model)
#endif

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
            dialog.SecondaryButtonStyle = App.Current.Resources[Project2FA.Core.Constants.AccentButtonStyleName] as Style;
            ContentDialogResult result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.Primary)
            {
                TwoFADataService.TempDeletedTFAModel = model;
                TwoFADataService.TempDeletedTFAModel.Seconds = 30;
                TwoFADataService.Collection.Remove(model);
                OnPropertyChanged(nameof(IsAccountDeleted));
                OnPropertyChanged(nameof(IsAccountNotDeleted));
                _dispatcherTimerDeletedModel.Start();
            }
        }

        /// <summary>
        /// Reloads the datafile from the filesystem and updates the collection
        /// </summary>
        public async Task ReloadDatafileAndUpdateCollection()
        {
            if (TwoFADataService.CollectionAccessSemaphore.CurrentCount > 0)
            {
                await TwoFADataService.ReloadDatafile();
            }
            else
            {
                // TODO add info for the user, that the task is currently run
            }
        }

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            if (!await DialogService.IsDialogRunning())
            {
                //detach the events
                if (_dispatcherTOTPTimer.IsEnabled)
                {
                    _dispatcherTOTPTimer.Stop();
                    _dispatcherTOTPTimer.Tick -= TOTPTimer;
                }
                if (_dispatcherTimerDeletedModel.IsEnabled)
                {
                    _dispatcherTimerDeletedModel.Stop();
                }
                TwoFADataService.TOTPEventStopwatch.Reset();
            }
            return !await DialogService.IsDialogRunning();
        }

        public DataService TwoFADataService => DataService.Instance;

        /// <summary>
        /// Starts the dialog to share the account information as QR code
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
#if WINDOWS_UWP
        private async Task ExportQRCode(TwoFACodeModel model)
#else
        public async Task ExportQRCode(TwoFACodeModel model)
#endif
        {
            var param = new DialogParameters();
            param.Add("Model", model);
            var dialog = new DisplayQRCodeContentDialog();
            await DialogService.ShowDialogAsync(dialog, param);
        }

        public void Initialize(INavigationParameters parameters)
        {
            //set the view setting from SettingsPage
            CodeVisibilityOptionEnabled = SettingsService.Instance.UseHiddenTOTP;

            //Start the app logic
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StartTOTPLogic();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public bool IsAccountDeleted => TwoFADataService.TempDeletedTFAModel != null;

        public bool IsAccountNotDeleted => TwoFADataService.TempDeletedTFAModel == null;

        public bool CodeVisibilityOptionEnabled 
        { 
            get => _codeVisibilityOptionEnabled;
            private set => SetProperty(ref _codeVisibilityOptionEnabled, value); 
        }
        public bool DatafileUpdated 
        { 
            get => _datafileUpdated;
            set => SetProperty(ref _datafileUpdated, value);
        }
        public bool DatafileWebDAVUpToDate 
        { 
            get => _datafileWebDAVUpToDate; 
            set => SetProperty(ref _datafileWebDAVUpToDate, value);
        }
        public bool DatafileNoInternetConnection 
        {
            get => _datafileNoInternetConnection;
            set => SetProperty(ref _datafileNoInternetConnection, value);
        }
        public bool DatafileWebDAVUpdated 
        { 
            get => _datafileWebDAVUpdated; 
            set => SetProperty(ref _datafileWebDAVUpdated, value); 
        }
    }
}
