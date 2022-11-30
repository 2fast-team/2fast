using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.MAUI.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.MAUI.Views;

namespace Project2FA.MAUI.ViewModels
{
    public partial class AccountCodePageViewModel : ObservableObject
    {
        private IDispatcherTimer _dispatcherTOTPTimer;
        private IDispatcherTimer _dispatcherTimerDeletedModel;
        public INewtonsoftJSONService NewtonsoftJSONService { get; }
        private bool _codeVisibilityOptionEnabled;

        public AccountCodePageViewModel(INewtonsoftJSONService newtonsoftJSONService)
        {
            NewtonsoftJSONService = newtonsoftJSONService;

            //Title = "Accounts";

            _dispatcherTOTPTimer = Dispatcher.GetForCurrentThread().CreateTimer();
            _dispatcherTOTPTimer.Interval = new TimeSpan(0, 0, 0, 1); //every second
            _dispatcherTOTPTimer.Tick -= TOTPTimer;
            _dispatcherTOTPTimer.Tick += TOTPTimer;

            _dispatcherTimerDeletedModel = Dispatcher.GetForCurrentThread().CreateTimer();
            _dispatcherTimerDeletedModel.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTimerDeletedModel.Tick -= TimerDeletedModel;
            _dispatcherTimerDeletedModel.Tick += TimerDeletedModel;
            CodeVisibilityOptionEnabled = SettingsService.Instance.UseHiddenTOTP;
        }

        public async Task StartTOTPLogic(bool reloadDatafile = false)
        {
            if (reloadDatafile)
            {
                await DataService.Instance.ReloadDatafile();
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


            _dispatcherTOTPTimer.Start(); // the event for the set of seconds and calculating the totp code
        }

        /// <summary>
        /// Creates a timer for every collection entry to show the duration of the generated TOTP code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void TOTPTimer(object sender, object e)
        {
            await TOTPTimerTask();
        }

        private async Task TOTPTimerTask()
        {
            //prevent the acccess for other Threads
            await TwoFADataService.CollectionAccessSemaphore.WaitAsync();
            for (int i = 0; i < TwoFADataService.Collection.Count; i++)
            {
                TwoFADataService.Collection[i].Seconds -= TwoFADataService.TOTPEventStopwatch.Elapsed.TotalSeconds; // elapsed time (seconds) from the last event call
                if (Convert.ToInt32(TwoFADataService.Collection[i].Seconds) <= 0)
                {
                    await DataService.Instance.GenerateTOTP(i);
                }
            }
            TwoFADataService.TOTPEventStopwatch.Restart(); // reset the added time from the stopwatch => time+ / event
            TwoFADataService.CollectionAccessSemaphore.Release();
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
        /// Show or hide the TOTP code
        /// </summary>
        /// <param name="obj"></param>
        [RelayCommand]
        private void HideOrShowTOTPCode(TwoFACodeModel obj)
        {
            obj.HideTOTPCode = !obj.HideTOTPCode;
        }

        /// <summary>
        /// Navigate to the SettingsPage
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task NavigateToSettings()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }

        public DataService TwoFADataService => DataService.Instance;

        public bool IsAccountDeleted => TwoFADataService.TempDeletedTFAModel != null;

        public bool IsAccountNotDeleted => TwoFADataService.TempDeletedTFAModel == null;

        public bool CodeVisibilityOptionEnabled
        {
            get => _codeVisibilityOptionEnabled;
            private set => SetProperty(ref _codeVisibilityOptionEnabled, value);
        }
        public IDispatcherTimer DispatcherTOTPTimer { get => _dispatcherTOTPTimer; }
        public IDispatcherTimer DispatcherTimerDeletedModel { get => _dispatcherTimerDeletedModel;}


        ///// <summary>
        ///// Generates a TOTP code for the i'th entry of a collection
        ///// </summary>
        ///// <param name="i"></param>
        //public async Task GenerateTOTP(int i)
        //{
        //    try
        //    {
        //        if (Collection[i].SecretByteArray != null)
        //        {
        //            Totp totp = new Totp(Collection[i].SecretByteArray, Collection[i].Period, Collection[i].HashMode, Collection[i].TotpSize);
        //            int remainingTime;

        //            Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
        //            remainingTime = totp.RemainingSeconds();

        //            //Logger.Log("TOTP remaining Time: " + remainingTime.ToString(), Category.Debug, Priority.None);
        //            Collection[i].Seconds = remainingTime;
        //        }
        //        else
        //        {
        //            //TODO  add dialog
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;

        //    }
        //}
    }
}
