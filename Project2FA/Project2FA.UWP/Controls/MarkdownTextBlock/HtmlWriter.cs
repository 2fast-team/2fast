// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HtmlAgilityPack;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock.Renderers;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements.Html;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock
{
    internal class HtmlWriter
    {
        public static void WriteHtml(WinUIRenderer renderer, HtmlNodeCollection nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes)
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    renderer.WriteText(node.InnerText);
                }
                else if (node.NodeType == HtmlNodeType.Element && node.Name.TagToType() == TextElements.HtmlElementType.Inline)
                {
                    // detect br here
                    var inlineTagName = node.Name.ToLower();
                    if (inlineTagName == "br")
                    {
                        renderer.WriteInline(new MyLineBreak());
                    }
                    else if (inlineTagName == "a")
                    {
                        IAddChild hyperLink;
                        if (node.ChildNodes.Any(n => n.Name != "#text"))
                        {
                            var myHyperlinkButton = new MyHyperlinkButton(node, renderer.Config.BaseUrl);
                            //myHyperlinkButton.ClickEvent += (sender, e) =>
                            //{
                            //    //renderer.MarkdownTextBlock.RaiseLinkClickedEvent(((HyperlinkButton)sender).NavigateUri);
                            //};
                            hyperLink = myHyperlinkButton;
                        }
                        else
                        {
                            var myHyperlink = new MyHyperlink(node, renderer.Config.BaseUrl);
                            //myHyperlink.ClickEvent += (sender, e) =>
                            //{
                            //    renderer.MarkdownTextBlock.RaiseLinkClickedEvent(sender.NavigateUri);
                            //};
                            hyperLink = myHyperlink;
                        }
                        renderer.Push(hyperLink);
                        WriteHtml(renderer, node.ChildNodes);
                        renderer.Pop();
                    }
                    else if (inlineTagName == "img")
                    {
                        var image = new MyImage(node, renderer.Config);
                        renderer.WriteInline(image);
                    }
                    else
                    {
                        var inline = new MyInline(node);
                        renderer.Push(inline);
                        WriteHtml(renderer, node.ChildNodes);
                        renderer.Pop();
                    }
                }
                else if (node.NodeType == HtmlNodeType.Element && node.Name.TagToType() == TextElements.HtmlElementType.Block)
                {
                    IAddChild block;
                    var tag = node.Name.ToLower();
                    if (tag == "details")
                    {
                        block = new MyDetails(node);
                        node.ChildNodes.Remove(node.ChildNodes.FirstOrDefault(x => x.Name == "summary" || x.Name == "header"));
                        renderer.Push(block);
                        WriteHtml(renderer, node.ChildNodes);
                    }
                    else if (tag.IsHeading())
                    {
                        var heading = new MyHeading(node, renderer.Config);
                        renderer.Push(heading);
                        WriteHtml(renderer, node.ChildNodes);
                    }
                    else
                    {
                        block = new MyBlock(node);
                        renderer.Push(block);
                        WriteHtml(renderer, node.ChildNodes);
                    }
                    renderer.Pop();
                }
            }
        }
    }
}
