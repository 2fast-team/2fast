using CommunityToolkit.Mvvm.ComponentModel;
using System;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Repository.Models
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class SymbolModel : ObservableObject
    {
        private Symbol _symbol;

        public Symbol Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
