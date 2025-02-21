// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;

#if !WINAPPSDK
using FontWeight = Windows.UI.Text.FontWeight;
using FontWeights = Windows.UI.Text.FontWeights;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;
#else
using FontWeight = Windows.UI.Text.FontWeight;
using FontWeights = Microsoft.UI.Text.FontWeights;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
#endif

namespace CommunityToolkit.WinUI.Controls.MarkdownTextBlockRns
{
    public sealed class MarkdownThemes : DependencyObject
    {
        internal static MarkdownThemes Default { get; } = new MarkdownThemes();

        public Thickness Padding { get; set; } = new Thickness(8);

        public Thickness InternalMargin { get; set; } = new Thickness(4);

        public CornerRadius CornerRadius { get; set; } = new CornerRadius(4);

        public double H1FontSize { get; set; } = 22;

        public double H2FontSize { get; set; } = 20;

        public double H3FontSize { get; set; } = 18;

        public double H4FontSize { get; set; } = 16;

        public double H5FontSize { get; set; } = 14;

        public double H6FontSize { get; set; } = 12;

        //public SolidColorBrush HeadingForeground { get; set; } = Extensions.GetAccentColorBrush();

        public FontWeight H1FontWeight { get; set; } = FontWeights.Bold;

        public FontWeight H2FontWeight { get; set; } = FontWeights.Normal;

        public FontWeight H3FontWeight { get; set; } = FontWeights.Normal;

        public FontWeight H4FontWeight { get; set;} = FontWeights.Normal;

        public FontWeight H5FontWeight { get; set; } = FontWeights.Normal;

        public FontWeight H6FontWeight { get; set; } = FontWeights.Normal;

        public Thickness H1Margin { get; set; } = new Thickness(left: 0, top: 14, right: 0, bottom: 0);
        public Thickness H2Margin { get; set; } = new Thickness(left: 0, top: 14, right: 0, bottom: 0);
        public Thickness H3Margin { get; set; } = new Thickness(left: 0, top: 14, right: 0, bottom: 0);
        public Thickness H4Margin { get; set; } = new Thickness(left: 0, top: 14, right: 0, bottom: 0);
        public Thickness H5Margin { get; set; } = new Thickness(left: 0, top: 8, right: 0, bottom: 0);
        public Thickness H6Margin { get; set; } = new Thickness(left: 0, top: 8, right: 0, bottom: 0);

        //public Brush InlineCodeBackground { get; set; } = (Brush)Application.Current.Resources["ExpanderHeaderBackground"];
        //public Brush InlineCodeForeground { get; set; } = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];

        public SolidColorBrush InlineCodeBorderBrush { get; set; } = new SolidColorBrush(Colors.Gray);

        public Thickness InlineCodeBorderThickness { get; set; } = new Thickness(1);

        public CornerRadius InlineCodeCornerRadius { get; set; } = new CornerRadius(2);

        public Thickness InlineCodePadding { get; set; } = new Thickness(0);

        public double InlineCodeFontSize { get; set; } = 16;

        public FontWeight InlineCodeFontWeight { get; set; } = FontWeights.Normal;
    }
}
