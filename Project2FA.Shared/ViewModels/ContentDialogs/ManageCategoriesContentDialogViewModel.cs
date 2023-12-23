using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
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
    public class ManageCategoriesContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        public ICommand CreateCategoryCommand;
        private bool _dataChanged;
        private SymbolModel _selectedIconItem;
        private bool _canCreate;

        public ObservableCollection<SymbolModel> IconSourceCollection { get; private set; } = new ObservableCollection<SymbolModel>();
        public ObservableCollection<CategoryModel> TempGlobalCategories { get; private set; } = new ObservableCollection<CategoryModel>();
        //public ObservableCollection<CategoryModel> GlobalCategories
        //{
        //    get => DataService.Instance.GlobalCategories;
        //    set => DataService.Instance.GlobalCategories = value;
        //}
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
            TempGlobalCategories = new ObservableCollection<CategoryModel>(DataService.Instance.GlobalCategories);
        }

        private Task CreateCategoryCommandTask()
        {
            // no duplicate allowed
            if (TempGlobalCategories.Where(x => x.Name == Label).FirstOrDefault() == null)
            {
                DataChanged = true;
                //TempGlobalCategories.Add(new CategoryModel { Guid = Guid.NewGuid(), IsSelected = false, Glyph = "\uEA8D", Name = Label });
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
