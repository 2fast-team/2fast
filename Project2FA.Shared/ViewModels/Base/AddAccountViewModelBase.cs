using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Project2FA.Repository.Models;
using System.Collections.ObjectModel;
using Windows.Media.Capture.Frames;
using System.Windows.Input;
using UNOversal.Logging;
using UNOversal.Services.Serialization;
using Project2FA.Services.Parser;
using UNOversal.Services.File;
using Windows.Media;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using Project2FA.Helpers;
using Project2FA.Services;
using Project2FA.Core.Utils;
using Windows.UI.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using System.Web;
using Windows.UI.Popups;
using Project2FA.Core.ProtoModels;
using OtpNet;
using ZXing.QrCode;
using ZXing;
using ZXing.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Windows.Media.Core;
using CommunityToolkit.Mvvm.Input;
using Windows.Devices.Enumeration;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Project2FA.Core;
using UNOversal.Services.Dialogs;
using Prism.Ioc;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowActivatedEventArgs = Windows.UI.Core.WindowActivatedEventArgs;
using WinUIWindow = Windows.UI.Xaml.Window;
using Microsoft.Toolkit.Uwp.Helpers;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using CommunityToolkit.WinUI.Helpers;
#endif

namespace Project2FA.ViewModels
{
    public class AddAccountViewModelBase : ObservableObject, IDisposable
    {
        // future function: 
        // create screen capture without third party app
        // https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture

        // create PiP mode
        // https://stackoverflow.com/questions/64644610/how-to-set-minimum-window-size-in-compact-overlay-mode-of-uwp-app

        public ObservableCollection<TwoFACodeModel> OTPList { get; internal set; } = new ObservableCollection<TwoFACodeModel>();
        public ObservableCollection<MediaFrameSourceGroup> CameraSourceGroup { get; internal set; } = new ObservableCollection<MediaFrameSourceGroup>();
        private Windows.Media.Playback.MediaPlayer _mediaPlayer;
        private MediaPlayerElement _mediaPlayerElementControl;
        private CameraHelper _cameraHelper;

        private string _qrCodeStr;
        private bool _qrCodeScan, _launchScreenClip, _isButtonEnable;
        private bool _manualInput;
        private bool _isCameraActive;
        private TwoFACodeModel _model;
        private int _selectedPivotIndex, _selectedCameraSource;
        private int _openingSeconds;
        private int _seconds;
        private string _secretKey;
        private bool _isEditBoxVisible;
        private bool _noCameraFound, _noCameraPermission, _cameraSuccessfullyLoaded;
        private string _pivotViewSelectionName;
        private DispatcherTimer _dispatcherTimer;
        public ICommand ManualInputCommand { get; }
        public ICommand ScanQRCodeCommand { get; }
        public ICommand PrimaryButtonCommand { get; }
        public ICommand SecondayButtonCommand { get; }
        public ICommand CameraScanCommand { get; }
        public ICommand DeleteAccountIconCommand { get; }
        public ICommand EditAccountIconCommand { get; }
        public ICommand ReloadCameraCommand { get; }
        public ILoggerFacade Logger { get; internal set; }
        public ISerializationService SerializationService { get; internal set; }
        public IProject2FAParser Project2FAParser { get; internal set; }
        public IFileService FileService { get; internal set; }
        private IconNameCollectionModel _iconNameCollectionModel;
        private string _tempIconLabel;
        private VideoFrame _currentVideoFrame;
        private long _videoFrameCounter;
        private const int _vidioFrameDivider = 20; // every X frame for analyzing

        public AddAccountViewModelBase()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1); //every second   

            Model = new TwoFACodeModel();
            ManualInputCommand = new RelayCommand(() =>
            {
                PivotViewSelectionName = "NormalInputAccount";

                CleanUpCamera();

                SelectedPivotIndex = 1;
                ManualInput = true;
            });
            ScanQRCodeCommand = new AsyncRelayCommand(ScanQRCodeCommandTask);
            PrimaryButtonCommand = new AsyncRelayCommand(PrimaryButtonCommandTask);


            CameraScanCommand = new AsyncRelayCommand(CameraScanCommandTask);


            EditAccountIconCommand = new RelayCommand(() =>
            {
                IsEditBoxVisible = !IsEditBoxVisible;
            });

