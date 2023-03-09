using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Serialization;
using UNOversal.Logging;
using UNOversal.Services.File;
using Project2FA.Services.Parser;

#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif
    /// <summary>
    /// View model for adding an account countent dialog
    /// </summary>
    public class AddAccountContentDialogViewModel : AddAccountViewModelBase, IDialogInitializeAsync
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public AddAccountContentDialogViewModel(
            IFileService fileService,
            ISerializationService serializationService,
            ILoggerFacade loggerFacade,
            IProject2FAParser project2FAParser): base()
        {
            FileService = fileService;
            SerializationService = serializationService;
            Logger = loggerFacade;
            Project2FAParser = project2FAParser;

            //ErrorsChanged += Validation_ErrorsChanged;


        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            OTPList.Clear();
            await LoadIconNameCollection();
        }

    }
}
