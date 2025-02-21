// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyThematicBreak : IAddChild
    {
        private ThematicBreakBlock _thematicBreakBlock;
        private Paragraph _paragraph;

        public TextElement TextElement
        {
            get => _paragraph;
        }

        public MyThematicBreak(ThematicBreakBlock thematicBreakBlock)
        {
            _thematicBreakBlock = thematicBreakBlock;
            _paragraph = new Paragraph();

            var inlineUIContainer = new InlineUIContainer();
            Line line = new Line
            {
                Stretch = Stretch.Fill,
                Stroke = new SolidColorBrush(Colors.Gray),
                X2 = 1,
                Margin = new Thickness(0, 12, 0, 12)
            };
            inlineUIContainer.Child = line;
            _paragraph.Inlines.Add(inlineUIContainer);
        }

        public void AddChild(IAddChild child) {}
    }
}
