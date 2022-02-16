#if HAS_WINUI
using Microsoft.UI.Xaml;
using win = Microsoft.ApplicationModel;
#else
using Windows.UI.Xaml;
using win = Windows.ApplicationModel;
#endif


namespace Prism.Mvvm
{
    public class ViewModelLocator
    {
        /// <summary>
        /// Gets the AutowireViewModel property value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool? GetAutowireViewModel(DependencyObject obj)
        {
            return (bool?)obj.GetValue(AutowireViewModelProperty);
        }

        /// <summary>
        /// Sets the AutowireViewModel property value.  If <c>true</c>, creates an instance of a ViewModel using a convention, and sets the associated View's to that instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetAutowireViewModel(DependencyObject obj, bool? value)
        {
            obj.SetValue(AutowireViewModelProperty, value);
        }
        public static readonly DependencyProperty AutowireViewModelProperty =
            DependencyProperty.RegisterAttached("AutowireViewModel", typeof(bool?),
                typeof(ViewModelLocator), new PropertyMetadata(null, OnAutowireViewModelChanged));

        private static void OnAutowireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!win.DesignMode.DesignModeEnabled)
            {
                var value = (bool?)e.NewValue;
                if (value.HasValue && value.Value)
                {
                    ViewModelLocationProvider.AutoWireViewModelChanged(d, Bind);
                }
            }
        }


        /// <summary>
        /// Sets the  of a View
        /// </summary>
        /// <param name="view">The View to set the on</param>
        /// <param name="viewModel">The object to use as the for the View</param>
        static void Bind(object view, object viewModel)
        {
            if (view is FrameworkElement element)
                element.DataContext = viewModel;
        }
    }
}
