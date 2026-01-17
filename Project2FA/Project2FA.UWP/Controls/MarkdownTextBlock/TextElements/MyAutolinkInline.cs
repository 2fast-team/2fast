// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax.Inlines;
using System;
using Windows.Foundation;
using Windows.UI.Xaml.Documents;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyAutolinkInline : IAddChild
    {
        private AutolinkInline _autoLinkInline;

        public TextElement TextElement { get; private set; }

        public event TypedEventHandler<Hyperlink, HyperlinkClickEventArgs>? ClickEvent
        {
#if WINDOWS_UWP && NET10_0_OR_GREATER
            [DynamicWindowsRuntimeCast(typeof(Hyperlink))]
#endif
            add
            {
                ((Hyperlink)TextElement).Click += value;
            }
#if WINDOWS_UWP && NET10_0_OR_GREATER
            [DynamicWindowsRuntimeCast(typeof(Hyperlink))]
#endif
            remove
            {
                ((Hyperlink)TextElement).Click -= value;
            }
        }

        public MyAutolinkInline(AutolinkInline autoLinkInline)
        {
            _autoLinkInline = autoLinkInline;
            TextElement = new Hyperlink()
            {
                NavigateUri = new Uri(autoLinkInline.Url),
            };
        }
#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Hyperlink))]
#endif
        public void AddChild(IAddChild child)
        {
            try
            {
                var text = (MyInlineText)child;
                ((Hyperlink)TextElement).Inlines.Add((Run)text.TextElement);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding child to MyAutolinkInline", ex);
            }
        }
    }
}
