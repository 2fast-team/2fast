using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Ioc;
using System.Windows.Input;
using Template10.Services.Secrets;
using Prism.Commands;
using Project2FA.Core.Services;
using Project2FA.UWP.Services;
using Project2FA.Core;

namespace Project2FA.UWP.ViewModels
{
    /// <summary>
    /// View model for the content dialog to change the password of the current datafile
    /// </summary>
    public class ChangeDatafilePasswordContentDialogViewModel : BindableBase
    {
        private string _currentPassword, _newPassword, _newPasswordRepeat;
        private bool _isPrimaryBTNEnable;
        private bool _showError;
        private bool _invalidPassword;
        private bool _passwordChanged;
        private ISecretService _secretService { get; }
        public ICommand ConfirmErrorCommand { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeDatafilePasswordContentDialogViewModel()
        {
            _secretService = App.Current.Container.Resolve<ISecretService>();
            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
                CurrentPassword = string.Empty;
            });
        }

        /// <summary>
        /// Changing the password in the datafile and local database
        /// </summary>
        public async Task ChangePasswordInFileAndDB()
        {
            var passwordHash = await App.Repository.Password.GetAsync();
            //delete password in the secret vault
            _secretService.Helper.RemoveSecret(Constants.ContainerName, passwordHash.Hash);
            //set new hash
            passwordHash.Hash = CryptoService.CreateStringHash(NewPassword, SettingsService.Instance.UseExtendedHash);
            // update db with new pw hash
            await App.Repository.Password.UpsertAsync(passwordHash);

            //datafile is allready changed when password is invalid
            if (!InvalidPassword)
            {
                // save new pw in the secret vault
                _secretService.Helper.WriteSecret(Constants.ContainerName, passwordHash.Hash, NewPassword);
                DataService.Instance.WriteLocalDatafile();
            }
            PasswordChanged = true;
        }
        #region GetSetter
        public string CurrentPassword 
        { 
            get => _currentPassword;
            set
            {
                if (SetProperty(ref _currentPassword, value))
                {
                    CheckInputs();
                }
            }
        }
        public string NewPassword 
        { 
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                {
                    CheckInputs();
                }
            }
        }
        public string NewPasswordRepeat 
        { 
            get => _newPasswordRepeat;
            set
            {
                if(SetProperty(ref _newPasswordRepeat, value))
                {
                    CheckInputs();
                }
            }
        }

        private void CheckInputs()
        {
            if (InvalidPassword)
            {
                if (NewPassword == NewPasswordRepeat &&
                    (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(NewPasswordRepeat)))
                {
                    IsPrimaryBTNEnable = true;
                }
                else
                {
                    IsPrimaryBTNEnable = false;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentPassword))
                {
                    if (NewPassword == NewPasswordRepeat &&
                        (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(NewPasswordRepeat)))
                    {
                        IsPrimaryBTNEnable = true;
                    }
                    else
                    {
                        IsPrimaryBTNEnable = false;
                    }
                }
                else
                {
                    IsPrimaryBTNEnable = false;
                }
            }

        }

        public bool IsPrimaryBTNEnable
        {
            get => _isPrimaryBTNEnable;
            set => SetProperty(ref _isPrimaryBTNEnable, value);
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }
        public bool InvalidPassword
        { 
            get => _invalidPassword;
            set => SetProperty(ref _invalidPassword, value);
        }
        public bool PasswordChanged 
        { 
            get => _passwordChanged;
            set => _passwordChanged = value;
        }
        #endregion
    }
}
