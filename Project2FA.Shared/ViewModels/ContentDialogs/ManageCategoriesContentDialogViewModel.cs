using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UNOversal.Services.Dialogs;

namespace Project2FA.ViewModels
{
    public class ManageCategoriesContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        public ICommand CreateCategoryCommand;
        public ObservableCollection<CategoryModel> TempGlobalCategories { get; private set; } = new ObservableCollection<CategoryModel>();
        //public ObservableCollection<CategoryModel> GlobalCategories
        //{
        //    get => DataService.Instance.GlobalCategories;
        //    set => DataService.Instance.GlobalCategories = value;
        //}
        private string _label;

        public string Label 
        { 
            get => _label; 
            set => SetProperty(ref _label, value);
        }

        public void Initialize(IDialogParameters parameters)
        {
            CreateCategoryCommand = new AsyncRelayCommand(CreateCategoryCommandTask);
            TempGlobalCategories = new ObservableCollection<CategoryModel>(DataService.Instance.GlobalCategories);
        }

        private Task CreateCategoryCommandTask()
        {
            TempGlobalCategories.Add(new CategoryModel { Guid = Guid.NewGuid(), IsSelected=false, Glyph = "\uEA8D", Name = Label });
            return Task.CompletedTask;
        }
    }
}
