using Prism.Mvvm;
using Prism.Ioc;
using Project2FA.Core.Services.JSON;
using Project2FA.Repository.Models;
using Project2FA.Services.File;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OtpNet;
using Xamarin.Essentials;
using Project2FA.Core.Utils;
using System.Threading;
using System.Security.Cryptography;
using Prism.Services.Dialogs;

namespace Project2FA.Services
{
    public class DataService : BindableBase
    {
        private IFileService _fileService { get; }
        bool _initialization;

        private INewtonsoftJSONService _newtonsoftJSONService { get; }

        private IDialogService _dialogService { get; }

        private readonly SemaphoreSlim _collectionAccessSemaphore = new SemaphoreSlim(1, 1);

        public static DataService Instance { get; } = new DataService();

        public SortableObservableCollection<TwoFACodeModel> Collection { get; private set; } = new SortableObservableCollection<TwoFACodeModel>();

        private bool _emptyAccountCollectionTipIsOpen = false;
        private DataService()
        {
            _dialogService = App.Current.Container.Resolve<IDialogService>();
            _fileService = App.Current.Container.Resolve<IFileService>();
            _newtonsoftJSONService = App.Current.Container.Resolve<INewtonsoftJSONService>();
            Collection.CollectionChanged += Accounts_CollectionChanged;
            CheckDatafile();
        }

        /// <summary>
        /// Create TOTP code for collection initialization and write the datafile, if an item added or removed from the collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (!_initialization)
                    {
                        WriteLocalDatafile();
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            ResetCollection();
                        }
                    }
                    else
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            InitializeItem((sender as ObservableCollection<TwoFACodeModel>).Count - 1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reset the TOTP code
        /// </summary>
        public void ResetCollection()
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                InitializeItem(i);
            }
        }

        private void InitializeItem(int i)
        {
            Collection[i].Seconds = Collection[i].Period;
            GenerateTOTP(i);
        }

        /// <summary>
        /// Reloads the datafile in the local database
        /// </summary>
        public void ReloadDatafile()
        {
            CheckDatafile();
        }

        private async void CheckDatafile()
        {
            var dbDatafile = await App.Repository.Datafile.GetAsync();
            if (dbDatafile.IsWebDAV)
            {
                //TODO Webdav implementation
            }
            else
            {
                CheckLocalDatafile(dbDatafile);
            }
        }

        /// <summary>
        /// Checks and reads the current datafile
        /// </summary>
        /// <param name="dbDatafile"></param>
        private async void CheckLocalDatafile(DBDatafileModel dbDatafile)
        {
            try
            {
                ObservableCollection<TwoFACodeModel> deserializeCollection = new ObservableCollection<TwoFACodeModel>();

                if (_fileService.FileExists(dbDatafile.Name, StorageStrategies.Custom, dbDatafile.Path))
                {
                    var dbHash = await App.Repository.Password.GetAsync();
                    //prevent write of the datafile
                    _initialization = true;
                    try
                    {
                        string datafileStr = _fileService.ReadString(dbDatafile.Name, StorageStrategies.Custom, dbDatafile.Path);
                        if (!string.IsNullOrEmpty(datafileStr))
                        {
                            //read the iv for AES
                            DatafileModel datafile = _newtonsoftJSONService.Deserialize<DatafileModel>(datafileStr);
                            var iv = datafile.IV;

                            datafile = _newtonsoftJSONService.DeserializeDecrypt<DatafileModel>(
                                                            await SecureStorage.GetAsync(dbHash.Hash),
                                                            iv,
                                                            datafileStr);
                            deserializeCollection = datafile.Collection;
                        }

                    }
                    catch (Exception exc)
                    {
                        ShowPasswordError();
                    }
                }
                else
                {
                    throw new Exception();
                    //var dialog = new ContentDialog();
                    //dialog.Title = "Error";
                    //dialog.Content = "#Datei nicht gefunden, Pfad ändern?";
                    //dialog.PrimaryButtonText = "#Do IT!";
                    //dialog.SecondaryButtonText = "#App beenden";
                    //dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                    //dialog.PrimaryButtonCommand = new DelegateCommand(() =>
                    //{
                    //    //TODO Add logic
                    //});
                    //dialog.SecondaryButtonCommand = new DelegateCommand(() =>
                    //{
                    //    Prism.PrismApplicationBase.Current.Quit();
                    //});
                    //var _dialogService = App.Current.Container.Resolve<IDialogService>();
                    //_dialogService.ShowDialogAsync()
                    //await _dialogService.ShowAsync(dialog);
                }

                if (deserializeCollection != null)
                {
                    Collection.AddRange(deserializeCollection);
                    if (Collection.Count == 0)
                    {
                        EmptyAccountCollectionTipIsOpen = true;
                    }
                    else
                    {
                        if (EmptyAccountCollectionTipIsOpen)
                        {
                            EmptyAccountCollectionTipIsOpen = false;
                        }
                    }
                }
                else
                {
                    EmptyAccountCollectionTipIsOpen = true;
                }
            }
            catch (Exception exc)
            {
                //TODO 
            }
            


            _initialization = false;
        }

        private void ShowPasswordError()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the current accounts into the datafile
        /// </summary>
        public async void WriteLocalDatafile()
        {
            var dbHash = await App.Repository.Password.GetAsync();
            var datafile = await App.Repository.Datafile.GetAsync();
            var iv = new AesManaged().IV;

            await CollectionAccessSemaphore.WaitAsync();

            DatafileModel file = new DatafileModel() { IV = iv, Collection = Collection };

            _fileService.WriteStringAsync(
                    datafile.Name,
                    _newtonsoftJSONService.SerializeEncrypt(await SecureStorage.GetAsync(dbHash.Hash),
                    iv, file));
            CollectionAccessSemaphore.Release();
        }

        /// <summary>
        /// Generates a TOTP code for the i'th entry of a collection
        /// </summary>
        /// <param name="i"></param>
        public void GenerateTOTP(int i)
        {
            try
            {
                var totp = new Totp(Collection[i].SecretByteArray, Collection[i].Period, Collection[i].HashMode, Collection[i].TotpSize);
                Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                //_logger.Log(e.Message, Category.Exception, Priority.High);
                // TODO: Check if the Base32 encoding fails and deny creation of account entry
            }
        }

        public SemaphoreSlim CollectionAccessSemaphore => _collectionAccessSemaphore;

        public bool EmptyAccountCollectionTipIsOpen 
        {
            get => _emptyAccountCollectionTipIsOpen;
            set => SetProperty(ref _emptyAccountCollectionTipIsOpen, value);
        }
    }
}
