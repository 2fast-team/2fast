using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Encryption;
using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Repository.Models
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class CategoryModel : ObservableObject, ICloneable
    {
        private string _unicodeString;
        [Encrypt]
        public string UnicodeString 
        { 
            get => _unicodeString; 
            set => SetProperty(ref _unicodeString, value);
        }

        private string _unicodeIndex;

        public string UnicodeIndex 
        { 
            get => _unicodeIndex; 
            set => SetProperty(ref _unicodeIndex, value);
        }

        private string _name;
        [Encrypt]
        public string Name 
        { 
            get => _name; 
            set => SetProperty(ref _name, value);
        }

        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private Guid _guid;
        public Guid Guid 
        { 
            get => _guid; 
            set => _guid = value; 
        }

        public object Clone()
        {
            return new CategoryModel { UnicodeString = UnicodeString, UnicodeIndex = UnicodeIndex, Guid = Guid, Name = Name, IsSelected = IsSelected };
        }
    }
}
