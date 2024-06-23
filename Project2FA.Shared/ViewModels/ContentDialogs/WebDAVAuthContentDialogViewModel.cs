using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using UNOversal.Services.Dialogs;

namespace Project2FA.ViewModels
{
    public class WebDAVAuthContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private Uri _source;



        public void Initialize(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("url", out var url))
            {
                Source = new Uri(url);
            }
        }

        public Uri Source 
        { 
            get => _source; 
            set => SetProperty(ref _source, value);
        }
    }
}
