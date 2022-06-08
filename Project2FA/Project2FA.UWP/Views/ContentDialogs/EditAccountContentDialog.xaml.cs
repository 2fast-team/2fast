using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarButtons;
using Project2FA.Repository.Models;
using Project2FA.UWP.Extensions;
using Project2FA.UWP.Services;
using Project2FA.UWP.Services.Enums;
using Project2FA.UWP.ViewModels;
using System.Collections.Generic;
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

        private void HLBTN_PasswordInfo(object sender, RoutedEventArgs e)
        {

        }

        private void REB_Notes_TextChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.TempNotes = Toolbar.Formatter?.Text;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrEmpty(sender.Text) == false && sender.Text.Length >= 2)
                {
                    List<string> _nameList = new List<string>();
                    foreach (IconNameModel item in ViewModel.IconNameCollectionModel.Collection)
                    {
                        _nameList.Add(item.Name);
                    }
                    List<string> listSuggestion = _nameList.Where(x => x.Contains(sender.Text, System.StringComparison.OrdinalIgnoreCase)).ToList();
                    if (listSuggestion.Count == 0)
                    {
                        listSuggestion.Add(Strings.Resources.AccountCodePageSearchNotFound);
                    }
                    sender.ItemsSource = listSuggestion;
                }
                else
                {
                    sender.ItemsSource = null;
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string selectedItem = args.SelectedItem.ToString();
            if (selectedItem != Strings.Resources.AccountCodePageSearchNotFound)
            {
                ViewModel.TempAccountIconName = selectedItem;
                ViewModel.LoadIconSVG();
            }
            else
            {
                sender.Text = string.Empty;
            }
        }
    }
}
