using Microsoft.UI.Xaml.Controls;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI;
using UNOversal.Extensions;
using Windows.UI.Xaml.Controls.Primitives;

namespace Project2FA.UWP.Views
{
    public sealed partial class EditAccountContentDialog : ContentDialog
    {
        public EditAccountContentDialogViewModel ViewModel => DataContext as EditAccountContentDialogViewModel;
        public EditAccountContentDialog()
        {
            this.InitializeComponent();

            Loaded += EditAccountContentDialog_Loaded;
        }

        private void EditAccountContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
            // match the guid and add the items from GlobalTempCategories collection to the SelectedItems source.
            var selectedItems = ViewModel.GlobalTempCategories.Where(x => ViewModel.Model.SelectedCategories.Where(selected => selected.Guid == x.Guid).Any());
            for (int i = 0; i < selectedItems.Count(); i++)
            {
                TV_Categories.SelectedItems.Add(selectedItems.ElementAt(i));
                selectedItems.ElementAt(i).IsSelected = true;
            }
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                        switch (SettingsService.Instance.OriginalAppTheme)
                        {
                            case ApplicationTheme.Light:
                                ReplaceNoteFontColor(true);
                                break;
                            case ApplicationTheme.Dark:
                                ReplaceNoteFontColor(false);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                        ReplaceNoteFontColor(false);
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                        ReplaceNoteFontColor(true);
                    }
                    break;
                default:
                    break;
            }


            if (!string.IsNullOrWhiteSpace(ViewModel.Notes))
            {
                // TODO replace RTF control with markdown:
                // https://stackoverflow.com/questions/46119392/how-do-i-convert-an-rtf-string-to-a-markdown-string-and-back-c-net-core-or
                // TODO bug workaround https://github.com/microsoft/microsoft-ui-xaml/issues/1941
                var options = Windows.UI.Text.TextSetOptions.FormatRtf | Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults;
                REB_Notes.Document.SetText(options, ViewModel.Notes);
            }
        }

        private void ReplaceNoteFontColor(bool isLightTheme)
        {
            if (isLightTheme)
            {
                ViewModel.Notes = ViewModel.Notes.Replace(@"\red255\green255\blue255", @"\red0\green0\blue0");
            }
            else
            {
                ViewModel.Notes = ViewModel.Notes.Replace(@"\red0\green0\blue0", @"\red255\green255\blue255");
            }
        }

        private void REB_Notes_TextChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.Notes = Toolbar.Formatter?.Text;
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.SearchAccountFonts(sender.Text);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is FontIdentifikationModel selectedItem)
            {
                if (selectedItem.Name != Strings.Resources.AccountCodePageSearchNotFound)
                {
                    ViewModel.TempModel.AccountIconName = selectedItem.Name;
                }
                else
                {
                    sender.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Is triggered when the element is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TokenView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is CategoryModel model)
            {
                model.IsSelected = !model.IsSelected;
            }
        }

        /// <summary>
        /// Automatic search when the control has the focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            await ViewModel.SearchAccountFonts(ViewModel.TempModel.AccountIconName);
        }

        private void HLBTN_CategoryInfo(object sender, RoutedEventArgs e)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = sender as FrameworkElement,
                MaxWidth = 400,
                Subtitle = Strings.Resources.EditAccountContentDialogAccountCategoryInfoText,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true
            };
            RootGrid.Children.Add(teachingTip);
        }

        private void SettingsExpander_Expanded(object sender, EventArgs e)
        {

        }

        private void BTN_SearchIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                FlyoutBase.ShowAttachedFlyout(btn);
            }
        }
    }
}
