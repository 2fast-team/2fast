using Project2FA.Services.Parser;
using UNOversal.Common;
using System;
using UNOversal.Logging;
using UNOversal.Navigation;
using UNOversal.Services.Serialization;
using System.Collections.Generic;
using Project2FA.Repository.Models;
using OtpNet;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public partial class AddAccountPageViewModel : AddAccountViewModelBase, IInitializeAsync
    {
#if __ANDROID__ || __IOS__
        public ICommand CancelButtonCommand { get; }
#endif
        public AddAccountPageViewModel(
            ISerializationService serializationService,
            ILoggerFacade loggerFacade,
            IProject2FAParser project2FAParser,
            INavigationService navigationService) : base()
        {
            SerializationService = serializationService;
            Logger = loggerFacade;
            Project2FAParser = project2FAParser;

#if __ANDROID__ || __IOS__
            CancelButtonCommand = new AsyncRelayCommand(async() =>
            {
                await navigationService.NavigateAsync("/" + nameof(AccountCodePage));
            });
#endif

            //ErrorsChanged += Validation_ErrorsChanged;

#if __ANDROID__ || __IOS__
            App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
#endif
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            this.EntryEnum = Repository.Models.Enums.AccountEntryEnum.Add;
            if (parameters.TryGetValue<List<KeyValuePair<string, string>>>("AccountValuePair", out var valuePair))
            {
                Model = new TwoFACodeModel();
                await ParseQRCode(valuePair);
            }
        }
    }
}
