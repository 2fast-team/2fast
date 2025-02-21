// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace Project2FA.UWP.Controls.TextToolbarButtons
{
    /// <summary>
    /// Separates a collection of <see cref="IToolbarItem"/>
    /// </summary>
    public partial class ToolbarSeparator : AppBarSeparator, IToolbarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarSeparator"/> class.
        /// </summary>
        public ToolbarSeparator()
        {
            this.DefaultStyleKey = typeof(ToolbarSeparator);
        }

        /// <inheritdoc/>
        public int Position { get; set; } = -1;
    }
}