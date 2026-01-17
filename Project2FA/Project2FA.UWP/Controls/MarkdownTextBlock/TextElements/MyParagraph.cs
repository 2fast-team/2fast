// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Markdig.Extensions.TaskLists;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.Foundation;
using System;
using System.Linq;
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
    internal class MyParagraph : IAddChild
    {
        private ParagraphBlock _paragraphBlock;
        private Paragraph _paragraph;

        public TextElement TextElement
        {
            get => _paragraph;
        }

        public MyParagraph(ParagraphBlock paragraphBlock)
        {
            _paragraphBlock = paragraphBlock;
            _paragraph = new Paragraph();
        }

#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Inline))]
        [DynamicWindowsRuntimeCast(typeof(Block))]
#endif
        public void AddChild(IAddChild child)
        {
            if (child.TextElement is Inline inlineChild)
            {
                _paragraph.Inlines.Add(inlineChild);
            }
#if !WINAPPSDK
            else if (child.TextElement is Block blockChild)
#else
            else if (child.TextElement is Microsoft.UI.Xaml.Documents.Block blockChild)
#endif
            {
                var inlineUIContainer = new InlineUIContainer();
                var richTextBlock = new RichTextBlock();
                richTextBlock.TextWrapping = TextWrapping.Wrap;
                richTextBlock.Blocks.Add(blockChild);
                inlineUIContainer.Child = richTextBlock;
                _paragraph.Inlines.Add(inlineUIContainer);
            }
        }
    }
}
