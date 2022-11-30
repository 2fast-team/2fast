using Prism.Ioc;
using Template10.Services.Secrets;
using Prism.Commands;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.Core;
using Template10.Services.File;
using Project2FA.Core.Services.JSON;
using Prism.Services.Dialogs;

namespace Project2FA.UWP.ViewModels
{
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
            IDialogService dialogService,
            IFileService fileService) : base(secretService, fileService, newtonsoftJSONService, dialogService)
        {
            SecretService = App.Current.Container.Resolve<ISecretService>();
            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });
            UseDatafileCommand = new DelegateCommand(async() =>
            {
                await SetLocalFile(true).ConfigureAwait(false);
            });
            ChangeDatafile = true;
        }

        public async void UpdateLocalFileDB()
        {
            var hash = CryptoService.CreateStringHash(Password, SettingsService.Instance.UseExtendedHash);
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
