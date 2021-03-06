﻿using Prism.Commands;
using System;
using Windows.Storage;
using Prism.Ioc;
using Template10.Services.File;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Project2FA.UWP.Strings;
using System.Security.Cryptography;
using Project2FA.Repository.Models;
using Template10.Services.Serialization;

namespace Project2FA.UWP.ViewModels
{
    public class NewDatafileContentDialogViewModel : DatafileViewModelBase
    {
        private IFileService _fileService { get; }
        private ISerializationService _serializationService { get; }

        private string _errorText;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewDatafileContentDialogViewModel()
        {
            _serializationService = App.Current.Container.Resolve<ISerializationService>();
            _fileService = App.Current.Container.Resolve<IFileService>();
            PrimaryButtonCommand = new DelegateCommand(async () =>
            {
                //local filedata
                if (SelectedIndex == 1)
                {
                    Password = Password.Replace(" ", string.Empty);
                    await CreateLocalFileDB(false);
                    var iv = new AesManaged().IV;
                    DatafileModel file = new DatafileModel() { IV = iv, Collection = new System.Collections.ObjectModel.ObservableCollection<TwoFACodeModel>() };
                    await _fileService.WriteStringAsync(DateFileName, _serializationService.Serialize(file), await StorageFolder.GetFolderFromPathAsync(LocalStorageFolder.Path));
                }
            });
            CheckServerAddressCommand = new DelegateCommand(() =>
            {
                CheckServerStatus();
            });
            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });
            LoginCommand = new DelegateCommand(CheckLoginAsync);

            ChangePathCommand = new DelegateCommand( async() =>
            {
                await SetLocalPath(true); //change path is true
            });

            UseWebDAVCommand = new DelegateCommand(() =>
            {

            });
        }

        /// <summary>
        /// Checks the inputs and enables / disables the submit button
        /// </summary>
        public override async void CheckInputs()
        {
            if (!string.IsNullOrEmpty(DateFileName))
            {
                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(PasswordRepeat))
                {
                    if (!await CheckIfNameExists(DateFileName + ".2fa"))
                    {
                        if (Password == PasswordRepeat)
                        {
                            IsPrimaryBTNEnable = true;
                        }
                        else
                        {
                            IsPrimaryBTNEnable = false;
                        }

                        //ShowError = true;
                        //ErrorText = "#Die Passwörter stimmen nicht überein";

                    }
                    else
                    {
                        //TODO translate
                        ShowError = true;
                        ErrorText = Resources.NewDatafileContentDialogDatafileExistsError;
                    }
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

        /// <summary>
        /// Starts a folder picker to pick a datafile from a local path
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> SetLocalPath(bool changePath = false)
        {
            SelectedIndex = 1;
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            IsLoading = true;
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                IsLoading = false;
                //set folder to the access list
                StorageApplicationPermissions.FutureAccessList.Add(folder, "metadata");
                LocalStorageFolder = folder;
                return true;
            }
            else
            {
                //prevents the change of the index, if the user want to change
                //the path, but cancel the dialog 
                IsLoading = false;
                if (!changePath)
                {
                    SelectedIndex = 0;
                }
                return false;
            }
        }

        public string ErrorText 
        { 
            get => _errorText; 
            set
            {
                SetProperty(ref _errorText, value);
            }
        }
    }
}
