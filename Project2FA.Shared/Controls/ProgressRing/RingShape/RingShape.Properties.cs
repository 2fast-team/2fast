// Copyright (c) Files Community
// Licensed under the MIT License.

// based on https://github.com/files-community/Files/blob/528be10e78a1c4dc7d242e5a84b3330a604c5f5f/src/Files.App.Controls/Storage/RingShape/RingShape.Properties.cs

using Windows.Foundation;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Path = Windows.UI.Xaml.Shapes.Path;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Path = Microsoft.UI.Xaml.Shapes.Path;
#endif

namespace Project2FA.Controls
{
    public partial class RingShape : Path
    {
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(
                nameof(StartAngle),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(0.0d, OnStartAngleChanged));

        public double StartAngle 
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(
                nameof(EndAngle),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(90.0d, OnEndAngleChanged));

        public double EndAngle 
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public static readonly DependencyProperty SweepDirectionProperty =
            DependencyProperty.Register(
                nameof(SweepDirection),
                typeof(SweepDirection),
                typeof(RingShape),
                new PropertyMetadata(SweepDirection.Clockwise, OnSweepDirectionChanged));

        public SweepDirection SweepDirection 
        {
            get { return (SweepDirection)GetValue(SweepDirectionProperty); }
            set { SetValue(SweepDirectionProperty, value); }
        }

        public static readonly DependencyProperty MinAngleProperty =
            DependencyProperty.Register(
                nameof(MinAngle),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(0.0d, OnMinAngleChanged));

        public double MinAngle 
        {
            get { return (double)GetValue(MinAngleProperty); }
            set { SetValue(MinAngleProperty, value); }
        }

        public static readonly DependencyProperty MaxAngleProperty =
            DependencyProperty.Register(
                nameof(MaxAngle),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(360.0d, OnMaxAngleChanged));

        public double MaxAngle 
        {
            get { return (double)GetValue(MaxAngleProperty); }
            set { SetValue(MaxAngleProperty, value); }
        }

        public static readonly DependencyProperty RadiusWidthProperty =
            DependencyProperty.Register(
                nameof(RadiusWidth),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(0.0d, OnRadiusWidthChanged));

        public double RadiusWidth 
        {
            get { return (double)GetValue(RadiusWidthProperty); }
            set { SetValue(RadiusWidthProperty, value); }
        }

        public static readonly DependencyProperty RadiusHeightProperty =
        DependencyProperty.Register(
            nameof(RadiusHeight),
            typeof(double),
            typeof(RingShape),
            new PropertyMetadata(0.0d, OnRadiusHeightChanged));

        public double RadiusHeight 
        {
            get { return (double)GetValue(RadiusHeightProperty); }
            set { SetValue(RadiusHeightProperty, value); }
        }

        public static readonly DependencyProperty IsCircleProperty =
            DependencyProperty.Register(
                nameof(IsCircle),
                typeof(bool),
                typeof(RingShape),
                new PropertyMetadata(false, OnIsCircleChanged));

        public bool IsCircle 
        {
            get { return (bool)GetValue(IsCircleProperty); }
            set { SetValue(IsCircleProperty, value); }
        }

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(
                nameof(Center),
                typeof(Point),
                typeof(RingShape),
                new PropertyMetadata(default(Point)));

        public Point Center 
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public static readonly DependencyProperty ActualRadiusWidthProperty =
            DependencyProperty.Register(
                nameof(ActualRadiusWidth),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(0.0d));

        public double ActualRadiusWidth 
        {
            get { return (double)GetValue(ActualRadiusWidthProperty); }
            set { SetValue(ActualRadiusWidthProperty, value); }
        }

        public static readonly DependencyProperty AActualRadiusHeightProperty =
            DependencyProperty.Register(
                nameof(ActualRadiusHeight),
                typeof(double),
                typeof(RingShape),
                new PropertyMetadata(0.0d));
        public double ActualRadiusHeight 
        {
            get { return (double)GetValue(AActualRadiusHeightProperty); }
            set { SetValue(AActualRadiusHeightProperty, value); }
        }


        private static void OnStartAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.StartAngleChanged();
            }
        }

        private static void OnEndAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.EndAngleChanged();
            }
        }

        private static void OnSweepDirectionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.SweepDirectionChanged();
            }
        }

        private static void OnMinAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.MinMaxAngleChanged(false);
            }
        }

        private static void OnMaxAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.MinMaxAngleChanged(true);
            }
        }

        private static void OnRadiusWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.RadiusWidthChanged();
            }
        }

        private static void OnRadiusHeightChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.RadiusHeightChanged();
            }
        }

        private static void OnIsCircleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is RingShape ringShape)
            {
                ringShape.IsCircleChanged();
            }
        }
    }
}
