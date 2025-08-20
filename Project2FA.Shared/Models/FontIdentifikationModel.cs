using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Repository.Models
{
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
