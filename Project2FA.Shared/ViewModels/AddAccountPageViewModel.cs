using Project2FA.Repository.Models;
using Project2FA.Services.Parser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using UNOversal.Logging;
using UNOversal.Navigation;
using UNOversal.Services.File;
using UNOversal.Services.Serialization;
#if !WINDOWS_UWP
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class AddAccountPageViewModel : AddAccountViewModelBase, IInitializeAsync
    {
        public AddAccountPageViewModel() : base()
        { 

        }
        //public AddAccountPageViewModel(
        //    IFileService fileService,
        //    ISerializationService serializationService,
        //    ILoggerFacade loggerFacade,
        //    IProject2FAParser project2FAParser) : base()
        //{
        //    FileService = fileService;
        //    SerializationService = serializationService;
        //    Logger = loggerFacade;
        //    Project2FAParser = project2FAParser;

        //}
        public async Task InitializeAsync(INavigationParameters parameters)
        {
            //OTPList.Clear();
            //await LoadIconNameCollection();
        }
    }
}