            DeleteAccountIconCommand = new RelayCommand(() =>
            {
                Model.AccountSVGIcon = null;
                Model.AccountIconName = null;
                AccountIconName = null;
            });

            SecondayButtonCommand = new AsyncRelayCommand(SecondayButtonCommandTask);

            ReloadCameraCommand = new AsyncRelayCommand(InitializeCameraAsync);

#if WINDOWS_UWP
            WinUIWindow.Current.Activated -= Current_Activated;
            WinUIWindow.Current.Activated += Current_Activated;
#endif
        }

        internal async Task LoadIconNameCollection()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/IconNameCollection.json"));
            IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
            using StreamReader r = new StreamReader(randomStream.AsStreamForRead());
            IconNameCollectionModel = SerializationService.Deserialize<IconNameCollectionModel>(await r.ReadToEndAsync());
            //StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            //string name = "IconNameCollection.json";
            //if (await FileService.FileExistsAsync(name, localFolder))
            //{
            //    var result = await FileService.ReadStringAsync(name, localFolder);
            //    IconNameCollectionModel = SerializationService.Deserialize<IconNameCollectionModel>(result);
            //}
            //else
            //{
            //    //TODO should not happen
            //}
        }

        public async Task LoadIconSVG()
        {
            await SVGColorHelper.GetSVGIconWithThemeColor(Model, Model.AccountIconName);
        }

        private async Task ScanQRCodeCommandTask()
        {

            await CleanUpCamera();
            OTPList.Clear();
            OpeningSeconds = SettingsService.Instance.QRCodeScanSeconds;
            _dispatcherTimer.Tick -= OnTimedEvent;
            _dispatcherTimer.Tick += OnTimedEvent;
            Seconds = OpeningSeconds;
            _dispatcherTimer.Start();
            await ScanClipboardQRCode();
        }


        private async Task CameraScanCommandTask()
        {
            _cameraHelper = new CameraHelper();
#if WINDOWS_UWP
            CameraSourceGroup.AddRange(await CameraHelper.GetFrameSourceGroupsAsync());
#endif
            PivotViewSelectionName = "CameraInputAccount";
            if (CameraSourceGroup.Count > 0)
            {
                await InitializeCameraAsync();
            }
            else
            {
                NoCameraFound = true;
            }
        }

        private async Task PrimaryButtonCommandTask()
        {
            await CleanUpCamera();

            if (OTPList.Count > 0)
            {
                for (int i = 0; i < OTPList.Count; i++)
                {
                    if (OTPList[i].IsChecked)
                    {
                        DataService.Instance.Collection.Add(OTPList[i]);
                    }
                }
            }
            else
            {
                DataService.Instance.Collection.Add(Model);
            }
        }

        private async Task SecondayButtonCommandTask()
        {
            await CleanUpCamera();
        }

        /// <summary>
        /// Detects if the app loses focus and gets it again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                Logger.Log("Focus lost/Deactivated", Category.Info, Priority.Low);
                // if the screenclip app is started, the focus of the application is lost
                if (_launchScreenClip)
                {
                    // now set the scan to true
                    _qrCodeScan = true;
                    Logger.Log("QR-code scan is now active", Category.Info, Priority.Low);
                    // user has switch the application to scan the QR-code
                    _launchScreenClip = false;
                }
            }
            else
            {
                Logger.Log("Focus/Activated", Category.Info, Priority.Low);
                // if the app is focused again, check if a QR-Code is in the clipboard
                if (_qrCodeScan)
                {
#if WINDOWS_UWP
                    ReadQRCodeFromClipboard();
#endif
                    _qrCodeScan = false;
                }
            }
        }



        /// <summary>
        /// Launch the MS screenclip app
        /// </summary>
        public async Task ScanClipboardQRCode()
        {
            bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri(
                string.Format("ms-screenclip:edit?source={0}&delayInSeconds={1}",
                    Strings.Resources.ApplicationName,
                    OpeningSeconds)));
            if (result)
            {
                _launchScreenClip = true;
            }
            else
            {
                MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogScreenclipNotFound, Strings.Resources.Error);
                await dialog.ShowAsync();
            }
    //        bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri(
    //string.Format("ms-screenclip:edit?source={0}&delayInSeconds={1}&clippingMode=Window",
    //    Strings.Resources.ApplicationName,
    //    OpeningSeconds)));

            //// The GraphicsCapturePicker follows the same pattern the
            //// file pickers do.
            //var picker = new GraphicsCapturePicker();
            //GraphicsCaptureItem item = await picker.PickSingleItemAsync();

            //// The item may be null if the user dismissed the
            //// control without making a selection or hit Cancel.
            //if (item != null)
            //{
            //    // Stop the previous capture if we had one.

            //    // We'll define this method later in the document.
            //    //StartCaptureInternal(item);
            //}
            //await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            //var size = new Size(192, 150);
            //ApplicationView.GetForCurrentView().TryResizeView(size);
        }

        private void OnTimedEvent(object sender, object e)
        {
            if (Seconds == 0)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer.Tick -= OnTimedEvent;
            }
            else
            {
                --Seconds;
            }
        }

