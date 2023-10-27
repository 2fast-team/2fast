// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/6bfe8a5d21feb8818a8048b9fc41a84a70af8b0f/Microsoft.Toolkit.Uwp.UI.Controls.Core/ImageEx

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace Project2FA.Controls
{
    /// <summary>
    /// Base code for ImageEx
    /// </summary>
    public partial class ImageExBase
    {
        /// <summary>
        /// Identifies the <see cref="PlaceholderSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderSourceProperty = DependencyProperty.Register(
            nameof(PlaceholderSource),
            typeof(ImageSource),
            typeof(ImageExBase),
            new PropertyMetadata(default(ImageSource), PlaceholderSourceChanged));

        /// <summary>
        /// Identifies the <see cref="PlaceholderStretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderStretchProperty = DependencyProperty.Register(
            nameof(PlaceholderStretch),
            typeof(Stretch),
            typeof(ImageExBase),
            new PropertyMetadata(default(Stretch)));

        /// <summary>
        /// Gets or sets the placeholder source.
        /// </summary>
        /// <value>
        /// The placeholder source.
        /// </value>
        public ImageSource PlaceholderSource
        {
            get { return (ImageSource)GetValue(PlaceholderSourceProperty); }
            set { SetValue(PlaceholderSourceProperty, value); }
        }

        private static void PlaceholderSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageExBase control)
            {
                control.OnPlaceholderSourceChanged(e);
            }
        }

        /// <summary>
        /// Invoked when Placeholder source has changed
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPlaceholderSourceChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets the placeholder stretch.
        /// </summary>
        /// <value>
        /// The placeholder stretch.
        /// </value>
        public Stretch PlaceholderStretch
        {
            get { return (Stretch)GetValue(PlaceholderStretchProperty); }
            set { SetValue(PlaceholderStretchProperty, value); }
        }
    }
}