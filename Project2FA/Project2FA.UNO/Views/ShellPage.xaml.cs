using Microsoft.UI.Xaml.Controls;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UNOversal.Navigation;
using Windows.UI.Core;
using Frame = Microsoft.UI.Xaml.Controls.Frame;

namespace Project2FA.UNO.Views
{
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private SystemNavigationManager _navManager;
        private bool _navigationIsAllowed = true;
        public INavigationService NavigationService { get; private set; }
        public NavigationView ShellViewInternal { get; private set; }

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
            NavigationService = NavigationFactory.Create(MainFrame);

            SetupGestures();
        }

        private void SetupGestures()
        {
            _navManager.BackRequested += NavManager_BackRequested;
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ShellView.BackRequested += async (s, e) => await NavigationService.GoBackAsync();
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
        }

        private async void NavManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (NavigationService.CanGoBack())
            {
                e.Handled = true;
                await NavigationService.GoBackAsync();
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
                    if ((await NavigationService.NavigateAsync(_settingsNavigationStr)).Success)
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
                    else if ((await NavigationService.NavigateAsync(path)).Success)
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
