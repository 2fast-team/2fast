using System;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Controls
{
    /// <summary>
    /// A teaching tip that closes itself after an interval.
    /// </summary>
    public class AutoCloseTeachingTip : Microsoft.UI.Xaml.Controls.TeachingTip
    {
        private DispatcherTimer _timer;
        private long _token;

        public AutoCloseTeachingTip() : base()
        {
            this.Loaded += AutoCloseTeachingTip_Loaded;
            this.Unloaded += AutoCloseTeachingTip_Unloaded;
        }

        /// <summary>
        /// Gets or sets the auto-close interval, in milliseconds.
        /// </summary>

        public int AutoCloseInterval
        {
            get { return (int)GetValue(AutoCloseIntervalProperty); }
            set { SetValue(AutoCloseIntervalProperty, value); }
        }

        public static readonly DependencyProperty AutoCloseIntervalProperty =
            DependencyProperty.Register(
            nameof(AutoCloseInterval),
            typeof(int),
            typeof(AutoCloseTeachingTip),
            new PropertyMetadata(5000));

        private void AutoCloseTeachingTip_Loaded(object sender, RoutedEventArgs e)
        {
            _token = this.RegisterPropertyChangedCallback(IsOpenProperty, IsOpenChanged);
            if (IsOpen)
            {
                Open();
            }
        }

        private void AutoCloseTeachingTip_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnregisterPropertyChangedCallback(IsOpenProperty, _token);
        }

        private void IsOpenChanged(DependencyObject o, DependencyProperty p)
        {
            if (!(o is AutoCloseTeachingTip control))
            {
                return;
            }

            if (p != IsOpenProperty)
            {
                return;
            }

            if (control.IsOpen)
            {
                control.Open();
            }
            else
            {
                control.Close();
            }
        }

        private void Open()
        {
            _timer = new DispatcherTimer();
            _timer.Tick -= Timer_Tick;
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(AutoCloseInterval);
            _timer.Start();
        }

        private void Close()
        {
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            IsOpen = false;
        }
    }
}
