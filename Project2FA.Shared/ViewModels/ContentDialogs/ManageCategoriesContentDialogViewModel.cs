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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;
using Windows.Storage.Streams;
using Windows.Storage;
using UNOversal.Services.Serialization;


#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class ManageCategoriesContentDialogViewModel : ObservableRecipient, IDialogInitializeAsync
    {
        public ICommand CreateCategoryCommand;
        public ICommand PrimaryCommand;
        private bool _dataChanged;
        private FontIdentifikationModel _selectedIconItem;
        private bool _canCreate;
        private ISerializationService SerializationService { get; }

        public ObservableCollection<FontIdentifikationModel> IconSourceCollection { get; private set; } = new ObservableCollection<FontIdentifikationModel>();
        public ObservableCollection<CategoryModel> TempGlobalCategories { get; private set; } = new ObservableCollection<CategoryModel>();
        private string _label;

        public ManageCategoriesContentDialogViewModel(ISerializationService serializationService)
        {
            SerializationService = serializationService;
        }

        public async Task InitializeAsync(IDialogParameters parameters)
        {
            CreateCategoryCommand = new AsyncRelayCommand(CreateCategoryCommandTask);
            //ObservableCollection<SymbolModel> tempCollection = new ObservableCollection<SymbolModel>();
            //foreach (Symbol symbol in (Symbol[])Enum.GetValues(typeof(Symbol)))
            //{
            //    tempCollection.Add(new SymbolModel { Symbol = symbol, Name = symbol.ToString() });
            //}
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/CategoryIcons.json"));
            IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
            using StreamReader r = new StreamReader(randomStream.AsStreamForRead());
            IconSourceCollection.AddRange(SerializationService.Deserialize<ObservableCollection<FontIdentifikationModel>>(await r.ReadToEndAsync()));
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
                    UnicodeString = SelectedIconItem.UnicodeString,
                    UnicodeIndex = SelectedIconItem.UnicodeIndex.ToString(),
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

        public FontIdentifikationModel SelectedIconItem 
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