#if WINDOWS_UWP
        /// <summary>
        /// Read the image (QR-code) from the clipboard
        /// </summary>
        public async Task ReadQRCodeFromClipboard()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Bitmap))
            {
                IRandomAccessStreamReference imageReceived = null;
                try
                {
                    imageReceived = await dataPackageView.GetBitmapAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, Category.Exception, Priority.Medium);
                }
                finally
                {
                    try
                    {
                        if (imageReceived != null)
                        {
                            using IRandomAccessStreamWithContentType imageStream = await imageReceived.OpenReadAsync();
                            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(imageStream);
                            SoftwareBitmap softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();

                            string result = ReadQRCodeFromBitmap(softwareBitmap);
                            _qrCodeStr = HttpUtility.UrlDecode(result);
                            if (!string.IsNullOrEmpty(_qrCodeStr))
                            {
                                //clear the clipboard, if the image is read as TOTP
                                Clipboard.Clear();
                                await ReadAuthenticationFromString();
                            }
                            else
                            {
                                MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogQRCodeContentError, Strings.Resources.Error);
                                await dialog.ShowAsync();
                            }
                        }
                        else
                        {
                            //TODO add error: empty Clipboard?
                            //ContentDialog dialog = new ContentDialog();
                            //dialog.Title = Strings.Resources.ErrorHandle;
                            //dialog.Content = Strings.Resources.ErrorClipboardTask;
                            //dialog.PrimaryButtonText = Strings.Resources.ButtonTextRetry;
                            //dialog.PrimaryButtonStyle = App.Current.Resources[Constants.AccentButtonStyleName] as Style;
                            //dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
                            //{
                            //    await ReadQRCodeFromClipboard();
                            //});
                            //dialog.SecondaryButtonText = Strings.Resources.ButtonTextCancel;
                            //await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
                        }
                    }
                    catch (Exception exc)
                    {
#if WINDOWS_UWP
                        TrackingManager.TrackExceptionCatched(nameof(ReadQRCodeFromClipboard), exc);
#endif
                        // TODO error by processing the image
                    }
                }
            }
        }
