using Microsoft.UI.Xaml.Controls;
using Project2FA.ViewModels;

namespace Project2FA.Uno.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage : Page
    {
        public BlankPageViewModel ViewModel => DataContext as BlankPageViewModel;
        public BlankPage()
        {
            this.InitializeComponent();
            // Refresh x:Bind when the DataContext changes.
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
