#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using Prism.Regions;
using Prism.Ioc;

namespace Project2FA.Views
{
    public sealed partial class ShellPage : ContentControl, INotifyPropertyChanged
    {
        public NavigationView ShellViewInternal { get; private set; }
        private IRegionManager navigationService;
        public ShellPage(IRegionManager regionManager)
        {
            this.InitializeComponent();
            _settingsNavigationStr = "SettingPage?PivotItem=0";
            navigationService = regionManager;// containerProvider.Resolve<IRegionManager>();

            //MainFrame.Navigated += (s, e) =>
            //{
            //    if (TryFindItem(e.SourcePageType, e.Parameter, out object item))
            //    {
            //        SetSelectedItem(item, false);
            //    }
            //};

            ShellView.ItemInvoked += (sender, args) =>
            {
                SelectedItem = args.IsSettingsInvoked ? ShellView.SettingsItem : Find(args.InvokedItemContainer as NavigationViewItem);
            };
        }

        private string _settingsNavigationStr;
        private bool _navigationIsAllowed;

        private object PreviousItem { get; set; }

        private object SelectedItem
        {
            set => SetSelectedItem(value);
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
                    //TOOD Params
                    navigationService.RequestNavigate("ContentRegion", nameof(SettingsPage));
                    PreviousItem = selectedItem;
                    ShellView.SelectedItem = selectedItem;
                    //if ((navigationService.RequestNavigate("ContentRegion", _settingsNavigationStr)).Success)
                    //{
                    //    PreviousItem = selectedItem;
                    //    ShellView.SelectedItem = selectedItem;
                    //}
                    //else
                    //{
                    //    ShellView.SelectedItem = null;
                    //}
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
                    else
                    {
                        navigationService.RequestNavigate("ContentRegion", path);
                        PreviousItem = selectedItem;
                        ShellView.SelectedItem = selectedItem;
                    }
                    //else if ((await NavigationService.NavigateAsync(path)).Success)
                    //{
                    //    PreviousItem = selectedItem;
                    //    ShellView.SelectedItem = selectedItem;
                    //}
                    //else
                    //{
                    //    ShellView.SelectedItem = PreviousItem;
                    //}
                }
            }
        }

        private bool TryFindItem(Type type, object parameter, out object item)
        {
            // is page registered?

            //if (!PageNavigationRegistry.TryGetRegistration(type, out PageNavigationInfo info))
            //{
            //    item = null;
            //    return false;
            //}

            // search settings

            //if (NavigationQueue.TryParse(_settingsNavigationStr, null, out NavigationQueue settings))
            //{
            //    if (type == settings.Last().View && (string)parameter == settings.Last().QueryString)
            //    {
            //        item = ShellView.SettingsItem;
            //        return true;
            //    }
            //    else
            //    {
            //        // not settings
            //    }
            //}

            // filter menu items
            IEnumerable<(NavigationViewItem Item, string Path)> menuItems = this.ShellView.MenuItems
                .OfType<NavigationViewItem>()
                .Select(x => (
                    Item: x,
                    Path: x.Tag as string
                ))
                .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            //foreach ((NavigationViewItem Item, string Path) in menuItems)
            //{
            //    if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
            //        && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
            //    {
            //        item = Item;
            //        return true;
            //    }
            //}

            // filter footer menu items
            //IEnumerable<(NavigationViewItem Item, string Path)> footerMenuItems = ShellView.FooterMenuItems
            //    .OfType<NavigationViewItem>()
            //    .Select(x => (
            //        Item: x,
            //        Path: x.Tag as string
            //    ))
            //    .Where(x => !string.IsNullOrEmpty(x.Path));

            // search filtered items

            //foreach ((NavigationViewItem Item, string Path) in footerMenuItems)
            //{
            //    if (NavigationQueue.TryParse(Path, null, out NavigationQueue menuQueue)
            //        && Equals(menuQueue.Last().View, type) && menuQueue.Last().QueryString == (string)parameter)
            //    {
            //        item = Item;
            //        return true;
            //    }
            //}

            // not found

            item = null;
            return false;
        }

        private NavigationViewItem Find(NavigationViewItem item)
        {
            NavigationViewItem menuItem = ShellView.MenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            //if (menuItem is null)
            //{
            //    menuItem = ShellView.FooterMenuItems.OfType<NavigationViewItem>().SingleOrDefault(x => x.Equals(item) && x.Tag != null);
            //}
            return menuItem;
        }

        #region NotifyPropertyChanged
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        #endregion
        /// <summary>
        /// Allow or disable the NavigationView items
        /// </summary>
        public bool NavigationIsAllowed
        {
            get => _navigationIsAllowed;
            set => SetProperty(ref _navigationIsAllowed, value);
        }
    }
}
