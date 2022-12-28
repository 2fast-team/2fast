using OtpNet;
using Prism.Logging;
using Prism.Mvvm;
using Project2FA.Core.Services.Parser;
using Project2FA.Repository.Models;
using System;
using System.Web;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Prism.Ioc;
using Project2FA.UWP.Services;
using Windows.UI.Popups;
using Template10.Services.Resources;
using ZXing.QrCode;
using ZXing.Common;
using ZXing;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Project2FA.UWP.Helpers;
using Template10.Services.File;
using Prism.Services.Dialogs;
using Template10.Services.Serialization;
using System.IO;
using Project2FA.Core.ProtoModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Media.Capture.Frames;
using Template10.Utilities;
using Windows.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;
using System.Linq;
using Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for adding an account countent dialog
    /// </summary>
    public class AddAccountContentDialogViewModel : ObservableObject, IDialogInitializeAsync, IDisposable
    {
        public ObservableCollection<TwoFACodeModel> OTPList { get; private set; } = new ObservableCollection<TwoFACodeModel>();
        public ObservableCollection<MediaFrameSourceGroup> CameraSourceGroup { get; private set; } = new ObservableCollection<MediaFrameSourceGroup>();
        private CameraHelper _cameraHelper;
        private MediaPlayer _mediaPlayer;
        private MediaPlayerElement _mediaPlayerElementControl;
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
        private ILoggerFacade Logger { get; }
        private IResourceService ResourceService { get; }
        private ISerializationService SerializationService { get; }
        private IFileService FileService { get; }
        private IconNameCollectionModel _iconNameCollectionModel;
        private string _tempIconLabel;
        private VideoFrame _currentVideoFrame;
        private long _videoFrameCounter;
        private const int _vidioFrameDivider = 20; // every X frame for analyzing

        /// <summary>
        /// Constructor
        /// </summary>
        public AddAccountContentDialogViewModel(
            IResourceService resourceService,
            IFileService fileService,
            ISerializationService serializationService,
            ILoggerFacade loggerFacade)
        {
            ResourceService = resourceService;
            FileService = fileService;
            SerializationService = serializationService;
            Logger = loggerFacade;
            OTPList = new ObservableCollection<TwoFACodeModel>();

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

            //ErrorsChanged += Validation_ErrorsChanged;

            Window.Current.Activated += Current_Activated;
        }

        private async Task LoadIconNameCollection()
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
            CameraSourceGroup.AddRange(await CameraHelper.GetFrameSourceGroupsAsync());
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
                Logger.Log("Focus lost/Deactivated " + DateTime.Now, Category.Info, Priority.Low);
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
                Logger.Log("Focus/Activated " + DateTime.Now, Category.Info, Priority.Low);
                // if the app is focused again, check if a QR-Code is in the clipboard
                if (_qrCodeScan)
                {
                    ReadQRCodeFromClipboard();
                    _qrCodeScan = false;
                }
            }
        }



        /// <summary>
        /// Launch the MS screenclip app
        /// </summary>
        public async Task ScanClipboardQRCode()
        {
            bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-screenclip:edit?delayInSeconds=" + OpeningSeconds));
            if (result)
            {
                
                _launchScreenClip = true;
            }
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
                        }
                    }
                    catch (Exception exc)
                    {
                        TrackingManager.TrackException(exc);
                        // TODO error by processing the image
                    }

                }
            }
        }

        private async Task ReadAuthenticationFromString()
        {
            await App.ShellPageInstance.Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //migrate code import
                if (_qrCodeStr.StartsWith("otpauth-migration://"))
                {
                    await ParseMigrationQRCode();
                    PivotViewSelectionName = "ImportBackupAccounts";
                    CheckInputs();
                }
                // normal otpauth import
                else
                {
                    await ParseQRCode();
                    //move to the input dialog
                    PivotViewSelectionName = "NormalInputAccount";

                    if (!string.IsNullOrEmpty(SecretKey)
                       && !string.IsNullOrEmpty(Model.Issuer))  /*   && !string.IsNullOrEmpty(Model.Label)*/
                    {
                        IsPrimaryBTNEnable = true;
                    }
                    else
                    {
                        MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogQRCodeContentError, Strings.Resources.Error);
                        await dialog.ShowAsync();
                        //move to the selection dialog
                        SelectedPivotIndex = 0;
                    }
                }
            });
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
                var param = HttpUtility.ParseQueryString(new Uri(_qrCodeStr).Query);
                var dataByteArray = Convert.FromBase64String(param["data"]);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(dataByteArray, 0, dataByteArray.Length);
                    memoryStream.Position = 0;
                    Stream myStream = memoryStream;
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
                            //TODO inform that not all accounts were imported
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
            IProject2FAParser parser = App.Current.Container.Resolve<IProject2FAParser>();
            List<KeyValuePair<string, string>> valuePair = parser.ParseQRCodeStr(_qrCodeStr);

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
                LuminanceSource luminance = new ZXing.SoftwareBitmapLuminanceSource(bitmap);
                BinaryBitmap bbmap = new BinaryBitmap(new HybridBinarizer(luminance));
                Result result = qrReader.decode(bbmap);
                return result == null ? string.Empty : result.Text;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Category.Exception, Priority.Medium);
                TrackingManager.TrackException(ex);
                return string.Empty;
            }
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            await LoadIconNameCollection();
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
                        _mediaPlayer = new MediaPlayer
                        {
                            AutoPlay = true,
                            RealTimePlayback = true
                        };
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
            _currentVideoFrame = e.VideoFrame;
            
            // analyse only every _vidioFrameDivider value
            if (_videoFrameCounter % _vidioFrameDivider == 0 && SelectedPivotIndex == 1)
            {
                var luminanceSource = new SoftwareBitmapLuminanceSource(_currentVideoFrame.SoftwareBitmap);
                if (luminanceSource != null)
                {
                    var barcodeReader = new BarcodeReader
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
            _videoFrameCounter++;

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
            set => SetProperty(ref _selectedPivotIndex, value);
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
            set => SetProperty(ref _pivotViewSelectionName, value); }
        public int SelectedCameraSource 
        { 
            get => _selectedCameraSource;
            set
            {
                if(SetProperty(ref _selectedCameraSource, value))
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
