#if WINDOWS_UWP

using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using UNOversal.Services.Dialogs;
using Project2FA.Models;
using System.Collections.ObjectModel;
using Windows.Services.Store;
using System.Threading.Tasks;
using Project2FA.Strings;
using Project2FA.UWP;
using Project2FA.UWP.Services;
using Project2FA.Core;

namespace Project2FA.ViewModels
{
    public class InAppPaymentContentDialogViewModel : ObservableObject, IDialogInitialize
    {
        private bool _primaryButtonCanClick;
        private int _selectedIndex = -1;
        public ObservableCollection<InAppPaymentSubscriptionModel> Items { get; } = new ObservableCollection<InAppPaymentSubscriptionModel>();

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



        public void Initialize(IDialogParameters parameters)
        {
            CheckSubscriptionsAndPurchases();
        }

        private async Task CheckSubscriptionsAndPurchases()
        {
            //var monthlySupportModel = new InAppPaymentSubscriptionModel
            //{
            //    Description = Resources.InAppSubscriptionMonthSupport,
            //    Url = "ms-appx:///Assets/Images/give-love.png",
            //    IsEnabled = false
            //};
            //Items.Add(monthlySupportModel);
            var subService = App.Current.Container.Resolve<ISubscriptionService>();
            subService.Initialize(Constants.SupportSubscriptionId);
            (bool IsActive, StoreLicense info) = await subService.SetupSubscriptionInfoAsync();
            bool inAppSubscriptionMonthCanSubscribe = !IsActive && info == null;
            MonthItemIsChecked = IsActive;
            MonthItemIsEnabled = inAppSubscriptionMonthCanSubscribe;
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