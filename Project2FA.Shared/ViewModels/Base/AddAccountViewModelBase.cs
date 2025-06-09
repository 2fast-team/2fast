using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using Project2FA.Repository.Models;
using System.Collections.ObjectModel;
using Windows.Media.Capture.Frames;
using System.Windows.Input;
using UNOversal.Logging;
using UNOversal.Services.Serialization;
using Project2FA.Services.Parser;
using Windows.Media;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.IO;
using Project2FA.Services;
using Windows.Graphics.Imaging;
using Windows.UI.Popups;
using Project2FA.Core.ProtoModels;
using OtpNet;
using ZXing;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Windows.Media.Core;
using CommunityToolkit.Mvvm.Input;
using Windows.UI.ViewManagement;
using Project2FA.Core.Utils;
using System.Web;
using System.Linq;
using CommunityToolkit.WinUI.Helpers;
using UNOversal.Ioc;
using UNOversal.Services.Logging;
using Project2FA.Utils;
using Windows.UI.Core;
using Project2FA.Repository.Models.Enums;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowActivatedEventArgs = Windows.UI.Core.WindowActivatedEventArgs;
using WinUIWindow = Windows.UI.Xaml.Window;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Capture;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Microsoft.Graphics.Canvas;
using Windows.UI.Xaml.Media.Imaging;
#else
using Project2FA.UnoApp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using Project2FA.Uno.Views;
#endif

namespace Project2FA.ViewModels
{
    public class AddAccountViewModelBase : ObservableObject, IDisposable
    {
        public ObservableCollection<TwoFACodeModel> ImportCollection { get; } = new ObservableCollection<TwoFACodeModel>();
        public ObservableCollection<MediaFrameSourceGroup> CameraSourceGroup { get; internal set; } = new ObservableCollection<MediaFrameSourceGroup>();
        public ObservableCollection<FontIdentifikationModel> FontIdentifikationCollection { get; } = new ObservableCollection<FontIdentifikationModel>();
        public ObservableCollection<CategoryModel> GlobalTempCategories { get; } = new ObservableCollection<CategoryModel>();
        private Windows.Media.Playback.MediaPlayer _mediaPlayer;
        private MediaPlayerElement _mediaPlayerElementControl;
        private CameraHelper _cameraHelper;
        public AccountEntryEnum EntryEnum { get; set; } = AccountEntryEnum.None;
        private string _qrCodeStr = string.Empty;
        private bool _isButtonEnable;
        private bool _manualInput;
        private bool _isCameraActive;
        private TwoFACodeModel _model;
        private int _selectedPivotIndex, _selectedCameraSource;
        private int _openingSeconds;
        private int _seconds;
        private string _secretKey;
        private bool _isEditBoxVisible;
        private bool _noCameraFound, _noCameraPermission, _cameraSuccessfullyLoaded;
        private string _pivotViewSelectionName = string.Empty;
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
        public ILoggingService LoggingService { get; internal set; }
        private string _tempIconLabel;
        private VideoFrame _currentVideoFrame;
        private long _videoFrameCounter;
        private const int _vidioFrameDivider = 20; // every X frame for analyzing
        private string _lastPivotItemName;
#if WINDOWS_UWP
        private Direct3D11CaptureFramePool? _framePool;
        private CanvasDevice _canvasDevice;
        private GraphicsCaptureSession? _session;
        
#endif

        public AddAccountViewModelBase()
        {
            Model = new TwoFACodeModel();
            ManualInputCommand = new RelayCommand(() =>
            {

#if WINDOWS_UWP
                CleanUpCamera();
#endif

                SelectedPivotIndex = 1;
                ManualInput = true;
            });
#if WINDOWS_UWP
            ScanQRCodeCommand = new AsyncRelayCommand(ScanQRCodeCommandTask);
#endif
            PrimaryButtonCommand = new AsyncRelayCommand(PrimaryButtonCommandTask);


            CameraScanCommand = new AsyncRelayCommand(CameraScanCommandTask);


            EditAccountIconCommand = new RelayCommand(() =>
            {
                IsEditBoxVisible = !IsEditBoxVisible;
            });

            DeleteAccountIconCommand = new RelayCommand(() =>
            {
                Model.AccountIconName = string.Empty;
                AccountIconName = string.Empty;
                OnPropertyChanged(nameof(Model));
            });

            SecondayButtonCommand = new AsyncRelayCommand(SecondayButtonCommandTask);

#if WINDOWS_UWP
            ReloadCameraCommand = new AsyncRelayCommand(InitializeCameraAsync);
#endif

            LoggingService = App.Current.Container.Resolve<ILoggingService>();
        }

#if WINDOWS_UWP
        private async Task ScanQRCodeCommandTask()
        {
            await CleanUpCamera();
            //OTPList.Clear();
            await ScanScreenQRCode();
        }
#endif


