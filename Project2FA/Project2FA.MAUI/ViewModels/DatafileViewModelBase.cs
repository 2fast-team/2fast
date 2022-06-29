using Prism.Mvvm;
using Project2FA.Core.Services;
using Project2FA.MAUI.Services;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebDAVClient.Exceptions;

namespace Project2FA.MAUI.ViewModels
{
    public class DatafileViewModelBase : BindableBase
    {
        private string _serverAddress;
        private string _username;
        private int _selectedIndex;
        private string _dateFileName;
        private bool _datafileBTNActive;
        private bool _isLoading;
        private bool _showError;

        private string _localStorageFolderPath;
        private string _password, _passwordRepeat;

        public ICommand PrimaryButtonCommand { get; set; }
        public ICommand ChangePathCommand { get; set; }
        public ICommand ConfirmErrorCommand { get; set; }
        public ICommand CheckServerAddressCommand { get; set; }


        public ICommand LoginCommand { get; set; }


        //private ISecretService SecretService { get; }

        //private IFileService FileService { get; }

        public DatafileViewModelBase()
        {
            //SecretService = secretService;
            //FileService = fileService;
            //ErrorsChanged += Model_ErrorsChanged;
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public virtual Task CheckInputs()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a local DB with the data from the datafile
        /// </summary>
        /// <param name="isWebDAV"></param>
        public async Task<DBDatafileModel> CreateLocalFileDB(bool isWebDAV)
        {
            string hash = CryptoService.CreateStringHash(Password, false);
            DBPasswordHashModel passwordModel = await App.Repository.Password.UpsertAsync(new DBPasswordHashModel { Hash = hash });
            string tempDataFileName;
            if (!DateFileName.Contains(".2fa"))
            {
                DateFileName += ".2fa";
            }
            tempDataFileName = DateFileName;

            var model = new DBDatafileModel
            {
                DBPasswordHashModel = passwordModel,
                IsWebDAV = isWebDAV,
                Path = LocalStoragePath,
                Name = tempDataFileName
            };


            await App.Repository.Datafile.UpsertAsync(model);

            // write the password with the hash(key) in the secret vault
            await SecureStorage.Default.SetAsync(hash, Password);

            return model;
        }

        /// <summary>
        /// Check if a .2fa datafile exists.
        /// </summary>
        /// <returns>true if the datafile exists with the name, else false</returns>
        public bool CheckIfNameExists(string name)
        {
            return File.Exists(LocalStoragePath + name);
        }


        //private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        //{
        //    OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        //}

        #region GetSets
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

        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public string LocalStoragePath
        {
            get => _localStorageFolderPath;
            set => SetProperty(ref _localStorageFolderPath, value);
        }

        [Required]
        public string DateFileName
        {
            get => _dateFileName;
            set
            {
                if (SetProperty(ref _dateFileName, value))
                {
                    CheckInputs();
                }
            }
        }

        public bool DatafileBTNActive
        {
            get => _datafileBTNActive;
            set => SetProperty(ref _datafileBTNActive, value);
        }

        [Required]
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                CheckInputs();
            }
        }

        //private List<(string name, string message)> _errors;
        //public List<(string name, string message)> Errors
        //{
        //    get
        //    {
        //        if (_errors == null)
        //        {
        //            _errors = new List<(string name, string message)>();
        //        }
        //        foreach (var item in from ValidationResult e in GetErrors(null) select e)
        //        {
        //            _errors.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
        //        }
        //        return _errors;
        //    }
        //}

        [Required]
        public string PasswordRepeat
        {
            get => _passwordRepeat;
            set
            {
                SetProperty(ref _passwordRepeat, value);
                CheckInputs();
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowError
        {
            get => _showError;
            set => SetProperty(ref _showError, value);
        }
        #endregion
    }
}
