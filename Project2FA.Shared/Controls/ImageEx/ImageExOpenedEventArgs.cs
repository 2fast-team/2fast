// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/6bfe8a5d21feb8818a8048b9fc41a84a70af8b0f/Microsoft.Toolkit.Uwp.UI.Controls.Core/ImageEx

using System;

namespace Project2FA.Controls
{
    /// <summary>
    /// A delegate for <see cref="ImageEx"/> opened.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ImageExOpenedEventHandler(object sender, ImageExOpenedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="ImageEx"/> ImageOpened event.
    /// </summary>
    public class ImageExOpenedEventArgs : EventArgs
    {
    }
}