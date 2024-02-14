using Project2FA.Repository.Models;
using Project2FA.ViewModels;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


namespace Project2FA.UWP.Views
{
    public sealed partial class InAppPaymentContentDialog : ContentDialog
    {
        public InAppPaymentContentDialogViewModel ViewModel => DataContext as InAppPaymentContentDialogViewModel;
        public InAppPaymentContentDialog()
        {
            this.InitializeComponent();

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ToggleButton tbtn && tbtn.DataContext is InAppPaymentItemModel model)
            {
                for (int i = 0; i < ViewModel.Items.Count; i++)
                {
                    if (ViewModel.Items[i] != model)
                    {
                        ViewModel.Items[i].IsChecked = false;
                    }
                }
            }
        }
    }
}