        private async Task CameraScanCommandTask()
        {
#if WINDOWS_UWP
            _cameraHelper = new CameraHelper();

            CameraSourceGroup.AddRange(await CameraHelper.GetFrameSourceGroupsAsync());

            if (CameraSourceGroup.Count > 0)
            {
                await InitializeCameraAsync();
            }
            else
            {
                NoCameraFound = true;
            }
#endif
        }

        private async Task PrimaryButtonCommandTask()
        {
#if WINDOWS_UWP
            await CleanUpCamera();
#endif

            Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
            Model.SelectedCategories.AddRange(GlobalTempCategories.Where(x => x.IsSelected == true), true);
            DataService.Instance.Collection.Add(Model);
#if __ANDROID__ || _IOS__
            await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
#endif
        }

        private async Task SecondayButtonCommandTask()
        {
#if WINDOWS_UWP
            await CleanUpCamera();
#endif
        }

#if WINDOWS_UWP
        /// <summary>
        /// Launch the screen capture app
        /// </summary>
        private async Task ScanScreenQRCode()
        {
            try
            {
                // set Window for compact overlay
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                var size = new Size(192, 150);
                ApplicationView.GetForCurrentView().TryResizeView(size);
                _canvasDevice = new CanvasDevice();

                // create picker for graphic capture
                var picker = new GraphicsCapturePicker();
                GraphicsCaptureItem item = await picker.PickSingleItemAsync();

                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                // The item may be null if the user dismissed the
                // control without making a selection or hit Cancel.
                if (item != null)
                {
                    // Stop the previous capture if we had one.
                    StopWindowScreenCapture();

                    _framePool = Direct3D11CaptureFramePool.Create(
                        _canvasDevice, // D3D device
                        DirectXPixelFormat.B8G8R8A8UIntNormalized, // Pixel format
                        2, // Number of frames
                        item.Size); // Size of the buffers

                    _session = _framePool.CreateCaptureSession(item);

                    _framePool.FrameArrived -= FramePool_WindowFrameArrived;
                    _framePool.FrameArrived += FramePool_WindowFrameArrived;


                    _session.StartCapture();

                }
                else
                {
                    MessageDialog dialog = new MessageDialog(Strings.Resources.AddAccountContentDialogNoCaptureContentError, Strings.Resources.Error);
                    await dialog.ShowAsync();
                }
            }
            catch (Exception exc)
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                TrackingManager.TrackExceptionCatched(nameof(ScanScreenQRCode), exc);
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                await ErrorDialogs.ShowUnexpectedError(exc.ToString());
            }
        }

        /// <summary>
        /// Read the frame from the pool with ZXing
        /// </summary>
        /// <param name="sender">Direct3D11CaptureFramePool</param>
        /// <param name="args"></param>
        private async void FramePool_WindowFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            try
            {
                // only one frame is nessasary
                _framePool.FrameArrived -= FramePool_WindowFrameArrived;
                // The FrameArrived event fires for every frame on the thread that
                // created the Direct3D11CaptureFramePool. This means we don't have to
                // do a null-check here, as we know we're the only one  
                // dequeueing frames in our application.  

                // NOTE: Disposing the frame retires it and returns  
                // the buffer to the pool.
                using var frame = _framePool.TryGetNextFrame();

                // Convert our D3D11 surface into a Win2D object.
                CanvasBitmap canvasBitmap = CanvasBitmap.CreateFromDirect3D11Surface(
                    _canvasDevice,
                    frame.Surface);

                var renderer = new CanvasRenderTarget(_canvasDevice,
                                                      canvasBitmap.SizeInPixels.Width,
                                                      canvasBitmap.SizeInPixels.Height, canvasBitmap.Dpi);
                using (var ds = renderer.CreateDrawingSession())
                {
                    ds.DrawImage(canvasBitmap, 0, 0);
                }

                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                await renderer.SaveAsync(randomAccessStream, CanvasBitmapFileFormat.Png);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                StopWindowScreenCapture();

                var luminanceSource = new SoftwareBitmapLuminanceSource(softwareBitmap);
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
                        _qrCodeStr = HttpUtility.UrlDecode(decodedStr.Text);
                        await ReadAuthenticationFromString();
                    }
                    else
                    {
                        await ErrorDialogs.QRReadError();
                    }
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                await ErrorDialogs.QRReadError();
            }
        }

        public void StopWindowScreenCapture()
        {
            _session?.Dispose();
            _framePool?.Dispose();
            _session = null;
            _framePool = null;
        }
