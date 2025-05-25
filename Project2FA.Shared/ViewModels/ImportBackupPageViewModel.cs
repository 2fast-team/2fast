using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Project2FA.Core.ProtoModels;
using OtpNet;
using UNOversal.Navigation;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UnoApp;
using Project2FA.Uno.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
    public class ImportBackupPageViewModel : AddAccountViewModelBase, IInitialize
    {
        public void Initialize(INavigationParameters parameters)
        {
            this.EntryEnum = Repository.Models.Enums.AccountEntryEnum.Import;
        }

        public async Task PrimaryButtonCommandTask()
        {
#if WINDOWS_UWP
            await CleanUpCamera();
#endif

            // multiple accounts to import?
            if (ImportCollection.Count > 0)
            {
                for (int i = 0; i < ImportCollection.Count; i++)
                {
                    if (ImportCollection[i].IsChecked && ImportCollection[i].IsEnabled)
                    {
                        DataService.Instance.Collection.Add(ImportCollection[i]);
                    }
                }
            }
            else
            {
                // add selected categories to the model and add to collection
                Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
                Model.SelectedCategories.AddRange(GlobalTempCategories.Where(x => x.IsSelected == true), true);
                DataService.Instance.Collection.Add(Model);
            }
#if __ANDROID__ || _IOS__
            await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
#endif
        }

        
    }
}
