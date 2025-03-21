// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax.Inlines;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements;
using System;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.Renderers.ObjectRenderers.Inlines
{
    internal class LinkInlineRenderer : UWPObjectRenderer<LinkInline>
    {
        protected override void Write(WinUIRenderer renderer, LinkInline link)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (link == null) throw new ArgumentNullException(nameof(link));

            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                url = "#";
            }

            if (link.IsImage)
            {
                var image = new MyImage(link, CommunityToolkit.Labs.WinUI.MarkdownTextBlock.Extensions.GetUri(url, renderer.Config.BaseUrl), renderer.Config);
                renderer.WriteInline(image);
            }
            else
            {
                if (link.FirstChild is LinkInline linkInlineChild && linkInlineChild.IsImage)
                {
                    var myHyperlinkButton = new MyHyperlinkButton(link, renderer.Config.BaseUrl);
                    //myHyperlinkButton.ClickEvent += (sender, e) =>
                    //{
                    //    renderer.MarkdownTextBlock.RaiseLinkClickedEvent(((HyperlinkButton)sender).NavigateUri);
                    //};
                    renderer.Push(myHyperlinkButton);
                }
                else
                {
                    var hyperlink = new MyHyperlink(link, renderer.Config.BaseUrl);
                    //hyperlink.ClickEvent += (sender, e) =>
                    //{
                    //    renderer.MarkdownTextBlock.RaiseLinkClickedEvent(sender.NavigateUri);
                    //};

                    renderer.Push(hyperlink);
                }

                renderer.WriteChildren(link);
                renderer.Pop();
            }
        }
    }
}
