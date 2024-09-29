using Project2FA.Core.Services;
using Project2FA.Core;
using Project2FA.Core.Services.JSON;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Services.Secrets;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using System.Threading.Tasks;
using Project2FA.Services;
using Project2FA.Core.Services.Crypto;
using UNOversal.Ioc;


#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.UNO;
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
    public class UpdateDatafileContentDialogViewModel : UseDatafileContentDialogViewModel
    {
        private ISecretService SecretService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UpdateDatafileContentDialogViewModel(
            ISecretService secretService,
            INewtonsoftJSONService newtonsoftJSONService,
            IFileService fileService) : base(secretService, fileService, newtonsoftJSONService)
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

        public async Task UpdateLocalFileDB()
        {
            var hash = CryptoService.CreateStringHash(Password);
            var passwordRepo = await App.Repository.Password.GetAsync();
            //delete password in the secret vault
            SecretService.Helper.RemoveSecret(Constants.ContainerName, passwordRepo.Hash);
            passwordRepo.Hash = hash;

            //writes the hash in the db and the secret vault
            SecretService.Helper.WriteSecret(Constants.ContainerName, passwordRepo.Hash, Password);
            await App.Repository.Password.UpsertAsync(passwordRepo);

            var datafileDB = await App.Repository.Datafile.GetAsync();
            datafileDB.Path = LocalStorageFolder.Path;
            datafileDB.Name = DateFileName;
            await App.Repository.Datafile.UpsertAsync(datafileDB);
        }
    }
}
