using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Project2FA.Core.Messenger;
using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class ManageCategoriesContentDialogViewModel : ObservableRecipient, IDialogInitialize
    {
        public ICommand CreateCategoryCommand;
        public ICommand PrimaryCommand;
        private bool _dataChanged;
        private SymbolModel _selectedIconItem;
        private bool _canCreate;

        public ObservableCollection<SymbolModel> IconSourceCollection { get; private set; } = new ObservableCollection<SymbolModel>();
        public ObservableCollection<CategoryModel> TempGlobalCategories { get; private set; } = new ObservableCollection<CategoryModel>();
        private string _label;



        public void Initialize(IDialogParameters parameters)
        {
            CreateCategoryCommand = new AsyncRelayCommand(CreateCategoryCommandTask);
            ObservableCollection<SymbolModel> tempCollection = new ObservableCollection<SymbolModel>();
            foreach (Symbol symbol in (Symbol[])Enum.GetValues(typeof(Symbol)))
            {
                tempCollection.Add(new SymbolModel { Symbol = symbol, Name = symbol.ToString() });
            }
            IconSourceCollection.AddRange(tempCollection.OrderBy(x => x.Name));
            TempGlobalCategories ??= new ObservableCollection<CategoryModel>();
            for (int i = 0; i < DataService.Instance.GlobalCategories.Count; i++)
            {
                // clone the item without a reference
                TempGlobalCategories.Add((CategoryModel)DataService.Instance.GlobalCategories[i].Clone());
            }
            PrimaryCommand = new AsyncRelayCommand(PrimaryCommandTask);
        }

        private Task PrimaryCommandTask()
        {
            DataService.Instance.GlobalCategories.AddRange(TempGlobalCategories, true);
            //for (int i = 0; i < TempGlobalCategories.Count; i++)
            //{
            //    // check for changed names
            //    var foundSameItems = DataService.Instance.GlobalCategories.Where(x => x.Guid == TempGlobalCategories[i].Guid).FirstOrDefault();
            //    if (foundSameItems != null)
            //    {
            //        if (foundSameItems.Name != TempGlobalCategories[i].Name)
            //        {
            //            foundSameItems.Name = TempGlobalCategories[i].Name;
            //        }
            //    }
            //    if (DataService.Instance.GlobalCategories.Count == 0)
            //    {
            //        DataService.Instance.GlobalCategories.Add(TempGlobalCategories[i]);
            //    }
            //    else
            //    {
            //        if (DataService.Instance.GlobalCategories.Where(x => x.Guid != TempGlobalCategories[i].Guid).Any())
            //        {
            //            DataService.Instance.GlobalCategories.Add(TempGlobalCategories[i]);
            //        }
            //    }
            //}
            DataService.Instance.WriteLocalDatafile();
            Messenger.Send(new CategoriesChangedMessage(true));
            return Task.CompletedTask;
            
        }

        private Task CreateCategoryCommandTask()
        {
            // no duplicate allowed
            if (TempGlobalCategories.Where(x => x.Name == Label).FirstOrDefault() == null)
            {
                DataChanged = true;
                TempGlobalCategories.Add(new CategoryModel 
                { 
                    Guid = Guid.NewGuid(),
                    IsSelected = false,
                    UnicodeString = "\uE821",
                    UnicodeIndex = "59425",
                    Name = Label });
            }
            else
            {
                // show error
            }
            
            return Task.CompletedTask;
        }
        private void ValidateInput()
        {
            if (!string.IsNullOrWhiteSpace(Label) && SelectedIconItem != null)
            {
                CanCreate = true;
            }
            else
            {
                CanCreate = false;
            }
        }

        public string Label
        {
            get => _label;
            set 
            { 
                if(SetProperty(ref _label, value))
                {
                    ValidateInput();
                }
            }
        }

        public bool DataChanged
        {
            get => _dataChanged;
            set => SetProperty(ref _dataChanged, value);
        }

        public SymbolModel SelectedIconItem 
        {
            get => _selectedIconItem;
            set
            {
                if(SetProperty(ref _selectedIconItem, value))
                {
                    ValidateInput();
                }
            }
        }

        public bool CanCreate 
        { 
            get => _canCreate; 
            set => SetProperty(ref _canCreate, value);
        }
    }
}
