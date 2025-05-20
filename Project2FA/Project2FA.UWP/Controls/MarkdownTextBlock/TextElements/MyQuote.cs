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

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyQuote : IAddChild
    {
        private Paragraph _paragraph;
        private MyFlowDocument _flowDocument;
        private QuoteBlock _quoteBlock;

        public TextElement TextElement
        {
            get => _paragraph;
        }

        public MyQuote(QuoteBlock quoteBlock)
        {
            _quoteBlock = quoteBlock;
            _paragraph = new Paragraph();

            _flowDocument = new MyFlowDocument(quoteBlock);
            var inlineUIContainer = new InlineUIContainer();

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            var bar = new Grid();
            bar.Width = 4;
            bar.Background = new SolidColorBrush(Colors.Gray);
            bar.SetValue(Grid.ColumnProperty, 0);
            bar.VerticalAlignment = VerticalAlignment.Stretch;
            bar.Margin = new Thickness(0, 0, 4, 0);
            grid.Children.Add(bar);

            var rightGrid = new Grid();
            rightGrid.Padding = new Thickness(4);
            rightGrid.Children.Add(_flowDocument.RichTextBlock);

            rightGrid.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(rightGrid);
            grid.Margin = new Thickness(0, 2, 0, 2);

            inlineUIContainer.Child = grid;

            _paragraph.Inlines.Add(inlineUIContainer);
        }

        public void AddChild(IAddChild child)
        {
            _flowDocument.AddChild(child);
        }
    }
}
