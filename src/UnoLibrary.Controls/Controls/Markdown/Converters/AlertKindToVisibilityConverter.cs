using Microsoft.UI.Xaml.Data;
using Symptum.UI.Markdown;

namespace Symptum.UI.Converters;

public class AlertKindToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is AlertKind kind
            ? kind switch
            { AlertKind.None => Visibility.Collapsed, _ => Visibility.Visible }
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
