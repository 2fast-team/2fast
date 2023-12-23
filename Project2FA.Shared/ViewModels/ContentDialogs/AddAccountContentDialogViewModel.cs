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
    public class AddAccountContentDialogViewModel : AddAccountViewModelBase, IDialogInitialize
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public AddAccountContentDialogViewModel(
            ISerializationService serializationService,
            ILoggerFacade loggerFacade,
            IProject2FAParser project2FAParser): base()
        {
            SerializationService = serializationService;
            Logger = loggerFacade;
            Project2FAParser = project2FAParser;

            //ErrorsChanged += Validation_ErrorsChanged;


        }

        public void Initialize(IDialogParameters parameters)
        {
            OTPList.Clear();
        }
    }
}
