using Microsoft.UI.Xaml.Data;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.UNO.Converters
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
