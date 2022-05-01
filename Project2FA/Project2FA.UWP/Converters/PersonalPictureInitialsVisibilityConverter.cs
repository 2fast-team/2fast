using Project2FA.Repository.Models;
using System;
using Windows.UI.Xaml.Data;

namespace Project2FA.UWP.Converters
{
    /// <summary>
    /// Called twice because of two PersonPicture controls (binding):/
    /// </summary>
    public class PersonalPictureInitialsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TwoFACodeModel model)
            {
                return string.IsNullOrEmpty(model.AccountIconName) ? model.Label : string.Empty;
            }

            return "NaO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
