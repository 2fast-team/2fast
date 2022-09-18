using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Project2FA.Repository.Models;

namespace Project2FA.UWP.Helpers
{
    public class AccountAvatarTemplateSelector : DataTemplateSelector
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
