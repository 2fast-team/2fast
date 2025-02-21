using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class TutorialPage : Page
    {
        public TutorialPageViewModel ViewModel => DataContext as TutorialPageViewModel;
        public TutorialPage()
        {
            this.InitializeComponent();
            this.Loaded += TutorialPage_Loaded;
        }

        private void TutorialPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = string.Empty;
            App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
        }

        /// <summary>
        /// Create the TeachingTip for the specific framework element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task CreateTeachingTip(FrameworkElement element,string title, string content)
        {
            var control = MainGrid.FindDescendant(nameof(TeachingTip));
            if (control != null)
            {
                var tooltip = (control as TeachingTip);
                if (tooltip.IsOpen)
                {
                    tooltip.IsOpen = false;
                    await Task.Delay(500);
                }
                TextBlock txt = new TextBlock { Text = content, TextWrapping = TextWrapping.WrapWholeWords };
                tooltip.Title = title;
                tooltip.Content = txt;
                tooltip.Target = element;
                tooltip.IsOpen = true;

            }
            else
            {
                TextBlock txt = new TextBlock { Text = content, TextWrapping = TextWrapping.WrapWholeWords };
                TeachingTip teachingTip = new TeachingTip
                {
                    Target = element,
                    Name = nameof(TeachingTip),
                    Content = txt,
                    IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource { Symbol = Symbol.Help },
                    BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                    IsOpen = true,
                };
                MainGrid.Children.Add(teachingTip);
            }
        }

        private void BTN_SetFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, Strings.Resources.TutorialPageItemFavouriteBTNTitle, Strings.Resources.TutorialPageItemFavouriteBTNTDesc).ConfigureAwait(false);
            }
        }

        private void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, Strings.Resources.TutorialPageItemCopyCodeBTNTitle, Strings.Resources.TutorialPageItemCopyCodeBTNDesc).ConfigureAwait(false);
            }
        }

        private void BTN_ShowCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, Strings.Resources.TutorialPageItemShowCodeBTNTitle, Strings.Resources.TutorialPageItemShowCodeBTNDesc).ConfigureAwait(false);
            }
        }

        private void TutorialPageItemMoreBTN_Click(object sender, RoutedEventArgs e)
        {
            if (sender as FrameworkElement != null)
            {
                CreateTeachingTip(sender as FrameworkElement, Strings.Resources.TutorialPageItemMoreBTNTitle, Strings.Resources.TutorialPageItemMoreBTNDesc).ConfigureAwait(false);
            }
        }

        private void FV_Tutorials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && ViewModel.SelectedIndex == 4)
            {
                var control = MainGrid.FindDescendant(nameof(TeachingTip));
                if (control != null)
                {
                    var tooltip = (control as TeachingTip);
                    if (tooltip.IsOpen)
                    {
                        tooltip.IsOpen = false;
                    }
                }
            }
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Uri.ToString(), UriKind.Absolute, out Uri link))
            {
                await Launcher.LaunchUriAsync(link);
            }

        }

        private void HLBTN_PasswordInfo(object sender, RoutedEventArgs e)
        {
            var markdownText = new MarkdownTextBlock();
            markdownText.Margin = new Thickness(8,8,8,8);
            markdownText.Text = Strings.Resources.TutorialPagePasswordInfo;
            markdownText.OnLinkClicked += MarkdownTextBlock_LinkClicked;
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = sender as FrameworkElement,
                HeroContent = markdownText,
                AutoCloseInterval = 8000,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            MainGrid.Children.Add(teachingTip);
        }
    }
}
