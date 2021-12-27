using Prism.Mvvm;
using System;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using Project2FA.Views;
#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace Project2FA.ViewModels
{
    public class WelcomePageViewModel : BindableBase
    {
        IDialogService _dialogService { get; }
        IRegionManager _regionManager { get; }
        public ICommand NewDatefileCommand { get; }
        public ICommand TutorialCommand { get; }
        public ICommand OpenTutorialCommand { get; }

        public ICommand UseExistDatefileCommand { get; }
        private bool _isTutorialOpen;

        private string _title;

        //private bool _canNavigate;

        public WelcomePageViewModel(IDialogService dialogService, IRegionManager regionManager)
        {
            _dialogService = dialogService;
            _regionManager = regionManager;
            // disable the navigation to other pages
            //App.ShellPageInstance.NavigationIsAllowed = false;
            //Title = Strings.Resources.WelcomePageTitle;

            NewDatefileCommand = new DelegateCommand(() =>
            {
                NewDatafile();
            });
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            UseExistDatefileCommand = new DelegateCommand(() =>
            {
                _regionManager.RequestNavigate("ContentRegion", nameof(UseDataFilePage));
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            TutorialCommand = new DelegateCommand(() =>
            {
                IsTutorialOpen = !IsTutorialOpen;
            });
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            OpenTutorialCommand = new DelegateCommand(() =>
            {
                //var dialog = new ContentDialog();
                //dialog.Title = "#test";
                //dialog.PrimaryButtonText = "#bla";
                //if (IsTutorialOpen)
                //{
                //    IsTutorialOpen = false;
                //}
                //await _dialogService.ShowDialog();
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private async Task NewDatafile()
        {
            //var dialog = new NewDatafileDialog();
            //var result = await _dialogService.ShowDialog(dialog, new DialogParameters());
            //if (result == ContentDialogResult.Primary)
            //{
            //    //_canNavigate = true;
            //    string navPath = "/" + nameof(AccountCodePage);
            //    _regionManager.RequestNavigate("ContentRegion", navPath);
            //}
        }

        //private async Task UseExistDatafile()
        //{
        //    try
        //    {
        //        UseDatafileContentDialog dialog = new UseDatafileContentDialog();
        //        ContentDialogResult result = await _dialogService.ShowDialogAsync(dialog, new DialogParameters());

        //        //result is also none, when the datafileDB is correct created
        //        if (result == ContentDialogResult.None)
        //        {
        //            var datafileDB = await App.Repository.Datafile.GetAsync();
        //            if (datafileDB != null)
        //            {
        //                //_canNavigate = true;
        //                _regionManager.RequestNavigate("ContentRegion", nameof(AccountCodePage));
        //                //await _navigationService.NavigateAsync("/" + nameof(AccountCodePage));
        //            }
        //        }
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        //ErrorDialogs.UnauthorizedAccessDialog();
        //    }
        //}

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool IsTutorialOpen { get => _isTutorialOpen; set => SetProperty(ref _isTutorialOpen, value); }
    }
}
