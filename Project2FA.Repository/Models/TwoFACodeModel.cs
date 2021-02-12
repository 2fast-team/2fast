using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Encryption;
using OtpNet;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Project2FA.Core;

namespace Project2FA.Repository.Models
{
    public class TwoFACodeModel : ObservableValidator
    {
        private string _label;
        [Encrypt]
        [Required(ErrorMessage = "Required")]
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, true);
        }

        private string _issuer;
        [Encrypt]
        [Required(ErrorMessage = "Required")]
        public string Issuer
        {
            get => _issuer;
            set => SetProperty(ref _issuer, value, true);
        }

        private string _imageUrl;
        [Encrypt]
        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value, true);
        }

        //default seconds for renew the 2fa code
        //no need for SetProperty, because no UI binding(refresh)
        public int Period { get; set; } = 30;

        //no need for SetProperty, because no UI binding
        public OtpHashMode HashMode { get; set; } = OtpHashMode.Sha1;

        //no need for SetProperty, because no UI binding
        public int TotpSize { get; set; } = 6;

        private int _seconds;

        [JsonIgnore]
        public int Seconds
        {
            get => _seconds;
            set
            {
                SetProperty(ref _seconds, value);
            }
        }

        private byte[] _secretByteArray;
        //no need for SetProperty, because no UI binding
        //TODO not working with protect byte array & Android
        [Encrypt]
        public byte[] SecretByteArray
        {
            get
            {
                if (_secretByteArray != null)
                {
#if __MOBILE__
                    return _secretByteArray;
#endif
                    return ProtectData.Unprotect(_secretByteArray);
                }
                else
                {
                    return null;
                }

            }
            set
            {
#if __MOBILE__
                _secretByteArray = value;
#endif
                _secretByteArray = ProtectData.Protect(value);
            }
        }

        private string _twoFACode;
        [JsonIgnore]
        public string TwoFACode
        {
            get => _twoFACode;
            set
            {
                Seconds = Period;
                SetProperty(ref _twoFACode, value);
            }
        }

        private bool _userInfoCopyCodeBTN;
        [JsonIgnore]
        public bool UserInfoCopyCodeBTN
        {
            get => _userInfoCopyCodeBTN;
            set => SetProperty(ref _userInfoCopyCodeBTN, value);
        }

        private bool _userInfoCopyCodeCodeRightClick;
        [JsonIgnore]
        public bool UserInfoCopyCodeRightClick
        {
            get => _userInfoCopyCodeCodeRightClick;
            set => SetProperty(ref _userInfoCopyCodeCodeRightClick, value);
        }

        [JsonIgnore]
        public List<(string name, string message)> Errors
        {
            get
            {
                var list = new List<(string name, string message)>();
                foreach (var item in from ValidationResult e in GetErrors(null) select e)
                {
                    list.Add((item.MemberNames.FirstOrDefault(), item.ErrorMessage));
                }
                return list;
            }
        }

        private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Errors)); // Update Errors on every Error change, so I can bind to it.
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public TwoFACodeModel()
        {
            ErrorsChanged += Model_ErrorsChanged;
        }
    }
}