#endif

        private async Task ReadAuthenticationFromString()
        {
            SecretKey = string.Empty;
            Issuer = string.Empty;
            //migrate code import
            if (_qrCodeStr.StartsWith("otpauth-migration://"))
            {
                if (await ParseMigrationQRCode())
                {
                    PivotViewSelectionName = "ImportBackupAccounts";
                    CheckInputs();
                }
                else
                {
                    await QRReadError();
                    PivotViewSelectionName = "Overview";
                    //move to the selection dialog
                    SelectedPivotIndex = 0;
                }
            }
            // normal otpauth import
            else
            {
                //move to the input dialog
                PivotViewSelectionName = "NormalInputAccount";

                if (await ParseQRCode() && !string.IsNullOrEmpty(SecretKey)
                   && !string.IsNullOrEmpty(Issuer))
                {
                    IsPrimaryBTNEnable = true;
                }
                else
                {
                    await QRReadError();
                    PivotViewSelectionName = "Overview";
                    //move to the selection dialog
                    SelectedPivotIndex = 0;
                }
            }
        }

        private async Task QRReadError()
        {
            MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogQRCodeContentError, Strings.Resources.Error);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Parse the protobuf data to TwoFACodeModel list
        /// </summary>
        /// <returns></returns>
        private Task<bool> ParseMigrationQRCode()
        {
            try
            {
                var otpmm = new OTPMigrationModel();
                var query = new Uri(_qrCodeStr).Query.Replace("?data=", string.Empty);
                var dataByteArray = Convert.FromBase64String(query);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(dataByteArray, 0, dataByteArray.Length);
                    memoryStream.Position = 0;
                    otpmm = ProtoBuf.Serializer.Deserialize<OTPMigrationModel>(memoryStream);
                    for (int i = 0; i < otpmm.otp_parameters.Count; i++)
                    {
                        if (otpmm.otp_parameters[i].Type == OTPMigrationModel.OtpType.OtpTypeTotp)
                        {
                            string label = string.Empty, issuer = string.Empty;
                            if (otpmm.otp_parameters[i].Name.Contains(":"))
                            {
                                string[] issuerArray = otpmm.otp_parameters[i].Name.Split(':');
                                label = issuerArray[0];
                                issuer = issuerArray[1];
                            }
                            else
                            {
                                label = otpmm.otp_parameters[i].Name;
                                issuer = otpmm.otp_parameters[i].Issuer;
                            }
                            int hashMode = 0;
                            switch (otpmm.otp_parameters[i].Algorithm)
                            {
                                case OTPMigrationModel.Algorithm.AlgorithmSha1:
                                    hashMode = 0;
                                    break;
                                case OTPMigrationModel.Algorithm.AlgorithmSha256:
                                    hashMode = 1;
                                    break;
                                case OTPMigrationModel.Algorithm.AlgorithmSha512:
                                    hashMode = 2;
                                    break;
                            }
                            OTPList.Add(new TwoFACodeModel
                            {
                                Label = label,
                                Issuer = issuer,
                                SecretByteArray = otpmm.otp_parameters[i].Secret,
                                HashMode = (OtpHashMode)hashMode
                            });
                        }
                        else
                        {
                            // no TOTP, not supported
                            string label = string.Empty, issuer = string.Empty;
                            if (otpmm.otp_parameters[i].Name.Contains(":"))
                            {
                                string[] issuerArray = otpmm.otp_parameters[i].Name.Split(':');
                                label = issuerArray[0];
                                issuer = issuerArray[1];
                            }
                            else
                            {
                                label = otpmm.otp_parameters[i].Name;
                                issuer = otpmm.otp_parameters[i].Issuer;
                            }
                            OTPList.Add(new TwoFACodeModel
                            {
                                Label = label,
                                Issuer = issuer,
                                IsChecked = false,
                                IsEnabled = false
                            });
                        }
                    }
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }


        }

        /// <summary>
        /// Parses the QR code by splitting the different parts
        /// </summary>
        /// <returns>true if TOTP</returns>
        private async Task<bool> ParseQRCode()
        {
            List<KeyValuePair<string, string>> valuePair = Project2FAParser.ParseQRCodeStr(_qrCodeStr);

            if (valuePair.Count == 0)
            {
                return false;
            }

            Model = new TwoFACodeModel();
            foreach (KeyValuePair<string, string> item in valuePair)
            {
                switch (item.Key)
                {
                    case "secret":
                        SecretKey = item.Value;
                        break;
                    case "label":
                        Label = item.Value;
                        await CheckLabelForIcon();
                        OnPropertyChanged(nameof(Label));
                        break;
                    case "issuer":
                        Model.Issuer = item.Value;
                        OnPropertyChanged(nameof(Issuer));
                        break;
                    case "algorithm":
                        string algo = item.Value.ToLower();
                        switch (algo)
                        {
                            case "sha1":
                                Model.HashMode = OtpHashMode.Sha1;
                                break;
                            case "sha256":
                                Model.HashMode = OtpHashMode.Sha256;
                                break;
                            case "sha512":
                                Model.HashMode = OtpHashMode.Sha512;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "period":
                        Model.Period = Convert.ToInt32(item.Value);
                        break;
                    case "digits":
                        Model.TotpSize = Convert.ToInt32(item.Value);
                        break;
                    default:
                        break;
                }
            }


            return true;
        }

        private async Task CheckLabelForIcon()
        {
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            string path = root + @"\Assets\AccountIcons";
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);

            var transformName = Model.Label.ToLower();
            transformName = transformName.Replace(" ", string.Empty);
            transformName = transformName.Replace("-", string.Empty);
            ;
            if (await FileService.FileExistsAsync(string.Format("{0}.svg", transformName), folder))
            {

                Model.AccountIconName = transformName;
                AccountIconName = transformName;
                await SVGColorHelper.GetSVGIconWithThemeColor(Model, Model.AccountIconName);
            }
            //var file = await StorageFile.GetFileFromPathAsync(string.Format("ms-appx:///Assets/AccountIcons/{0}.svg", Model.Label.ToLower()))
            //string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            //string path = root + @"\Assets\AccountIcons";
            //StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            //var element = (await folder.GetFilesAsync()).Where(x => x.DisplayName.Contains("microsoft")).FirstOrDefault();
            //if (element != null)
            //{
            //    Model.AccountIconName = element.DisplayName;
            //    //Model.AccountSVGIcon = SV
            //}
        }

        /// <summary>
        /// Checks if the inputs are correct and enables / disables the submit button
        /// </summary>
        private void CheckInputs()
        {
            if (OTPList.Count > 0)
            {
                IsPrimaryBTNEnable = true;
            }
            else
            {
                IsPrimaryBTNEnable = !string.IsNullOrEmpty(SecretKey) && !string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Issuer);
            }
        }

        /// <summary>
        /// Read QR code from writeble bitmap  
        /// </summary>
        /// <param name="encodedStr"></param>
        /// <returns>decoded result</returns>
        private string ReadQRCodeFromBitmap(SoftwareBitmap bitmap)
        {
            try
            {
                QRCodeReader qrReader = new QRCodeReader();
                LuminanceSource luminance = new Project2FA.ZXing.SoftwareBitmapLuminanceSource(bitmap);
                BinaryBitmap bbmap = new BinaryBitmap(new HybridBinarizer(luminance));
                Result result = qrReader.decode(bbmap);

                return result == null ? string.Empty : result.Text;

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.Medium);
#if WINDOWS_UWP
                TrackingManager.TrackException(nameof(ReadQRCodeFromBitmap), ex);
#endif
                return string.Empty;
            }
        }




