using Microsoft.Xaml.Interactivity;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Behaviors
{
    public class HighlightFormFieldOnErrors : Behavior<FrameworkElement>
    {
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }
        public List<(string name, string message)> PropertyErrors
        {
            get { return (List<(string name, string message)>)GetValue(PropertyErrorsProperty); }
            set { SetValue(PropertyErrorsProperty, value); }
        }

        public string HighlightStyleName
        {
            get { return (string)GetValue(HighlightStyleNameProperty); }
            set { SetValue(HighlightStyleNameProperty, value); }
        }

        public string OriginalStyleName
        {
            get { return (string)GetValue(OriginalStyleNameProperty); }
            set { SetValue(OriginalStyleNameProperty, value); }
        }

        public static DependencyProperty PropertyNameProperty =
        DependencyProperty.RegisterAttached(nameof(PropertyName), typeof(string), typeof(HighlightFormFieldOnErrors), new PropertyMetadata(string.Empty));

        public static DependencyProperty PropertyErrorsProperty =
        DependencyProperty.RegisterAttached(nameof(PropertyErrors), typeof(List<(string name, string message)>), typeof(HighlightFormFieldOnErrors), new PropertyMetadata(new List<(string name, string message)>(), OnPropertyErrorsChanged));

        // The default for this property only applies to TextBox controls.
        public static DependencyProperty HighlightStyleNameProperty =
            DependencyProperty.RegisterAttached(nameof(HighlightStyleName), typeof(string), typeof(HighlightFormFieldOnErrors), new PropertyMetadata("HighlightTextBoxStyle"));

        // The default for this property only applies to TextBox controls.
        protected static DependencyProperty OriginalStyleNameProperty =
            DependencyProperty.RegisterAttached(nameof(OriginalStyleName), typeof(Style), typeof(HighlightFormFieldOnErrors), new PropertyMetadata("DefaultTextBoxStyle"));

        private static void OnPropertyErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (args == null || args.NewValue == null)
            {
                return;
            }

            var behavior = (HighlightFormFieldOnErrors)d;
            var control = ((Behavior<FrameworkElement>)d).AssociatedObject;
            var propertyErrors = (List<(string name, string message)>)args.NewValue;
            if (propertyErrors.Any())
            {
                var thisError = propertyErrors.Where(x => x.name == behavior.PropertyName).ToList();
                Style style = thisError.Any() ?
                (Style)Application.Current.Resources[behavior.HighlightStyleName] :
                ((Style)Application.Current.Resources[behavior.OriginalStyleName]);

                control.Style = style;
            }
            else
            {
                Style style = (Style)Application.Current.Resources[behavior.OriginalStyleName];
                control.Style = style;
            }

        }

        protected override void OnAttached()
        {
        }
    }
}
