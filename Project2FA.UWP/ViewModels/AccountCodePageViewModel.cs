﻿using Project2FA.Repository.Models;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Prism.Mvvm;
using Project2FA.UWP.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Project2FA.UWP.Views;
using Prism.Commands;
using Project2FA.UWP.Strings;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Prism.Navigation;
using Prism.Logging;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Microsoft.Toolkit.Mvvm.Input;

namespace Project2FA.UWP.ViewModels
{
    public class AccountCodePageViewModel : BindableBase, IConfirmNavigationAsync
    {
        private DispatcherTimer _dispatcherTOTPTimer;
        private DispatcherTimer _dispatcherTimerDeletedModel;
        private IDialogService DialogService { get; }
        private ILoggerFacade Logger { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand Copy2FACodeToClipboardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UndoDeleteCommand { get; }
        public ICommand ExportAccountCommand { get; }

        private string _title;


        public AccountCodePageViewModel(IDialogService dialogService, ILoggerFacade loggerFacade)
        {
            DialogService = dialogService;
            Logger = loggerFacade;
            Title = "Accounts";

            _dispatcherTOTPTimer = new DispatcherTimer();
            _dispatcherTOTPTimer.Interval = new TimeSpan(0, 0, 0, 1); //every second
            _dispatcherTOTPTimer.Tick -= TOTPTimer;
            _dispatcherTOTPTimer.Tick += TOTPTimer;

            _dispatcherTimerDeletedModel = new DispatcherTimer();
            _dispatcherTimerDeletedModel.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTimerDeletedModel.Tick -= TimerDeletedModel;
            _dispatcherTimerDeletedModel.Tick += TimerDeletedModel;

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            AddAccountCommand = new DelegateCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                AddAccountContentDialog dialog = new AddAccountContentDialog();
                dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
                await DialogService.ShowDialogAsync(dialog, new DialogParameters());
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            RefreshCommand = new AsyncRelayCommand(ReloadDatafileAndUpdateCollection);

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            LogoutCommand = new DelegateCommand(async () =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
                // clear the navigation stack
                await App.ShellPageInstance.NavigationService.NavigateAsync("/" + nameof(BlankPage));
                LoginPage loginPage = new LoginPage(true);
                Window.Current.Content = loginPage;
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            UndoDeleteCommand = new DelegateCommand(() =>
            {
                _dispatcherTimerDeletedModel.Stop();
                TwoFADataService.RestoreDeletedModel();
                RaisePropertyChanged(nameof(IsAccountDeleted));
                RaisePropertyChanged(nameof(IsAccountNotDeleted));
            });

            ExportAccountCommand = new AsyncRelayCommand<TwoFACodeModel>(ExportQRCode);

            EditAccountCommand = new RelayCommand<TwoFACodeModel>(EditAccountFromCollection);
            DeleteAccountCommand = new RelayCommand<TwoFACodeModel>(DeleteAccountFromCollection);
            Copy2FACodeToClipboardCommand = new RelayCommand<TwoFACodeModel>(Copy2FACodeToClipboard);
            if (TwoFADataService.TempDeletedTFAModel != null)
            {
                _dispatcherTimerDeletedModel.Start();
            }
            StartTOTPLogic();
        }

        private async Task StartTOTPLogic()
        {
            if (DataService.Instance.Collection.Count != 0)
            {
                //only reset the time and calc the new totp
                await DataService.Instance.ResetCollection();
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
            if (TwoFADataService.TempDeletedTFAModel.Seconds > 0)
            {
                TwoFADataService.TempDeletedTFAModel.Seconds--;
            }
            else
            {
                _dispatcherTimerDeletedModel.Stop();
                TwoFADataService.TempDeletedTFAModel = null;
                RaisePropertyChanged(nameof(IsAccountDeleted));
                RaisePropertyChanged(nameof(IsAccountNotDeleted));
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
            await TwoFADataService.CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < TwoFADataService.Collection.Count; i++)
            {
                if (TwoFADataService.Collection[i].Seconds == 1)
                {
                    await DataService.Instance.GenerateTOTP(i);
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
        private async void EditAccountFromCollection(TwoFACodeModel model)
        {
            var dialog = new EditAccountContentDialog();
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            var param = new DialogParameters();
            param.Add("model", model);
            await DialogService.ShowDialogAsync(dialog, param);
        }

        /// <summary>
        /// Deletes an account from the collection
        /// </summary>
        /// <param name="parameter"></param>
        private async void DeleteAccountFromCollection(TwoFACodeModel model)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Style = App.Current.Resources["MyContentDialogStyle"] as Style;
            dialog.Title = Resources.DeleteAccountContentDialogTitle;
            var markdown = new MarkdownTextBlock
            {
                Text = Resources.DeleteAccountContentDialogDescription
            };
            dialog.Content = markdown;
            dialog.PrimaryButtonText = Resources.Confirm;
            dialog.SecondaryButtonText = Resources.ButtonTextCancel;
            dialog.SecondaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            ContentDialogResult result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());
            if (result == ContentDialogResult.Primary)
            {
                TwoFADataService.TempDeletedTFAModel = model;
                TwoFADataService.TempDeletedTFAModel.Seconds = 30;
                TwoFADataService.Collection.Remove(model);
                RaisePropertyChanged(nameof(IsAccountDeleted));
                RaisePropertyChanged(nameof(IsAccountNotDeleted));
                _dispatcherTimerDeletedModel.Start();
            }
        }

        /// <summary>
        /// Reloads the datafile from the filesystem and updates the collection
        /// </summary>
        public async Task ReloadDatafileAndUpdateCollection()
        {
            TwoFADataService.Collection.Clear();
            await TwoFADataService.ReloadDatafile();
        }

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            //detach the events
            if (_dispatcherTOTPTimer.IsEnabled)
            {
                _dispatcherTOTPTimer.Stop();
            }
            if (_dispatcherTimerDeletedModel.IsEnabled)
            {
                _dispatcherTimerDeletedModel.Stop();
            }
            return !await DialogService.IsDialogRunning();
        }

        public DataService TwoFADataService => DataService.Instance;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private async Task ExportQRCode(TwoFACodeModel model)
        {
            var param = new DialogParameters();
            param.Add("Model", model);
            var dialog = new DisplayQRCodeContentDialog();
            await DialogService.ShowDialogAsync(dialog, param);
        }

        public bool IsAccountDeleted => TwoFADataService.TempDeletedTFAModel != null;

        public bool IsAccountNotDeleted => TwoFADataService.TempDeletedTFAModel == null;
    }
}
