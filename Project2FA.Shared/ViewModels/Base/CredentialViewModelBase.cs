﻿using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Strings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;
using Project2FA.Services;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class CredentialViewModelBase : ObservableRecipient
    {
        private bool _windowsHelloIsUsable, _isLogout;
        private string _password;
        public ICommand LoginCommand { get; internal set; }
        public ICommand WindowsHelloLoginCommand { get; internal set; }
        public ICommand BiometricoLoginCommand { get; internal set; }
        public IDialogService DialogService { get; internal set; }
        private string _applicationTitle;
        private bool _isScreenCaptureEnabled;

        /// <summary>
        /// Shows a wrong password error to the user
        /// </summary>
        public async Task ShowLoginError()
        {
            var dialog = new ContentDialog();
            dialog.Title = Resources.Error;
            dialog.Content = Resources.LoginPagePasswordMismatch;
            dialog.PrimaryButtonText = Resources.Confirm;
            await DialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public bool IsMDMLoginScreenWallpaperAvailable
        {
            get
            {
                if (SettingsService.Instance.IsProVersion)
                {
                    if (!string.IsNullOrWhiteSpace(SettingsService.Instance.LoginScreenWallpaper))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsMDMLoginScreenWallpaperNotAvailable
        {
            get
            {
                if (SettingsService.Instance.IsProVersion)
                {
                    if (string.IsNullOrWhiteSpace(SettingsService.Instance.LoginScreenWallpaper))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public string MDMLoginScreenWallpaperStr
        {
            get
            {
                if (SettingsService.Instance.IsProVersion)
                {
                    if (!string.IsNullOrWhiteSpace(SettingsService.Instance.LoginScreenWallpaper))
                    {
                        return SettingsService.Instance.LoginScreenWallpaper;
                    }
                }
                return string.Empty;
            }
        }

#if WINDOWS_UWP
        public bool WindowsHelloIsUsable
#else
        public bool BiometricIsUsable
#endif
        {
            get => _windowsHelloIsUsable;
            set => SetProperty(ref _windowsHelloIsUsable, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public bool IsLogout
        {
            get => _isLogout;
            set => SetProperty(ref _isLogout, value);
        }
        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }

        internal bool IsScreenCaptureEnabled
        {
            get => _isScreenCaptureEnabled;
            set => SetProperty(ref _isScreenCaptureEnabled, value);
        }
    }
}
