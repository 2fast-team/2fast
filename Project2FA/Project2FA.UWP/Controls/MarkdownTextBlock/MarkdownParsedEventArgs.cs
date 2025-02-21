// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax;
using System;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock
{
    public sealed class MarkdownParsedEventArgs : EventArgs
    {
        public MarkdownDocument Document { get; }
        public MarkdownParsedEventArgs(MarkdownDocument document)
        {
            Document = document;
        }
    }
}
