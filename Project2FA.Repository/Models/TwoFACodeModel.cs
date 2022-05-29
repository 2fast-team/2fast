using Newtonsoft.Json;
using Newtonsoft.Json.Encryption;
using OtpNet;
using System.ComponentModel.DataAnnotations;
using Project2FA.Core;
using Prism.Mvvm;

namespace Project2FA.Repository.Models
{
    public class TwoFACodeModel : BindableBase
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

        private string _imageUrl;
        [Encrypt]
        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        private bool _isFavourite;
        public bool IsFavourite 
        { 
            get => _isFavourite;
            set
            {
                if(SetProperty(ref _isFavourite, value))
                {
                    RaisePropertyChanged(nameof(IsFavouriteText));
                }
            }
        }

        [JsonIgnore]
        public string IsFavouriteText
        {
            get => _isFavourite ? "AAA"+ Label : Label;
        }

        //default seconds for renew the 2fa code
        //no need for SetProperty, because no UI binding(refresh)
        public int Period { get; set; } = 30;

        //no need for SetProperty, because no UI binding
        public OtpHashMode HashMode { get; set; } = OtpHashMode.Sha1;

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
        //no need for SetProperty, because no UI binding
        [Encrypt]
        public byte[] SecretByteArray
        {
            get => _secretByteArray != null ? ProtectData.Unprotect(_secretByteArray) : null;
            set => _secretByteArray = ProtectData.Protect(value);
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
                    RaisePropertyChanged(nameof(Model));
                }
            }
        }

        private string _accountSVGIcon;

        [JsonIgnore]
        public string AccountSVGIcon
        {
            get => _accountSVGIcon;
            set => SetProperty(ref _accountSVGIcon, value);
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

        [JsonIgnore]
        public TwoFACodeModel Model
        {
            get => this;
        }

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
    }
}
