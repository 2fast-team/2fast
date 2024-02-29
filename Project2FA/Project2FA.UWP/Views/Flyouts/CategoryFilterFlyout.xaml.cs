using Project2FA.Repository.Models;
using Project2FA.ViewModels;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class CategoryFilterFlyout : Page
    {
        public CategoryFilterFlyoutViewModel ViewModel { get; } = new CategoryFilterFlyoutViewModel();
        public CategoryFilterFlyout()
        {
            this.InitializeComponent();
        }

        private void TokenView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is CategoryModel item)
            {
                item.IsSelected = !item.IsSelected;
                ViewModel.CanSaveFilter = true;
            }
        }


        private void TV_Categories_Loaded(object sender, RoutedEventArgs e)
        {
            var selectedItems = ViewModel.GlobalTempCategories.Where(x => x.IsSelected == true);
            if (selectedItems.Count() > 0)
            {
                ViewModel.CanResetFilter = true;
            }
            for (int i = 0; i < selectedItems.Count(); i++)
            {
                TV_Categories.SelectedItems.Add(selectedItems.ElementAt(i));
            }
            
        }

        private void BTN_SaveFilter_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveCategorySetting();

            FindAndCloseFlyout(sender);
        }

        private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            FindAndCloseFlyout(sender);
        }

        private void ABBTN_ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            FindAndCloseFlyout(sender);
        }

        private void FindAndCloseFlyout(object sender)
        {
            var parent = MainPage ?? sender as DependencyObject;
            while (parent != null)
            {
                if (parent is FlyoutPresenter)
                {
                    ((parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                    break;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
        }


        private void ABTN_ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            TV_Categories.SelectedItems.Clear();
            ViewModel.CanResetFilter = false;
            for (int i = 0; i < ViewModel.GlobalTempCategories.Count; i++)
            {
                ViewModel.GlobalTempCategories[i].IsSelected = false;
            }
            ViewModel.SaveCategorySetting();
        }
    }
}
