#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using UNOversal.Services.Dialogs;
using System.Collections.ObjectModel;
using Windows.Services.Store;
using System.Threading.Tasks;
using Project2FA.Strings;
using Project2FA.UWP.Services;
using Project2FA.Core;
using Project2FA.Repository.Models;
using Project2FA.Core.Messenger;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.Services;
using System;

namespace Project2FA.ViewModels
{
    public class InAppPaymentContentDialogViewModel : ObservableObject, IDialogInitialize, IRecipient<InAppItemChangedMessage>
    {
        private bool _primaryButtonCanClick;
        private int _selectedIndex = -1;
        private bool _isLoading = false;
        private IPurchaseAddOnService PurchaseService { get; }

        private InAppPaymentItemModel _selectedItem;
        public ObservableCollection<InAppPaymentItemModel> Items { get; } = new ObservableCollection<InAppPaymentItemModel>();

        public bool PrimaryButtonCanClick 
        {
            get => _primaryButtonCanClick; 
            set => SetProperty(ref _primaryButtonCanClick, value);
        }
        public int SelectedIndex 
        { 
            get => _selectedIndex; 
            set => SetProperty(ref _selectedIndex, value);
        }

        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        public InAppPaymentItemModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public InAppPaymentContentDialogViewModel(IPurchaseAddOnService subscriptionService)
        {
            PurchaseService = subscriptionService;
            WeakReferenceMessenger.Default.Register<InAppItemChangedMessage>(this);
        }

        // receive the status change from model
        public void Receive(InAppItemChangedMessage message)
        {
            // ensure that only one item is checked
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] != SelectedItem)
                {
                    Items[i].IsChecked = false;
                }
                else
                {
                    // if the selected item is the item of the list, check if the condition is complete
                    PrimaryButtonCanClick = Items[i].IsChecked && Items[i].IsEnabled;
                }
            }
        }

        public void Initialize(IDialogParameters parameters)
        {
            CheckSubscriptionsAndPurchases();
        }

        private async Task CheckSubscriptionsAndPurchases()
        {
            IsLoading = true;
            var monthlySupportModel = new InAppPaymentItemModel
            {
                Description = Resources.InAppSubscriptionMonthSupport,
                Url = "ms-appx:///Assets/Images/give-love.png",
                UidCheckBox = Resources.InAppPaymentContentDialogSubscriptionItemSelect,
                IsEnabled = false,
                IsChecked = false,
                StoreId = Constants.SupportSubscriptionId
            };
            Items.Add(monthlySupportModel);

            var yearSubscriptionModel = new InAppPaymentItemModel
            {
                Description = Resources.InAppSubscriptionYear,
                Url = "ms-appx:///Assets/Images/give-love.png",
                UidCheckBox = Resources.InAppPaymentContentDialogSubscriptionItemSelect,
                IsEnabled = false,
                IsChecked = false,
                StoreId = Constants.OneYearSubscriptionId
            };
            Items.Add(yearSubscriptionModel);

            var lifeTimeModel = new InAppPaymentItemModel
            {
                Description = Resources.InAppBuyOnce,
                Url = "ms-appx:///Assets/Images/give-love.png",
                UidCheckBox = Resources.InAppPaymentContentDialogBuyItemSelect,
                IsEnabled = false,
                IsChecked = false,
                StoreId = Constants.LifeTimeId
            };
            Items.Add(lifeTimeModel);

            //default item
            SelectedItem = monthlySupportModel;

            PurchaseService.Initialize(Constants.SupportSubscriptionId);
            (bool IsActiveMonthSubscription, StoreLicense infoMonth) = await PurchaseService.SetupPurchaseAddOnInfoAsync();
            bool inAppSubscriptionMonthCanSubscribe = !IsActiveMonthSubscription && infoMonth == null;
            monthlySupportModel.IsEnabled = inAppSubscriptionMonthCanSubscribe;
            monthlySupportModel.IsChecked = IsActiveMonthSubscription;
            

            PurchaseService.Initialize(Constants.OneYearSubscriptionId);
            (bool IsActiveYearSubscription, StoreLicense infoYear) = await PurchaseService.SetupPurchaseAddOnInfoAsync();
            bool inAppSubscriptionYearCanSubscribe = !IsActiveMonthSubscription && infoYear == null;
            if (IsActiveYearSubscription)
            {
                SelectedItem = yearSubscriptionModel;
            }
            yearSubscriptionModel.IsEnabled = inAppSubscriptionYearCanSubscribe;
            yearSubscriptionModel.IsChecked = IsActiveYearSubscription;
            

            PurchaseService.Initialize(Constants.LifeTimeId);
            (bool IsActiveLifeTimeBuy, StoreLicense infoLifeTime) = await PurchaseService.SetupPurchaseAddOnInfoAsync();
            bool inAppPurchaseCanSubscribe = !IsActiveLifeTimeBuy && infoLifeTime == null;
            if (IsActiveLifeTimeBuy)
            {
                SelectedItem = lifeTimeModel;
            }
            lifeTimeModel.IsEnabled = inAppPurchaseCanSubscribe;
            lifeTimeModel.IsChecked = IsActiveLifeTimeBuy;
            

            if (monthlySupportModel.IsChecked)
            {
                SelectedItem = monthlySupportModel;
                yearSubscriptionModel.IsEnabled = false;
                lifeTimeModel.IsEnabled = false;

                CheckProVersion(infoMonth);
            }

            if (yearSubscriptionModel.IsChecked)
            {
                SelectedItem = yearSubscriptionModel;
                monthlySupportModel.IsEnabled = false;
                lifeTimeModel.IsEnabled = false;

                CheckProVersion(infoYear);
            }

            if (lifeTimeModel.IsChecked)
            {
                SelectedItem = lifeTimeModel;
                yearSubscriptionModel.IsEnabled = false;
                monthlySupportModel.IsEnabled = false;

                CheckProVersion(infoLifeTime);
            }
            IsLoading = false;
        }

        private void CheckProVersion(StoreLicense storeLicense)
        {
            if (SettingsService.Instance.IsProVersion == false)
            {
                SettingsService.Instance.IsProVersion = true;
                SettingsService.Instance.PurchasedStoreId = storeLicense.SkuStoreId.Split("/")[0];
                SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                SettingsService.Instance.NextCheckedInPurchaseAddon = storeLicense.ExpirationDate;
            }
        }
    }
}
#endif