using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadialProgressBar : ProgressBar
    {
        private const string OutlineFigurePartName = "OutlineFigurePart";
        private const string OutlineArcPartName = "OutlineArcPart";
        private const string BarFigurePartName = "BarFigurePart";
        private const string BarArcPartName = "BarArcPart";

        private PathFigure outlineFigure;
        private PathFigure barFigure;
        private ArcSegment outlineArc;
        private ArcSegment barArc;

        private bool allTemplatePartsDefined = false;

        /// <summary>
        /// Called when the Minimum property changes.
        /// </summary>
        /// <param name="oldMinimum">Old value of the Minimum property.</param>
        /// <param name="newMinimum">New value of the Minimum property.</param>
        protected static void OnMinimumChanged(BindableObject bindable, double oldMinimum, double newMinimum)
        {
            var control = (RadialProgressBar)bindable;
            base.OnMinimumChanged(oldMinimum, newMinimum);
            RenderSegment();
        }

        /// <summary>
        /// Called when the Maximum property changes.
        /// </summary>
        /// <param name="oldMaximum">Old value of the Maximum property.</param>
        /// <param name="newMaximum">New value of the Maximum property.</param>
        protected static void OnMaximumChanged(BindableObject bindable, double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            RenderSegment();
        }

        /// <summary>
        /// Called when the Value property changes.
        /// </summary>
        /// <param name="oldValue">Old value of the Value property.</param>
        /// <param name="newValue">New value of the Value property.</param>
        protected override void OnValueChanged(BindableObject bindable, double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            RenderSegment();
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        public RadialProgressBar()
        {
            outlineFigure = GetTemplateChild(OutlineFigurePartName) as PathFigure;
            outlineArc = GetTemplateChild(OutlineArcPartName) as ArcSegment;
            barFigure = GetTemplateChild(BarFigurePartName) as PathFigure;
            barArc = GetTemplateChild(BarArcPartName) as ArcSegment;

            allTemplatePartsDefined = outlineFigure != null && outlineArc != null && barFigure != null && barArc != null;

            RenderAll();
        }

        /// <summary>
        /// Gets or sets the thickness of the circular outline and segment
        /// </summary>
        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the Thickness dependency property
        /// </summary>
        public static readonly BindableProperty ThicknessProperty =
            BindableProperty.Create(nameof(Thickness), typeof(double), typeof(RadialProgressBar), 0.0, propertyChanged: ThicknessChangedHandler));

        public static readonly BindableProperty MaxValueProperty =
            BindableProperty.Create("MaxValue", typeof(int), typeof(RadialProgressBar), 100, propertyChanged: OnMaxValueChanged, validateValue: ValidateMaxVal);


        public static readonly BindableProperty MinValueProperty =
            BindableProperty.Create("MinValue", typeof(int), typeof(RadialProgressBar), 0, propertyChanged: OnMinValueChanged, validateValue: ValidateMinVal);

        /// <summary>
        /// Gets or sets the color of the circular outline on which the segment is drawn
        /// </summary>
        public Brush Outline
        {
            get { return (Brush)GetValue(OutlineProperty); }
            set { SetValue(OutlineProperty, value); }
        }

        /// <summary>
        /// Identifies the Outline dependency property
        /// </summary>
        public static readonly BindableProperty OutlineProperty =
            BindableProperty.Create(nameof(Outline), typeof(Brush), typeof(RadialProgressBar), new SolidColorBrush());

        /// <summary>
        /// Initializes a new instance of the <see cref="RadialProgressBar"/> class.
        /// Create a default circular progress bar
        /// </summary>
        public RadialProgressBar()
        {
            DefaultStyleKey = typeof(RadialProgressBar);
            SizeChanged += SizeChangedHandler;
        }

        // Render outline and progress segment when thickness is changed
        private static void ThicknessChangedHandler(BindableObject bindable, DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as RadialProgressBar;
            sender.RenderAll();
        }

        // Render outline and progress segment when control is resized.
        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            var self = sender as RadialProgressBar;
            self.RenderAll();
        }

        private double ComputeNormalizedRange()
        {
            var range = Maximum - Minimum;
            var delta = Value - Minimum;
            var output = range == 0.0 ? 0.0 : delta / range;
            output = Math.Min(Math.Max(0.0, output), 0.9999);
            return output;
        }

        // Compute size of ellipse so that the outer edge touches the bounding rectangle
        private Size ComputeEllipseSize()
        {
            var safeThickness = Math.Max(Thickness, 0.0);
            var width = Math.Max((ActualWidth - safeThickness) / 2.0, 0.0);
            var height = Math.Max((ActualHeight - safeThickness) / 2.0, 0.0);
            return new Size(width, height);
        }

        // Render the segment representing progress ratio.
        private static void RenderSegment()
        {
            if (!allTemplatePartsDefined)
            {
                return;
            }

            var normalizedRange = ComputeNormalizedRange();

            var angle = 2 * Math.PI * normalizedRange;
            var size = ComputeEllipseSize();
            var translationFactor = Math.Max(Thickness / 2.0, 0.0);

            double x = Math.Sin(angle) * size.Width + size.Width + translationFactor;
            double y = (Math.Cos(angle) * size.Height - size.Height) * -1 + translationFactor;

            barArc.IsLargeArc = angle >= Math.PI;
            barArc.Point = new Point(x, y);
        }

        // Render the progress segment and the loop outline. Needs to run when control is resized or retemplated
        private void RenderAll()
        {
            if (!allTemplatePartsDefined)
            {
                return;
            }

            var size = ComputeEllipseSize();
            var segmentWidth = size.Width;
            var translationFactor = Math.Max(Thickness / 2.0, 0.0);

            outlineFigure.StartPoint = barFigure.StartPoint = new Point(segmentWidth + translationFactor, translationFactor);
            outlineArc.Size = barArc.Size = new Size(segmentWidth, size.Height);
            outlineArc.Point = new Point(segmentWidth + translationFactor - 0.05, translationFactor);

            RenderSegment();
        }
    }
}
