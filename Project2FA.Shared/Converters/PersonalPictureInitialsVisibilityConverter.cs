using Project2FA.Repository.Models;
using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    /// <summary>
    /// Called twice because of two PersonPicture controls (binding):/
    /// </summary>
    public partial class PersonalPictureInitialsVisibilityConverter : IValueConverter
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
