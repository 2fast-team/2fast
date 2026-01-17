// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HtmlAgilityPack;
using Markdig.Syntax.Inlines;
using Windows.Foundation;
using Windows.UI.Xaml.Documents;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif
#if !WINAPPSDK
using Block = Windows.UI.Xaml.Documents.Block;
using Inline = Windows.UI.Xaml.Documents.Inline;
#else
using Block = Microsoft.UI.Xaml.Documents.Block;
using Inline = Microsoft.UI.Xaml.Documents.Inline;
#endif

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyHyperlink : IAddChild
    {
        private Hyperlink _hyperlink;
        private LinkInline? _linkInline;
        private HtmlNode? _htmlNode;
        private string? _baseUrl;

        public event TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> ClickEvent
        {
            add
            {
                _hyperlink.Click += value;
            }
            remove
            {
                _hyperlink.Click -= value;
            }
        }

        public bool IsHtml => _htmlNode != null;

        public TextElement TextElement
        {
            get => _hyperlink;
        }

        public MyHyperlink(LinkInline linkInline, string? baseUrl)
        {
            _baseUrl = baseUrl;
            var url = linkInline.GetDynamicUrl != null ? linkInline.GetDynamicUrl() ?? linkInline.Url : linkInline.Url;
            _linkInline = linkInline;
            _hyperlink = new Hyperlink()
            {
                NavigateUri = Extensions.GetUri(url, baseUrl),
            };
        }

        public MyHyperlink(HtmlNode htmlNode, string? baseUrl)
        {
            _baseUrl = baseUrl;
            var url = htmlNode.GetAttribute("href", "#");
            _htmlNode = htmlNode;
            _hyperlink = new Hyperlink()
            {
                NavigateUri = Extensions.GetUri(url, baseUrl),
            };
        }

#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Inline))]
#endif
        public void AddChild(IAddChild child)
        {
#if !WINAPPSDK
            if (child.TextElement is Inline inlineChild)
            {
                try
                {
                    _hyperlink.Inlines.Add(inlineChild);
                    // TODO: Add support for click handler
                }
                catch { }
            }
#else
            if (child.TextElement is Microsoft.UI.Xaml.Documents.Inline inlineChild)
            {
                try
                {
                    _hyperlink.Inlines.Add(inlineChild);
                    // TODO: Add support for click handler
                }
                catch { }
            }
#endif
        }
    }
}
