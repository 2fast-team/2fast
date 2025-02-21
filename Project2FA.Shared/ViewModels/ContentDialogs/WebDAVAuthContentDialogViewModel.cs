using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using UNOversal.Services.Dialogs;

namespace Project2FA.ViewModels
{
    public class WebDAVAuthContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private Uri _url;



        public void Initialize(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("url", out var url))
            {
                URL = new Uri(url);
            }
        }

        public Uri URL 
        { 
            get => _url; 
            set => SetProperty(ref _url, value);
        }
    }
}