#region CameraRegion

        private void SetMediaPlayerSource()
        {
            try
            {
                MediaFrameSource frameSource = _cameraHelper?.PreviewFrameSource;
                if (frameSource != null)
                {
                    if (_mediaPlayer == null)
                    {
                        _mediaPlayer = new Windows.Media.Playback.MediaPlayer
                        {
                            AutoPlay = true
                        };
#if WINDOWS_UWP
                        _mediaPlayer.RealTimePlayback = true;
#endif
                    }

                    _mediaPlayer.Source = MediaSource.CreateFromMediaFrameSource(frameSource);
                    MediaPlayerElementControl.SetMediaPlayer(_mediaPlayer);
                }
            }
            catch (Exception ex)
            {
                //InvokePreviewFailed(ex.Message);
            }
        }

        public async Task CleanUpCamera()
        {
            if (_cameraHelper != null)
            {
                await _cameraHelper.CleanUpAsync();
                //_mediaPlayer.SystemMediaTransportControls.IsPlayEnabled = false;
                //MediaPlayerElementControl.SetMediaPlayer(null);
                _mediaPlayer = null;
                _cameraHelper = null;
            }
        }
        private async Task InitializeCameraAsync()
        {
            if (CameraSourceGroup[SelectedCameraSource] is MediaFrameSourceGroup selectedGroup)
            {
                _cameraHelper.FrameSourceGroup = selectedGroup;
                var result = await _cameraHelper.InitializeAndStartCaptureAsync();
                switch (result)
                {
                    case CameraHelperResult.Success:
                        SetMediaPlayerSource();
                        // Subscribe to get frames as they arrive
                        _cameraHelper.FrameArrived -= CameraHelper_FrameArrived;
                        _cameraHelper.FrameArrived += CameraHelper_FrameArrived;
                        CameraSuccessfullyLoaded = true;
                        break;
                    default:
                    case CameraHelperResult.CreateFrameReaderFailed:
                    case CameraHelperResult.InitializationFailed_UnknownError:
                    case CameraHelperResult.NoCompatibleFrameFormatAvailable:
                    case CameraHelperResult.StartFrameReaderFailed:
                    case CameraHelperResult.NoFrameSourceGroupAvailable:
                    case CameraHelperResult.NoFrameSourceAvailable:
                        //InvokePreviewFailed(result.ToString());
                        CameraSuccessfullyLoaded = false;
                        MediaPlayerElementControl.SetMediaPlayer(null);
                        break;

                    case CameraHelperResult.CameraAccessDenied:
                        CameraSuccessfullyLoaded = false;
                        NoCameraPermission = true;
                        break;
                }
            }
        }

        private async void CameraHelper_FrameArrived(object sender, FrameEventArgs e)
        {
            try
            {
                _currentVideoFrame = e.VideoFrame;

                // analyse only every _vidioFrameDivider value
                if (_videoFrameCounter % _vidioFrameDivider == 0 && SelectedPivotIndex == 1)
                {
                    var luminanceSource = new Project2FA.ZXing.SoftwareBitmapLuminanceSource(_currentVideoFrame.SoftwareBitmap);
                    if (luminanceSource != null)
                    {
                        var barcodeReader = new Project2FA.ZXing.BarcodeReader
                        {
                            AutoRotate = true,
                            Options = { TryHarder = true }
                        };
                        var decodedStr = barcodeReader.Decode(luminanceSource);
                        if (decodedStr != null)
                        {
                            if (decodedStr.Text.StartsWith("otpauth"))
                            {
                                await CleanUpCamera();
                                _qrCodeStr = decodedStr.Text;
                                await ReadAuthenticationFromString();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore errors
            }
            finally
            {
                _videoFrameCounter++;
            }
        }


        //async Task SetupCameraAutoFocus()
        //{
        //    if (IsFocusSupported)
        //    {
        //        var focusControl = _mediaCapture.VideoDeviceController.FocusControl;

        //        var focusSettings = new FocusSettings();

        //        //if (ScanningOptions.DisableAutofocus)
        //        //{
        //        //    focusSettings.Mode = FocusMode.Manual;
        //        //    focusSettings.Distance = ManualFocusDistance.Nearest;
        //        //    focusControl.Configure(focusSettings);
        //        //    return;
        //        //}

        //        focusSettings.AutoFocusRange = focusControl.SupportedFocusRanges.Contains(AutoFocusRange.FullRange)
        //            ? AutoFocusRange.FullRange
        //            : focusControl.SupportedFocusRanges.FirstOrDefault();

        //        var supportedFocusModes = focusControl.SupportedFocusModes;
        //        if (supportedFocusModes.Contains(FocusMode.Continuous))
        //        {
        //            focusSettings.Mode = FocusMode.Continuous;
        //        }
        //        else if (supportedFocusModes.Contains(FocusMode.Auto))
        //        {
        //            focusSettings.Mode = FocusMode.Auto;
        //        }

        //        if (focusSettings.Mode == FocusMode.Continuous || focusSettings.Mode == FocusMode.Auto)
        //        {
        //            focusSettings.WaitForFocus = false;
        //            focusControl.Configure(focusSettings);
        //            await focusControl.FocusAsync();
        //        }
        //    }
        //}
#endregion


        //private void Validation_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        //{
        //    OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        //}

#region GetSet

        //public List<(string name, string message)> Errors
        //{
        //    get
        //    {
        //        var list = new List<(string name, string message)>();
        //        foreach (var item in from ValidationResult e in GetErrors(null) select e)
        //        {
        //            list.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
        //        }
        //        return list;
        //    }
        //}
        public int HashModeIndex
        {
            get => Model.HashMode switch
            {
                OtpHashMode.Sha1 => 0,
                OtpHashMode.Sha256 => 1,
                OtpHashMode.Sha512 => 2,
                _ => 0,
            };

            set
            {
                switch (value)
                {
                    case 0:
                        Model.HashMode = OtpHashMode.Sha1;
                        break;
                    case 1:
                        Model.HashMode = OtpHashMode.Sha256;
                        break;
                    case 2:
                        Model.HashMode = OtpHashMode.Sha512;
                        break;
                    default:
                        Model.HashMode = OtpHashMode.Sha1;
                        break;
                }
            }
        }

        //public bool IsFocusSupported
        //    => _mediaCapture != null
        //    && _mediaCapture.VideoDeviceController != null
        //    && _mediaCapture.VideoDeviceController.FocusControl != null
        //    && _mediaCapture.VideoDeviceController.FocusControl.Supported;
        public TwoFACodeModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
        public int SelectedPivotIndex
        {
            get => _selectedPivotIndex;
            set
            {
                if (SetProperty(ref _selectedPivotIndex, value))
                {
                    //OnPropertyChanged(nameof(PivotViewSelectionName));
                    //if(value== 0)
                    //{
                    //    PivotViewSelectionName = "NormalInputAccount";
                    //}
                }
            }
        }
        public bool IsPrimaryBTNEnable
        {
            get => _isButtonEnable;
            set => SetProperty(ref _isButtonEnable, value);
        }

        [Required]
        public string SecretKey
        {
            get => _secretKey;
            set
            {
                if (SetProperty(ref _secretKey, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        // see https://tools.ietf.org/html/rfc4648 for Base32 specification
                        if (Regex.IsMatch(value, "^[A-Z2-7]*={0,6}$"))
                        {
                            Model.SecretByteArray = Base32Encoding.ToBytes(value);
                            CheckInputs();
                        }
                        else
                        {
#pragma warning disable CA2011 // Avoid infinite recursion
                            string secret = _secretKey.Replace("-", string.Empty);
                            secret = secret.Replace(" ", string.Empty);
                            SecretKey = secret.ToUpper();
#pragma warning restore CA2011 // Avoid infinite recursion
                        }
                    }
                }
            }
        }

        public string Issuer
        {
            get => Model.Issuer;
            set
            {
                Model.Issuer = value;
                CheckInputs();
            }
        }

        public string Label
        {
            get => Model.Label;
            set
            {
                Model.Label = value;
                TempIconLabel = value;
                CheckInputs();
            }
        }
        public string AccountIconName
        {
            get => Model.AccountIconName;
            set
            {

                Model.AccountIconName = value;
                OnPropertyChanged(nameof(AccountIconName));
                if (value != null)
                {
                    TempIconLabel = string.Empty;
                }
                else
                {
                    TempIconLabel = Label;
                }
            }
        }
        public bool ManualInput
        {
            get => _manualInput;
            set => SetProperty(ref _manualInput, value);
        }
        public int OpeningSeconds
        {
            get => _openingSeconds;
            set => SetProperty(ref _openingSeconds, value);
        }
        public int Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value);
        }
        public bool IsCameraActive
        {
            get => _isCameraActive;
            set => SetProperty(ref _isCameraActive, value);
        }
        public IconNameCollectionModel IconNameCollectionModel
        {
            get => _iconNameCollectionModel;
            private set => _iconNameCollectionModel = value;
        }

        public bool IsEditBoxVisible
        {
            get => _isEditBoxVisible;
            set => SetProperty(ref _isEditBoxVisible, value);
        }

        public string TempIconLabel
        {
            get => _tempIconLabel;
            set => SetProperty(ref _tempIconLabel, value);
        }
        public string PivotViewSelectionName
        {
            get => _pivotViewSelectionName;
            set => SetProperty(ref _pivotViewSelectionName, value);
        }
        public int SelectedCameraSource
        {
            get => _selectedCameraSource;
            set
            {
                if (SetProperty(ref _selectedCameraSource, value))
                {
                    InitializeCameraAsync();
                }
            }
        }

        public MediaPlayerElement MediaPlayerElementControl
        {
            get => _mediaPlayerElementControl;
            set => SetProperty(ref _mediaPlayerElementControl, value);
        }

        public bool NoCameraPermission
        {
            get => _noCameraPermission;
            set => SetProperty(ref _noCameraPermission, value);
        }
        public bool NoCameraFound
        {
            get => _noCameraFound;
            set => SetProperty(ref _noCameraFound, value);
        }
        public bool CameraSuccessfullyLoaded
        {
            get => _cameraSuccessfullyLoaded;
            set => SetProperty(ref _cameraSuccessfullyLoaded, value);
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

                _mediaPlayer?.Dispose();
                _cameraHelper?.Dispose();
            }
        }
#endregion
    }
}
