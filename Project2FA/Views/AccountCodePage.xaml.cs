using Project2FA.Repository.Models;
using Project2FA.Resources;
using Project2FA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Extensions;

namespace Project2FA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountCodePage : ContentPage
    {
        AccountCodePageViewModel ViewModel => BindingContext as AccountCodePageViewModel;
        public AccountCodePage()
        {
            InitializeComponent();
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is TwoFACodeModel model)
            {
                await Clipboard.SetTextAsync(model.TwoFACode);
                await this.DisplayToastAsync(AppResource.Copyed);
            }
        }

        private void MI_DeleteAccount_Clicked(object sender, EventArgs e)
        {
            if ((sender as MenuItem).CommandParameter is TwoFACodeModel model)
            {
                ViewModel.DeleteAccountFromCollection(model);
            }
        }

        private void MI_EditAccount_Clicked(object sender, EventArgs e)
        {
            if ((sender as MenuItem).CommandParameter is TwoFACodeModel model)
            {
                ViewModel.EditAccountFromCollection(model);
            }
        }
    }
}