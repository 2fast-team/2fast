using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Project2FA.Helpers
{
    public partial class TemplateSelector : DependencyObject
    {
        public static object GetChangedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(ChangedItemProperty);
        }

        public static void SetChangedItem(DependencyObject obj, object value)
        {
            obj.SetValue(ChangedItemProperty, value);
        }

        public static readonly DependencyProperty ChangedItemProperty =
            DependencyProperty.RegisterAttached("ChangedItem", typeof(object), typeof(TemplateSelector), new PropertyMetadata(default(object), OnChangedItemChanged));

        private static void OnChangedItemChanged(DependencyObject attachingElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(attachingElement is ItemsControl itemsControl))
            {
                throw new ArgumentException($"Attaching element must be of type '{nameof(ItemsControl)}'");
            }

            var container = (itemsControl.ContainerFromItem(e.NewValue) as ContentControl);
            if (container != null)
            {
                var containerContent = container.Content;
                container.Content = null;
                container.Content = containerContent; // Trigger the DataTemplateSelector
            }
        }
    }
}
