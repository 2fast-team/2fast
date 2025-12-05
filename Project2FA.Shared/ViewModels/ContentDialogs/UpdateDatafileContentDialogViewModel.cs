using Project2FA.Core.Services;
using Project2FA.Core;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Secrets;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using System.Threading.Tasks;
using Project2FA.Services;
using Project2FA.Core.Services.Crypto;
using UNOversal.Ioc;
using UNOversal.Services.Serialization;

#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.UnoApp;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    /// <summary>
    /// View model for the content dialog to use an existing datafile
    /// </summary>
    public partial class UpdateDatafileContentDialogViewModel : UseDatafileContentDialogViewModel
    {
        private ISecretService SecretService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UpdateDatafileContentDialogViewModel(
            ISecretService secretService,
            ISerializationService serializationService,
            ISerializationCryptoService serializationCryptoService,
            IFileService fileService) : base(secretService, fileService, serializationService, serializationCryptoService)
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            ConfirmErrorCommand = new RelayCommand(() =>
            {
                ShowError = false;
            });
            UseDatafileCommand = new RelayCommand(async() =>
            {
                await SetLocalFile(true).ConfigureAwait(false);
            });
            ChangeDatafile = true;
        }

        public void UpdateLocalFileSettings()
        {
            var hash = CryptoService.CreateStringHash(Password);
            //delete password in the secret vault
            SecretService.Helper.RemoveSecret(Constants.ContainerName, SettingsService.Instance.DataFilePasswordHash);
            SettingsService.Instance.DataFilePasswordHash = hash;

            //writes the hash in the db and the secret vault
            SecretService.Helper.WriteSecret(Constants.ContainerName, SettingsService.Instance.DataFilePasswordHash, Password);

            SettingsService.Instance.DataFilePath = LocalStorageFolder.Path;
            SettingsService.Instance.DataFileName = DateFileName;
        }
    }
}