#endif

        private async Task ReadAuthenticationFromString()
        {
            SecretKey = string.Empty;
            Issuer = string.Empty;

            switch (EntryEnum)
            {
                case AccountEntryEnum.None:
                    break;
                case AccountEntryEnum.Add:
                    if (_qrCodeStr.StartsWith("otpauth://") || _qrCodeStr.StartsWith("steam"))
                    {
                        if (await ParseQRCode())
                        {
                            // change the string to the named pivot item to trigger the real change in the code behind of the view
                            PivotViewSelectionName = "PI_AccountInput";
                            CheckInputs(); // the primary button enable state

                        }
                        else
                        {
                            await ErrorDialogs.QRReadError();
                            //move to the selection dialog
                            SelectedPivotIndex = 0;
                        }
                    }
                    else
                    {
                        MessageDialog dialog = new MessageDialog(Strings.Resources.AddQRCodeNotSupportedError, Strings.Resources.Error);
                        //move to the selection dialog
                        SelectedPivotIndex = 0;
                    }
                        break;
                case AccountEntryEnum.Import:
                    if (_qrCodeStr.StartsWith("otpauth-migration"))
                    {
                        //migrate code import (Google)
                        if (await ParseMigrationQRCode(_qrCodeStr))
                        {
                            CheckInputs();
                        }
                        else
                        {
                            await ErrorDialogs.QRReadError();
                            //move to the selection dialog
                            SelectedPivotIndex = 0;
                        }
                        // change the string to the named pivot item to trigger the real change in the code behind of the view
                        PivotViewSelectionName = "PI_ImportAccountList";

                    }
                    else
                    {
                        _qrCodeStr = string.Empty;
                        // error dialog only Google Import code supported
                        MessageDialog dialog = new MessageDialog(Strings.Resources.ImportBackupQRCodeNotSupportedError, Strings.Resources.Error);
                        await dialog.ShowAsync();
                    }
                    break;
                default:
                    _qrCodeStr = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Parses the QR code by splitting the different parts
        /// </summary>
        /// <returns>true if TOTP</returns>
        public async Task<bool> ParseQRCode(List<KeyValuePair<string, string>> accountValuePair = null)
        {
            List<KeyValuePair<string, string>> valuePair = accountValuePair ?? Project2FAParser.ParseQRCodeStr(_qrCodeStr);

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

        /// <summary>
        /// Parse the protobuf data to TwoFACodeModel list
        /// </summary>
        /// <returns></returns>
        private Task<bool> ParseMigrationQRCode(string qrCodeStr)
        {
            try
            {
                var otpmm = new OTPMigrationModel();
                var query = new Uri(qrCodeStr).Query.Replace("?data=", string.Empty);
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
                            ImportCollection.Add(new TwoFACodeModel
                            {
                                Label = label,
                                AccountIconName = DataService.Instance.GetIconForLabel(label),
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
                            ImportCollection.Add(new TwoFACodeModel
                            {
                                Label = label,
                                AccountIconName = DataService.Instance.GetIconForLabel(label),
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
        /// Checks if the inputs are correct and enables / disables the submit button
        /// </summary>
        private bool CheckInputs()
        {
            return IsPrimaryBTNEnable = !string.IsNullOrWhiteSpace(SecretKey) && !string.IsNullOrWhiteSpace(Label) && !string.IsNullOrWhiteSpace(Issuer);
        }

        #region FontIconRegion
        /// <summary>
        /// Search the account fonts for the sender text
        /// </summary>
        /// <param name="senderText"></param>
        /// <returns></returns>
        public Task<bool> SearchAccountFonts(string senderText)
        {
            if (string.IsNullOrEmpty(senderText) == false && senderText.Length >= 2 && senderText != Strings.Resources.AccountCodePageSearchNotFound)
            {
                var tempList = DataService.Instance.FontIconCollection.Where(x => x.Name.Contains(senderText, System.StringComparison.OrdinalIgnoreCase)).ToList();
                FontIdentifikationCollection.AddRange(tempList, true);
                try
                {
                    if (FontIdentifikationCollection.Count == 0)
                    {
                        FontIdentifikationCollection.Add(new FontIdentifikationModel { Name = Strings.Resources.AccountCodePageSearchNotFound });
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(true);
                }
                catch (System.Exception)
                {
                    FontIdentifikationCollection.Clear();
                    return Task.FromResult(false);
                }
            }
            else
            {
                FontIdentifikationCollection.Clear();
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Search the label for an icon in font collection
        /// </summary>
        /// <returns></returns>
        public Task CheckLabelForIcon()
        {
            var transformName = Model.Label.ToLower();
            transformName = transformName.Replace(" ", string.Empty);
            transformName = transformName.Replace("-", string.Empty);

            try
            {
                if (DataService.Instance.FontIconCollection.Where(x => x.Name == transformName).Any())
                {
                    Model.AccountIconName = transformName;
                    AccountIconName = transformName;
                }
                else
                {
                    // fallback: check if one IconNameCollectionModel name fits into the label name

                    var list = DataService.Instance.FontIconCollection.Where(x => x.Name.Contains(transformName));
                    if (list.Count() == 1)
                    {
                        Model.AccountIconName = list.FirstOrDefault().Name;
                        AccountIconName = list.FirstOrDefault().Name;
                    }
                }
            }
            catch (Exception exc)
            {
                LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
#if WINDOWS_UWP && !NET9_0_OR_GREATER
                TrackingManager.TrackExceptionCatched(nameof(CheckLabelForIcon), exc);
#endif
            }
            return Task.CompletedTask;
        }
        #endregion



        #region CameraRegion
#if WINDOWS_UWP
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

                        _mediaPlayer.RealTimePlayback = true;
                    }

                    _mediaPlayer.Source = MediaSource.CreateFromMediaFrameSource(frameSource);
                    MediaPlayerElementControl.SetMediaPlayer(_mediaPlayer);
                }
            }
            catch (Exception exc)
            {
                LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
            }
        }

        /// <summary>
        /// Clean up the camera
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Initialize the camera
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Analyse the frame for QR codes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CameraHelper_FrameArrived(object sender, FrameEventArgs e)
        {
            try
            {
                _currentVideoFrame = e.VideoFrame;

                // analyse only every _vidioFrameDivider value
                if (_videoFrameCounter % _vidioFrameDivider == 0)
                {
                    await App.ShellPageInstance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
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
                                    await CleanUpCamera();
                                    _qrCodeStr = HttpUtility.UrlDecode(decodedStr.Text);
                                    await ReadAuthenticationFromString();
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
                        }

                    });
                }
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
            }
            finally
            {
                _videoFrameCounter++;
            }
        }

#endif
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

        public int PeriodIndex
        {
            get => Model.Period switch
            {
                30 => 0,
                60 => 1,
                _ => 0,
            };
            set
            {
                switch (value)
                {
                    case 0:
                        Model.Period = 30;
                        break;
                    case 1:
                        Model.Period = 60;
                        break;
                    default:
                        Model.Period = 30;
                        break;
                }
            }
        }

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
                SetProperty(ref _selectedPivotIndex, value);
            }
        }

        public string LastPivotItemName
        {
            get => _lastPivotItemName;
            set
            {
                if (SetProperty(ref _lastPivotItemName, value))
                {
                    CheckInputs();
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
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // see https://tools.ietf.org/html/rfc4648 for Base32 specification
                        if (Regex.IsMatch(value, "^[A-Z2-7]*={0,6}$"))
                        {
                            Model.SecretByteArray = Base32Encoding.ToBytes(value);
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
                    CheckInputs();
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
        public int SelectedCameraSource
        {
            get => _selectedCameraSource;
            set
            {
                if (SetProperty(ref _selectedCameraSource, value))
                {
#if WINDOWS_UWP
                    InitializeCameraAsync();
#endif
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

        public bool NoCategoriesExists => DataService.Instance.GlobalCategories.Count == 0;

        public bool CategoriesExists => DataService.Instance.GlobalCategories.Count > 0;

        public string PivotViewSelectionName { get => _pivotViewSelectionName; set => SetProperty(ref _pivotViewSelectionName, value); }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
#if WINDOWS_UWP
                _mediaPlayer?.Dispose();
                _cameraHelper?.Dispose();
                _framePool?.Dispose();
                _canvasDevice?.Dispose();
                _session?.Dispose();
#endif
            }
        }
    }
}
