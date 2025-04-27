using Newtonsoft.Json;
using Newtonsoft.Json.Encryption;
using OtpNet;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Project2FA.Core;
using Project2FA.Core.Services.Crypto;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using Windows.UI.WebUI;
using Project2FA.Repository.Models.Enums;
#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif


namespace Project2FA.Repository.Models
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class TwoFACodeModel : ObservableObject, ICloneable
    {
        private string _label;
        [Encrypt]
        [Required(ErrorMessage = "Required")]
        public string Label
        {
            get => _label;
            set
            {
                if(SetProperty(ref _label, value))
                {
                    //RaisePropertyChanged(nameof(Model));
                }
            }
        }

        private string _issuer;
        [Encrypt]
        [Required(ErrorMessage = "Required")]
        public string Issuer
        {
            get => _issuer;
            set => SetProperty(ref _issuer, value);
        }

        private bool _isFavourite;
        public bool IsFavourite 
        { 
            get => _isFavourite;
            set
            {
                if(SetProperty(ref _isFavourite, value))
                {
                    OnPropertyChanged(nameof(IsFavouriteText));
                }
            }
        }

        [JsonIgnore]
        public string IsFavouriteText
        {
            get => _isFavourite ? "1#"+ Label : Label;
        }

        //default seconds for renew the 2fa code
        private int _period = 30;
        public int Period 
        {
            get => _period;
            set => SetProperty(ref _period, value);
        } 

        //no need for SetProperty, because no UI binding
        public OtpHashMode HashMode { get; set; } = OtpHashMode.Sha1;

        [Encrypt]
        public string OTPType { get; set; } = "totp";

        //no need for SetProperty, because no UI binding
        public int TotpSize { get; set; } = 6;

        private double _seconds;

        [JsonIgnore]
        public double Seconds
        {
            get => _seconds;
            set => SetProperty(ref _seconds, value);
        }

        private byte[] _secretByteArray;
        // no need for SetProperty, because no UI binding
        // Android not support ProtectedData
        [Encrypt]
        public byte[] SecretByteArray
        {
            get
            {
#if WINDOWS_UWP
                return _secretByteArray != null ? ProtectData.Unprotect(_secretByteArray) : null;
#else
                return _secretByteArray;
#endif
            }
            set
            {
#if WINDOWS_UWP
                _secretByteArray = ProtectData.Protect(value);
#else
                _secretByteArray = value;
#endif
            }
        }

        private string _twoFACode;
        [JsonIgnore]
        public string TwoFACode
        {
            get => _twoFACode;
            set => SetProperty(ref _twoFACode, value);
        }

        private bool _hideTOTPCode;
        [JsonIgnore]
        public bool HideTOTPCode
        {
            get => _hideTOTPCode;
            set => SetProperty(ref _hideTOTPCode, value);
        }

        private string _accountIconName;
        [Encrypt]
        public string AccountIconName
        {
            get => _accountIconName;
            set
            {
                if(SetProperty(ref _accountIconName, value))
                {
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        private string _notes;
        [Encrypt]
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private bool _isChecked  = true;

        [JsonIgnore]
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private bool _isEnabled = true;

        [JsonIgnore]
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        [JsonIgnore]
        public TwoFACodeModel Model
        {
            get => this;
        }


        public ObservableCollection<CategoryModel> SelectedCategories { get; set; }

        //[JsonIgnore]
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

        //private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        //{
        //    OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        //}

        /// <summary>
        /// default constructor
        /// </summary>
        public TwoFACodeModel()
        {
            //ErrorsChanged += Model_ErrorsChanged;
        }

        public object Clone()
        {
            return new TwoFACodeModel {  Label = Label, Issuer = Issuer, SelectedCategories = SelectedCategories, Notes = Notes, AccountIconName = AccountIconName };
        }
    }
}
