using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Controls
{
    /// <summary>
    /// Provides attached dependency properties for interacting with the <see cref="ApplicationViewTitleBar"/> on a window (app view).
    /// </summary>
    public static class TitleBar
    {
        /// <summary>
        /// Gets a value indicating whether TitleBar is supported or not.
        /// </summary>
        public static bool IsTitleBarSupported
        {
            get
            {
                return Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewTitleBar");
            }
        }

        private static ApplicationViewTitleBar GetTitleBar()
        {
            return IsTitleBarSupported ? Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar : null;
        }

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.RegisterAttached("ForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnForegroundColorPropertyChanged));

        public static Color GetForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ForegroundColorProperty);
        }

        public static void SetForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ForegroundColorProperty, value);
        }

        private static void OnForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ForegroundColor = color;
            }
        }

        public static readonly DependencyProperty InactiveForegroundColorProperty =
            DependencyProperty.RegisterAttached("InactiveForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnInactiveForegroundColorPropertyChanged));

        public static Color GetInactiveForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(InactiveForegroundColorProperty);
        }

        public static void SetInactiveForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(InactiveForegroundColorProperty, value);
        }

        private static void OnInactiveForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.InactiveForegroundColor = color;
            }
        }


        #region BackgroundColor /active/inactive
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.RegisterAttached("BackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

        public static Color GetBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(BackgroundColorProperty, value);
        }

        private static void OnBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.BackgroundColor = color;
            }
        }

        public static readonly DependencyProperty InactiveBackgroundColorProperty =
            DependencyProperty.RegisterAttached("InactiveBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnInactiveBackgroundColorPropertyChanged));

        public static Color GetInactiveBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(InactiveBackgroundColorProperty);
        }

        public static void SetInactiveBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(InactiveBackgroundColorProperty, value);
        }

        private static void OnInactiveBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.InactiveBackgroundColor = color;
            }
        }
        #endregion

        #region ButtonPressed Foreground/Background
        public static readonly DependencyProperty ButtonPressedBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonPressedBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonPressedBackgroundColorPropertyChanged));

        public static Color GetButtonPressedBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonPressedBackgroundColorProperty);
        }

        public static void SetButtonPressedBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonPressedBackgroundColorProperty, value);
        }

        private static void OnButtonPressedBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonPressedBackgroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonPressedForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonPressedForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonPressedForegroundColorPropertyChanged));

        public static Color GetButtonPressedForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonPressedForegroundColorProperty);
        }

        public static void SetButtonPressedForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonPressedForegroundColorProperty, value);
        }

        private static void OnButtonPressedForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonPressedForegroundColor = color;
            }
        }
        #endregion
        #region ButtonBackgroundColor /active/inactive

        public static readonly DependencyProperty ButtonBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonBackgroundColorPropertyChanged));

        public static Color GetButtonBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonBackgroundColorProperty);
        }

        public static void SetButtonBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonBackgroundColorProperty, value);
        }

        private static void OnButtonBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonBackgroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonInactiveBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonInactiveBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonInactiveBackgroundColorPropertyChanged));

        public static Color GetButtonInactiveBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonInactiveBackgroundColorProperty);
        }

        public static void SetButtonInactiveBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonInactiveBackgroundColorProperty, value);
        }

        private static void OnButtonInactiveBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonInactiveBackgroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonHoverBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonHoverBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonHoverBackgroundColorPropertyChanged));

        public static Color GetButtonHoverBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonHoverBackgroundColorProperty);
        }

        public static void SetButtonHoverBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonHoverBackgroundColorProperty, value);
        }

        private static void OnButtonHoverBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonHoverBackgroundColor = color;
            }
        }
        #endregion
        #region ButtonForegroundColor /active/inactive
        public static readonly DependencyProperty ButtonForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonForegroundColorPropertyChanged));

        public static Color GetButtonForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonForegroundColorProperty);
        }

        public static void SetButtonForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonForegroundColorProperty, value);
        }

        private static void OnButtonForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonForegroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonInactiveForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonInactiveForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonInactiveForegroundColorPropertyChanged));

        public static Color GetButtonInactiveForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonInactiveForegroundColorProperty);
        }

        public static void SetButtonInactiveForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonInactiveForegroundColorProperty, value);
        }

        private static void OnButtonInactiveForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonInactiveForegroundColor = color;
            }
        }

        public static readonly DependencyProperty ButtonHoverForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonHoverForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonHoverForegroundColorPropertyChanged));

        public static Color GetButtonHoverForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonHoverForegroundColorProperty);
        }

        public static void SetButtonHoverForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonHoverForegroundColorProperty, value);
        }

        private static void OnButtonHoverForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleBar = GetTitleBar();
            if (titleBar != null)
            {
                var color = (Color)e.NewValue;
                titleBar.ButtonHoverForegroundColor = color;
            }
        }
        #endregion
    }
}
