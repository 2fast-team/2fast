using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Ioc;
using UNOversal.Services.Dialogs;
using Project2FA.Services;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Core.Messenger;
using Project2FA.Repository.Models;
using System.Collections.ObjectModel;
using Project2FA.Core.Utils;
using System.Linq;


#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;

#else
using Project2FA.UNO;
using Project2FA.UNO.Views;
using Microsoft.UI.Xaml;

#endif

namespace Project2FA.ViewModels
{

    public class CategoryFilterFlyoutViewModel : ObservableRecipient
    {
        public ICommand ManageCategoriesCommand { get; }
        private bool _canSaveFilter, _canResetFilter;
        private bool _categoriesExists;
        public ObservableCollection<CategoryModel> GlobalTempCategories { get; } = new ObservableCollection<CategoryModel>();
        public CategoryFilterFlyoutViewModel()
        {
            ManageCategoriesCommand = new AsyncRelayCommand(ManageCategoriesCommandTask);
            OnPropertyChanged(nameof(NoCategoriesExists));

            Messenger.Register<CategoryFilterFlyoutViewModel, CategoriesChangedMessage>(this, (r, m) =>
            {
                OnPropertyChanged(nameof(NoCategoriesExists));
                CanSaveFilter = false;
                GlobalTempCategories.AddRange(DataService.Instance.GlobalCategories.Select(x => (CategoryModel)x.Clone()).ToList(), true);
            });
        }

        private async Task ManageCategoriesCommandTask()
        {
            var dialogService = App.Current.Container.Resolve<IDialogService>();
            ManageCategoriesContentDialog dialog = new ManageCategoriesContentDialog();
            await dialogService.ShowDialogAsync(dialog, new DialogParameters());
        }

        public void SaveCategorySetting()
        {
            // save the selected or not selected categories
            for (int i = 0; i < GlobalTempCategories.Count; i++)
            {
                DataService.Instance.GlobalCategories[i].IsSelected = GlobalTempCategories[i].IsSelected;
            }

            if (DataService.Instance.GlobalCategories.Where(x => x.IsSelected == true).Any())
            {
                if (DataService.Instance.IsFilterChecked)
                {
                    Messenger.Send(new FilteringChangedMessage(true));
                }
                else
                {
                    DataService.Instance.IsFilterChecked = true;
                }

                DataService.Instance.IsFilterEnabled = true;
            }
            else
            {
                DataService.Instance.IsFilterEnabled = false;
                DataService.Instance.IsFilterChecked = false;
                Messenger.Send(new FilteringChangedMessage(false));
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

        public bool CanSaveFilter 
        { 
            get => _canSaveFilter; 
            set => SetProperty(ref _canSaveFilter, value);
        }
        public bool CanResetFilter
        { 
            get => _canResetFilter; 
            set => SetProperty(ref _canResetFilter, value); 
        }
    }
}
