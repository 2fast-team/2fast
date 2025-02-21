// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock
{
    partial class MarkdownTextBlock
    {
        /// <summary>
        /// Event raised when a markdown link is clicked.
        /// </summary>
        public event EventHandler<LinkClickedEventArgs>? OnLinkClicked;

        /// <summary>
        /// Event raised when markdown is done parsing, with a complete MarkdownDocument.
        /// It is always raised before the control renders the document.
        /// </summary>
        public event EventHandler<MarkdownParsedEventArgs>? OnMarkdownParsed;
    }
}
