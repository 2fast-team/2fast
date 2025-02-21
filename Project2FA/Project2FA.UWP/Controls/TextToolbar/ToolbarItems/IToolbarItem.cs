// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace Project2FA.UWP.Controls.TextToolbarButtons
{
    /// <summary>
    /// Interface that defines the position of an item in a <see cref="TextToolbar"/>
    /// </summary>
    public interface IToolbarItem : ICommandBarElement
    {
        /// <summary>
        /// Gets or sets index of this Element
        /// </summary>
        int Position { get; set; }
    }
}