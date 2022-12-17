using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Controls
{
    public class AutoCloseInfoBar : Microsoft.UI.Xaml.Controls.InfoBar
    {
        private DispatcherTimer _timer;
        private long _token;

        public AutoCloseInfoBar()
        {
            this.Loaded += AutoCloseInfoBar_Loaded;
            this.Unloaded += AutoCloseInfoBar_Unloaded;
        }

        /// <summary>
        /// Gets or sets the auto-close interval, in milliseconds.
        /// </summary>

        public int AutoCloseInterval
        {
            get => (int)GetValue(AutoCloseIntervalProperty);
            set => SetValue(AutoCloseIntervalProperty, value);
        }

        public static readonly DependencyProperty AutoCloseIntervalProperty =
            DependencyProperty.Register(
            nameof(AutoCloseInterval),
            typeof(int),
            typeof(AutoCloseInfoBar),
            new PropertyMetadata(5000));

        private void AutoCloseInfoBar_Loaded(object sender, RoutedEventArgs e)
        {
            _token = this.RegisterPropertyChangedCallback(IsOpenProperty, IsOpenChanged);
            if (IsOpen)
            {
                Open();
            }
        }

        private void AutoCloseInfoBar_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnregisterPropertyChangedCallback(IsOpenProperty, _token);
        }

        private void IsOpenChanged(DependencyObject o, DependencyProperty p)
        {
            if (!(o is AutoCloseInfoBar control))
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
