using Prism.Ioc;
using Template10.Services.Secrets;
using Prism.Commands;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for the content dialog to use an existing datafile
    /// </summary>
    public class UpdateDatafileContentDialogViewModel : UseDatafileContentDialogViewModel
    {
        private ISecretService _secretService { get; }

        /// <summary>
        /// Constructor to start the datafile selector
        /// </summary>
        public UpdateDatafileContentDialogViewModel(): base()
        {
            _secretService = App.Current.Container.Resolve<ISecretService>();
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
            _secretService.Helper.RemoveSecret(Constants.ContainerName, passwordRepo.Hash);
            passwordRepo.Hash = hash;

            //writes the hash in the db and the secret vault
            _secretService.Helper.WriteSecret(Constants.ContainerName, passwordRepo.Hash, Password);
            await App.Repository.Password.UpsertAsync(passwordRepo);

            var datafileDB = await App.Repository.Datafile.GetAsync();
            datafileDB.Path = LocalStorageFolder.Path;
            datafileDB.Name = DateFileName;
            await App.Repository.Datafile.UpsertAsync(datafileDB);
        }
    }
}
