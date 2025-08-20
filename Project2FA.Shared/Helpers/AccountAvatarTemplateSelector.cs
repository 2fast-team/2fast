using Project2FA.Repository.Models;
#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#else
using Project2FA.Uno;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#endif

namespace Project2FA.Helpers
{
    public partial class AccountAvatarTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Normal { get; set; }
        public DataTemplate Accent { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TwoFACodeModel model)
            {
                if (model.IsFavourite)
                {
                    return Accent;
                }
                else
                {
                    return Normal;
                }
            }
            else
            {
                return Normal;
            }
        }
    }
}
