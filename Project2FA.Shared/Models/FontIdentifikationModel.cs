using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Text;

#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Repository.Models
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public partial class FontIdentifikationModel : ObservableObject
    {
        private uint _unicodeIndex;
        private string _unicodeString;
        private string name;

        public string Name 
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        public string UnicodeString 
        { 
            get => _unicodeString;
            set => SetProperty(ref _unicodeString, value);
        }
        public uint UnicodeIndex 
        { 
            get => _unicodeIndex; 
            set => SetProperty(ref _unicodeIndex, value);
        }
    }
}
