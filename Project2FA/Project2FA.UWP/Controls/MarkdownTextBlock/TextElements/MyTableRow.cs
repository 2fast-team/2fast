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

using Markdig.Extensions.Tables;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyTableRow : IAddChild
    {
        private TableRow _tableRow;
        private StackPanel _stackPanel;
        private Paragraph _paragraph;

        public TextElement TextElement
        {
            get => _paragraph;
        }

        public MyTableRow(TableRow tableRow)
        {
            _tableRow = tableRow;
            _paragraph = new Paragraph();

            _stackPanel = new StackPanel();
            _stackPanel.Orientation = Orientation.Horizontal;
            var inlineUIContainer = new InlineUIContainer();
            inlineUIContainer.Child = _stackPanel;
            _paragraph.Inlines.Add(inlineUIContainer);
        }

#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Paragraph))]
#endif
        public void AddChild(IAddChild child)
        {
            if (child is MyTableCell cellChild)
            {
                var richTextBlock = new RichTextBlock();
                richTextBlock.Blocks.Add((Paragraph)cellChild.TextElement);
                _stackPanel.Children.Add(richTextBlock);
            }
        }
    }
}
