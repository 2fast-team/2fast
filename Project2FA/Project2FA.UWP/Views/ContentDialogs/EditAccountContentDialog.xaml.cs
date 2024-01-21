using Project2FA.Extensions;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            if (!string.IsNullOrWhiteSpace(ViewModel.TempNotes))
            {
                // TODO replace RTF control with markdown:
                // https://stackoverflow.com/questions/46119392/how-do-i-convert-an-rtf-string-to-a-markdown-string-and-back-c-net-core-or
                // TODO bug workaround https://github.com/microsoft/microsoft-ui-xaml/issues/1941
                var options = Windows.UI.Text.TextSetOptions.FormatRtf | Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults;
                REB_Notes.Document.SetText(options, ViewModel.TempNotes);
            }
        }

        private void ReplaceNoteFontColor(bool isLightTheme)
        {
            if (isLightTheme)
            {
                ViewModel.TempNotes = ViewModel.TempNotes.Replace(@"\red255\green255\blue255", @"\red0\green0\blue0");
            }
            else
            {
                ViewModel.TempNotes = ViewModel.TempNotes.Replace(@"\red0\green0\blue0", @"\red255\green255\blue255");
            }
        }

        private void REB_Notes_TextChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.TempNotes = Toolbar.Formatter?.Text;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.SearchAccountFonts(sender.Text);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is FontIdentifikationModel selectedItem)
            {
                if (selectedItem.Name != Strings.Resources.AccountCodePageSearchNotFound)
                {
                    ViewModel.TempAccountIconName = selectedItem.Name;
                }
                else
                {
                    ViewModel.TempAccountIconName = string.Empty;
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
        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchAccountFonts(ViewModel.TempAccountIconName);
        }
    }
}
