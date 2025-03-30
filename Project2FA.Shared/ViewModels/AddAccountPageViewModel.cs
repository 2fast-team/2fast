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
    public class AddAccountPageViewModel : AddAccountViewModelBase, IInitializeAsync
    {
        public ICommand CancelButtonCommand { get; }
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
            if (parameters.TryGetValue<List<KeyValuePair<string, string>>>("AccountValuePair", out var valuePair))
            {
                Model = new TwoFACodeModel();
                foreach (KeyValuePair<string, string> item in valuePair)
                {
                    switch (item.Key)
                    {
                        case "secret":
                            SecretKey = item.Value;
                            break;
                        case "label":
                            Label = item.Value;
                            await CheckLabelForIcon();
                            OnPropertyChanged(nameof(Label));
                            break;
                        case "issuer":
                            Model.Issuer = item.Value;
                            OnPropertyChanged(nameof(Issuer));
                            break;
                        case "algorithm":
                            string algo = item.Value.ToLower();
                            switch (algo)
                            {
                                case "sha1":
                                    Model.HashMode = OtpHashMode.Sha1;
                                    break;
                                case "sha256":
                                    Model.HashMode = OtpHashMode.Sha256;
                                    break;
                                case "sha512":
                                    Model.HashMode = OtpHashMode.Sha512;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "period":
                            Model.Period = Convert.ToInt32(item.Value);
                            break;
                        case "digits":
                            Model.TotpSize = Convert.ToInt32(item.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
