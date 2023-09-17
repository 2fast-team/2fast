using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Uno.Toolkit.UI;
using UNOversal.Navigation;
using Windows.UI.Core;
using Frame = Microsoft.UI.Xaml.Controls.Frame;

namespace Project2FA.UNO.Views
{
    public sealed partial class ShellPage : Page
    {
        private SystemNavigationManager _navManager;
        
        public NavigationView ShellViewInternal { get; private set; }

        public ShellPageViewModel ViewModel { get; } = new ShellPageViewModel();

        public Frame MainFrame { get; }

        private string _settingsNavigationStr;

        private object PreviousItem { get; set; }
        public ShellPage()
        {
            this.InitializeComponent();
            _navManager = SystemNavigationManager.GetForCurrentView();
            _settingsNavigationStr = "SettingPage?PivotItem=0";
            ShellViewInternal = ShellView;
            ShellView.Content = MainFrame = new Frame();
            ViewModel.NavigationService = NavigationFactory.Create(MainFrame);

            SetupGestures();
        }

        private void SetupGestures()
        {
            _navManager.BackRequested += NavManager_BackRequested;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ShellView.BackRequested += async (s, e) => await ViewModel.NavigationService.GoBackAsync();
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private async void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ViewModel.NavigationService.CanGoBack())
            {
                e.Handled = true;
                await ViewModel.NavigationService.GoBackAsync();
#if ANDROID || IOS
                if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
                {
                    switch (uIElement)
                    {
                        case AccountCodePage:
                            ViewModel.SelectedIndex = 0;
                            break;
                        //case SearchPage:
                        //    ViewModel.SelectedIndex = 1;
                            break;
                        case SettingPage:
                            ViewModel.SelectedIndex = 2;
                            break;
                        default:
                            break;
                    }
                }
#endif
            }
            else
            {
#if ANDROID || IOS
                // if the search is seleted, a back command go to
                // the AccountCodePage index
                if (ViewModel.SelectedIndex == 1)
                {
                    e.Handled = true;
                    ViewModel.SelectedIndex = 0;
                }
#endif
            }
        }

        private async Task SetSelectedItem(object selectedItem, bool withNavigation = true)
        {
            if (selectedItem == null)
            {
                ShellView.SelectedItem = null;
            }
            else if (selectedItem == PreviousItem)
            {
                // already set
            }
            else if (selectedItem == ShellView.SettingsItem)
            {
                if (withNavigation)
                {
                    if ((await ViewModel.NavigationService.NavigateAsync(_settingsNavigationStr)).Success)
                    {
                        PreviousItem = selectedItem;
                        ShellView.SelectedItem = selectedItem;
                    }
                    else
                    {
                        ShellView.SelectedItem = null;
                    }
                }
                else
                {
                    PreviousItem = selectedItem;
                    ShellView.SelectedItem = selectedItem;
                }
            }
            else if (selectedItem is NavigationViewItem item)
            {
                if (item.Tag is string path)
                {
                    if (!withNavigation)
                    {
                        PreviousItem = item;
                        ShellView.SelectedItem = item;
                    }
                    else if ((await ViewModel.NavigationService.NavigateAsync(path)).Success)
                    {
                        PreviousItem = selectedItem;
                        ShellView.SelectedItem = selectedItem;
                    }
                    else
                    {
                        ShellView.SelectedItem = PreviousItem;
                    }
                }
            }
        }

        private bool TryFindItem(Type type, object parameter, out object item)
        {
            // is page registered?

            if (!PageNavigationRegistry.TryGetRegistration(type, out PageNavigationInfo info))
            {
                item = null;
                return false;
            }

            // search settings

            if (NavigationQueue.TryParse(_settingsNavigationStr, null, out NavigationQueue settings))
            {
                if (type == settings.Last().View && (string)parameter == settings.Last().QueryString)
                {
                    item = ShellView.SettingsItem;
                    return true;
                }
                else
                {
                    // not settings
                }
            }

            // filter menu items
            IEnumerable<(NavigationViewItem Item, string Path)> menuItems = ShellView.MenuItems
                .OfType<NavigationViewItem>()
                .Select(x => (
                    Item: x,
                    Path: x.Tag as string
                ))
                .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            foreach ((NavigationViewItem Item, string Path) in menuItems)
            {
                if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
                    && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
                {
                    item = Item;
                    return true;
                }
            }

            // filter footer menu items
            IEnumerable<(NavigationViewItem Item, string Path)> footerMenuItems = ShellView.FooterMenuItems
                .OfType<NavigationViewItem>()
                .Select(x => (
                    Item: x,
                    Path: x.Tag as string
                ))
                .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            foreach ((NavigationViewItem Item, string Path) in footerMenuItems)
            {
                if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
                    && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
                {
                    item = Item;
                    return true;
                }
            }

            // not found

            item = null;
            return false;
        }

        private NavigationViewItem Find(NavigationViewItem item)
        {
            NavigationViewItem menuItem = ShellView.MenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            if (menuItem is null)
            {
                menuItem = ShellView.FooterMenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            }
            return menuItem;
        }
    }
}
