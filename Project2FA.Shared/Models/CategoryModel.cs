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
    public class CategoryModel : ObservableObject
    {
        private string _glyph;

        public string Glyph 
        { 
            get => _glyph; 
            set => SetProperty(ref _glyph, value);
        }

        private string _name;
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

    }
}
