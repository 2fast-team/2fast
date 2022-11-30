using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project2FA.MAUI.Converters
{
    public class NameInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!string.IsNullOrEmpty(value.ToString()))
            {
                string name = value.ToString();
                // StringSplitOptions.RemoveEmptyEntries excludes empty spaces returned by the Split method

                string[] nameSplit = name.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

                string initials = "";

                foreach (string item in nameSplit)
                {
                    string firstLetter = item.Substring(0, 1).ToUpper();
                    if (Char.IsLetter(firstLetter,0))
                    {
                        initials += item.Substring(0, 1).ToUpper();
                    }
                }

                return initials;
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
