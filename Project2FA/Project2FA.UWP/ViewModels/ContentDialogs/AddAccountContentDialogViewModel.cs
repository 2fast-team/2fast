using OtpNet;
using Prism.Commands;
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
using Microsoft.Toolkit.Mvvm.Input;
using System.IO;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for adding an account countent dialog
    /// </summary>
    public class AddAccountContentDialogViewModel : BindableBase, IDialogInitializeAsync
    {
        private string _qrCodeStr;
        private bool _qrCodeScan, _launchScreenClip, _isButtonEnable;
        private bool _manualInput;
        private bool _isCameraActive;
        private bool _accountIconSavePossible;
        private TwoFACodeModel _model;
        private int _selectedPivotIndex;
        private int _openingSeconds;
        private int _seconds;
        private string _secretKey;
        private string _tempAccountIconName;
        DispatcherTimer _dispatcherTimer;
        public ICommand ManualInputCommand { get; }
        public ICommand ScanQRCodeCommand { get; }
        public ICommand PrimaryButtonCommand { get; }
        public ICommand CameraScanCommand { get; }
        public ICommand SaveAccountIconCommand { get; }
        public ICommand DeleteAccountIconCommand { get; }
        public ICommand CancelAccountIconCommand { get; }
        private ILoggerFacade Logger { get; }
        private IResourceService ResourceService { get; }
        private ISerializationService SerializationService { get; }
        private IFileService FileService { get; }
        private IconNameCollectionModel _iconNameCollectionModel;

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

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1); //every second        

            Model = new TwoFACodeModel();
            ManualInputCommand = new DelegateCommand(() =>
            {
                SelectedPivotIndex = 1;
                ManualInput = true;
            });
            ScanQRCodeCommand = new DelegateCommand( async() =>
            {
                OpeningSeconds = SettingsService.Instance.QRCodeScanSeconds;
                _dispatcherTimer.Tick -= OnTimedEvent;
                _dispatcherTimer.Tick += OnTimedEvent;
                Seconds = OpeningSeconds;
                _dispatcherTimer.Start();
                await ScanQRCode();
            });
            PrimaryButtonCommand = new DelegateCommand(() =>
            {
                DataService.Instance.Collection.Add(Model);
            });

            CameraScanCommand = new DelegateCommand(() =>
            {
                IsCameraActive = true;
            });

            SaveAccountIconCommand = new AsyncRelayCommand(LoadIconSVG);

            
            //ErrorsChanged += Validation_ErrorsChanged;

            Window.Current.Activated += Current_Activated;
        }

        private async Task LoadIconNameCollection()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/IconNameCollection.json"));
            IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
            using (StreamReader r = new StreamReader(randomStream.AsStreamForRead()))
            {
                IconNameCollectionModel = SerializationService.Deserialize<IconNameCollectionModel>(await r.ReadToEndAsync());
            }
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
            Model.AccountIconName = TempAccountIconName;
            string iconStr = await SVGColorHelper.ManipulateSVGColor(Model, Model.AccountIconName);
            if (!string.IsNullOrWhiteSpace(iconStr))
            {
                Model.AccountSVGIcon = iconStr;
            }
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
        public async Task ScanQRCode()
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
                                await ParseQRCode();
                                //move to the input dialog
                                SelectedPivotIndex = 1;

                                if (!string.IsNullOrEmpty(SecretKey)
                                   && !string.IsNullOrEmpty(Model.Issuer))  /*   && !string.IsNullOrEmpty(Model.Label)*/
                                {
                                    IsPrimaryBTNEnable = true;
                                }
                                else
                                {
                                    MessageDialog dialog = new MessageDialog(ResourceService.GetLocalizedString("AddAccountContentDialogQRCodeContentError"), Strings.Resources.Error);
                                    await dialog.ShowAsync();
                                    //move to the selection dialog
                                    SelectedPivotIndex = 0;
                                }
                            }
                            else
                            {
                                MessageDialog dialog = new MessageDialog(ResourceService.GetLocalizedString("AddAccountContentDialogQRCodeContentError"), Strings.Resources.Error);
                                await dialog.ShowAsync();
                            }
                        }
                        else
                        {
                            //TODO add error: empty Clipboard?
                        }
                    }
                    catch (Exception)
                    {
                        // TODO error by processing the image
                        throw;
                    }

                }
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
                        SecretKey= item.Value;
                        break;
                    case "label":
                        Label = item.Value;
                        await CheckLabelForIcon();
                        RaisePropertyChanged(nameof(Label));
                        break;
                    case "issuer":
                        Model.Issuer = item.Value;
                        RaisePropertyChanged(nameof(Issuer));
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

            //var file = await StorageFile.GetFileFromPathAsync(string.Format("ms-appx:///Assets/AccountIcons/{0}.svg", Model.Label.ToLower()));
            if (await FileService.FileExistsAsync(string.Format("{0}.svg", Label.ToLower()), folder))
            {
                Model.AccountIconName = Model.Label.ToLower();
                Model.AccountSVGIcon = await SVGColorHelper.ManipulateSVGColor(Model, Model.AccountIconName);
            }

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
            IsPrimaryBTNEnable = !string.IsNullOrEmpty(SecretKey) && !string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Issuer);
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
                            string secret = SecretKey.Replace("-", string.Empty);
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
                CheckInputs();
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
        public string TempAccountIconName { get => _tempAccountIconName; set => _tempAccountIconName = value; }
        public bool AccountIconSavePossible 
        { 
            get => _accountIconSavePossible; 
            set => SetProperty(ref _accountIconSavePossible, value);
        }

        #endregion
    }
}
