using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Serialization;
using UNOversal.Logging;
using UNOversal.Services.File;
using Project2FA.Services.Parser;
using Project2FA.Services;
using System.Linq;
using System.Collections.ObjectModel;
using Project2FA.Repository.Models;
using Project2FA.Core.Utils;
using System.Collections.Generic;

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
        public ObservableCollection<FontIdentifikationModel> FontIdentifikationCollection { get; } = new ObservableCollection<FontIdentifikationModel>();
        public ObservableCollection<CategoryModel> GlobalTempCategories { get; } = new ObservableCollection<CategoryModel>();

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
            if (parameters.TryGetValue<List<KeyValuePair<string, string>>>("account", out var account))
            {
                ParseQRCode(account);
                PivotViewSelectionName = "NormalInputAccount";
            }
            OTPList.Clear();
            if (DataService.Instance.GlobalCategories != null && DataService.Instance.GlobalCategories.Count > 0)
            {
                GlobalTempCategories.Clear();
                for (int i = 0; i < DataService.Instance.GlobalCategories.Count; i++)
                {
                    GlobalTempCategories.Add((CategoryModel)DataService.Instance.GlobalCategories[i].Clone());
                }
            }
        }

        public Task<bool> SearchAccountFonts(string senderText)
        {
            if (string.IsNullOrEmpty(senderText) == false && senderText.Length >= 2 && senderText != Strings.Resources.AccountCodePageSearchNotFound)
            {
                var tempList = DataService.Instance.FontIconCollection.Where(x => x.Name.Contains(senderText, System.StringComparison.OrdinalIgnoreCase)).ToList();
                FontIdentifikationCollection.AddRange(tempList, true);
                try
                {
                    if (FontIdentifikationCollection.Count == 0)
                    {
                        FontIdentifikationCollection.Add(new FontIdentifikationModel { Name = Strings.Resources.AccountCodePageSearchNotFound });
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(true);
                }
                catch (System.Exception)
                {
                    FontIdentifikationCollection.Clear();
                    return Task.FromResult(false);
                }
            }
            else
            {
                FontIdentifikationCollection.Clear();
                return Task.FromResult(false);
            }
        }

        public bool NoCategoriesExists
        {
            get
            {
                return DataService.Instance.GlobalCategories.Count == 0;
            }
        }

        public bool CategoriesExists
        {
            get
            {
                return DataService.Instance.GlobalCategories.Count > 0;
            }
        }
#if WINDOWS_UWP
        public bool IsProVersion
        {
            get => SettingsService.Instance.IsProVersion;
        }
#endif
    }
}
