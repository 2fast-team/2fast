using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Ioc;
using UNOversal.Services.Dialogs;
#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
#endif

namespace Project2FA.ViewModels
{

    public class CategoryFilterFlyoutViewModel : ObservableObject
    {
        public ICommand ManageCategoriesCommand { get; }
        public CategoryFilterFlyoutViewModel()
        {
            ManageCategoriesCommand = new AsyncRelayCommand(ManageCategoriesCommandTask);
        }

        private async Task ManageCategoriesCommandTask()
        {
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            ManageCategoriesContentDialog dialog = new ManageCategoriesContentDialog();
            await dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }
    }
}
