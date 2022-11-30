using Project2FA.Repository.Models;

namespace Project2FA.MAUI.Helpers
{
    public class AccountAvatarTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Normal { get; set; }
        public DataTemplate Accent { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
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
