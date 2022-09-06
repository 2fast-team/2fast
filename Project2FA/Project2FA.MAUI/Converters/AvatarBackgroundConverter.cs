using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Converters
{
    public class AvatarBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                //ontrol.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));
                //var conv = new System.Drawing.ColorConverter();
                //var color = (System.Drawing.Color)conv.ConvertFromString("#FF009BC1");
                return new Color(0,155,193);
            }
            else
            {
                return Colors.YellowGreen;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
