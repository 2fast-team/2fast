#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using UNOversal.Services.Dialogs;
using System.Collections.ObjectModel;
using Windows.Services.Store;
using System.Threading.Tasks;
using Project2FA.Strings;
using Project2FA.UWP;
using Project2FA.UWP.Services;
using Project2FA.Core;
using Project2FA.Repository.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Project2FA.ViewModels
{
    public class InAppPaymentContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private bool _primaryButtonCanClick;
        private int _selectedIndex = -1;
        private bool _isLoading = false;
        private ISubscriptionService SubscriptionService { get; }

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
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    if (value != -1)
                    {
                        PrimaryButtonCanClick = true;
                    }
                    else
                    {
                        PrimaryButtonCanClick = false;
                    }
                }
            }
        }

        private bool _monthItemIsEnabled = false;
        public bool MonthItemIsEnabled 
        { 
            get => _monthItemIsEnabled; 
            set => SetProperty(ref _monthItemIsEnabled, value); 
        }

        private bool _monthItemIsChecked = false;
        public bool MonthItemIsChecked
        {
            get => _monthItemIsChecked;
            set
            {
                if(SetProperty(ref _monthItemIsChecked, value))
                {
                    PrimaryButtonCanClick = value;
                }
            }
        }

        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
        public InAppPaymentItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if(SetProperty(ref _selectedItem, value))
                {
                    if (value != null && value.IsEnabled && value.IsChecked)
                    {
                        PrimaryButtonCanClick = true;
                    }
                    else
                    {
                        PrimaryButtonCanClick = false;
                    }
                }
            }
        }

        public InAppPaymentContentDialogViewModel(ISubscriptionService subscriptionService)
        {
            SubscriptionService = subscriptionService;
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
            };
            Items.Add(monthlySupportModel);

            var yearsupportModel = new InAppPaymentItemModel

            {
                Description = Resources.InAppSubscriptionMonthSupport,
                Url = "ms-appx:///Assets/Images/give-love.png",
                UidCheckBox = Resources.InAppPaymentContentDialogSubscriptionItemSelect,
                IsEnabled = false,
                IsChecked = false,
            };
            Items.Add(yearsupportModel);

            SelectedItem = monthlySupportModel;

            SubscriptionService.Initialize(Constants.SupportSubscriptionId);
            (bool IsActiveMonthSubscription, StoreLicense info) = await SubscriptionService.SetupSubscriptionInfoAsync();
            bool inAppSubscriptionMonthCanSubscribe = !IsActiveMonthSubscription && info == null;
            monthlySupportModel.IsChecked = IsActiveMonthSubscription;
            monthlySupportModel.IsEnabled = inAppSubscriptionMonthCanSubscribe;


            //MonthItemIsChecked = IsActive;
            //MonthItemIsEnabled = inAppSubscriptionMonthCanSubscribe;

            IsLoading = false;
            //Items.Add(new InAppPaymentSubscriptionModel { Description = "OneYearSub", Url = "ms-appx:///Assets/FileLogo.png" });

            //Items.Add(new InAppPaymentSubscriptionModel { Description = "BuyLifetime", Url = "ms-appx:///Assets/FileLogo.png" });
            //bool userOwnsSubscription = isActive;
            //if (userOwnsSubscription && info != null)
            //{
            //    // Unlock all the subscription add-on features here.
            //    return;
            //}

            //// Get the StoreProduct that represents the subscription add-on.
            //subscriptionStoreProduct = await GetSubscriptionProductAsync();
            //if (subscriptionStoreProduct == null)
            //{
            //    return;
            //}

            //// Check if the first SKU is a trial and notify the customer that a trial is available.
            //// If a trial is available, the Skus array will always have 2 purchasable SKUs and the
            //// first one is the trial. Otherwise, this array will only have one SKU.
            //StoreSku sku = subscriptionStoreProduct.Skus[0];
            //if (sku.SubscriptionInfo.HasTrialPeriod)
            //{
            //    // You can display the subscription trial info to the customer here. You can use 
            //    // sku.SubscriptionInfo.TrialPeriod and sku.SubscriptionInfo.TrialPeriodUnit 
            //    // to get the trial details.
            //}
            //else
            //{
            //    // You can display the subscription purchase info to the customer here. You can use 
            //    // sku.SubscriptionInfo.BillingPeriod and sku.SubscriptionInfo.BillingPeriodUnit
            //    // to provide the renewal details.
            //}

            //// Prompt the customer to purchase the subscription.
            //await PromptUserToPurchaseAsync();
        }
    }
}
#endif