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
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Project2FA.UWP.ViewModels
{
    public class AccountCodePageViewModel : BindableBase, IConfirmNavigation
    {
        private DispatcherTimer _dispatcherTOTPTimer;
        private DispatcherTimer _dispatcherTimerDeletedModel;
        private IDialogService _dialogService { get; }
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


        public AccountCodePageViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
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
                await _dialogService.ShowAsync(dialog);
            });

            RefreshCommand = new DelegateCommand(() =>
            {
                ReloadDatafileAndUpdateCollection();
            });

            LogoutCommand = new DelegateCommand(() =>
            {
                if (TwoFADataService.EmptyAccountCollectionTipIsOpen)
                {
                    TwoFADataService.EmptyAccountCollectionTipIsOpen = false;
                }
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

            // TODO next version
            //CheckSystemTime().ConfigureAwait(false);

            //            var groupedList = Collection.GroupBy(x => x.Label.First())
            //.Select(x => new GroupedTwoFACodeModel { GroupKey = x.Key, Items = x.ToObservableCollection() });
            //            _groupedCollection.AddRange(groupedList);
        }


        //TODO next version
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task CheckSystemTime()
        {
            //var lastCheckedSystemTime = SettingsService.Instance.LastCheckedSystemTime;
            //if ()
            //{

            //}
            try
            {
                var networkTime = await GetNetworkTimeAsync();
                var difference = DateTime.Now - networkTime;
                if (difference.TotalMinutes > 1)
                {
                    //TODO translate
                    var dialog = new ContentDialog();
                    dialog.Title = Resources.AccountCodePageWrongTimeTitle;
                    dialog.Content = Resources.AccountCodePageWrongTimeContent;
                    dialog.PrimaryButtonText = Resources.AccountCodePageWrongTimeBTN;
                    dialog.PrimaryButtonCommand = new DelegateCommand(async () =>
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:dateandtime"));
                    });
                    //dialog.SecondaryButtonText = Resources.Confirm;
                    await _dialogService.ShowAsync(dialog);
                }
            }
            catch (Exception)
            {
            }

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
                _dialogService.ShowAsync(new EditAccountContentDialog(model));
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
                var result = await _dialogService.ShowAsync(dialog);
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

        #region NTPServer
        private async Task<DateTime> GetNetworkTimeAsync()
        {
            //GWDG time server
            const string ntpServer = "ntps1.gwdg.de";

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                await socket.ConnectAsync(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        // stackoverflow.com/a/3294698/162671
        private uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        #endregion

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
