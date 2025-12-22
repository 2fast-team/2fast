using CommunityToolkit.Mvvm.ComponentModel;
using System;
#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Repository.Models
{
#if !WINDOWS_UWP
    [Bindable]
#endif

#if WINDOWS_UWP && NET10_0_OR_GREATER
    [WinRT.GeneratedBindableCustomPropertyAttribute]
#endif
    public partial class DependencyModel : ObservableObject
    {
        private string _name;

        public string Name 
        { 
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _authors;
        public string Authors 
        {
            get => _authors;
            set => SetProperty(ref _authors, value);
        }

        private string _licenseName;
        public string LicenseName
        {
            get => _licenseName;
            set => SetProperty(ref _licenseName, value);
        }

        private string _stringLicenseURL;
        public string StringLicenseURL
        {
            get => _stringLicenseURL;
            set 
            { 
                if(SetProperty(ref _stringLicenseURL, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        LicenseURL = new Uri(value);
                    }
                }
            }
        }

        private Uri _licenseURL;
        public Uri LicenseURL
        {
            get => _licenseURL;
            set => SetProperty(ref _licenseURL, value);
        }

        private string _stringURL;
        public string StringURL
        {
            get => _stringURL;
            set
            {
                if (SetProperty(ref _stringURL, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        URL = new Uri(value);
                    }
                }
            }
        }

        private Uri _url;
        public Uri URL
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private string _categoryUid;
        public string CategoryUid
        {
            get => _categoryUid;
            set
            {
                _categoryUid = value;
                // TODO Resource to category
            }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private string _describtion;
        public string Describtion
        {
            get => _describtion;
            set => SetProperty(ref _describtion, value);
        }
    }
}
