using Markdig.Syntax;
using RomanNumerals;
using System.Globalization;

namespace Symptum.UI.Markdown.TextElements;

public class ListElement : IAddChild
{
    private SContainer _container;
    private StackPanel _stackPanel;
    private MarkdownConfiguration _config;
    private BulletType _bulletType;
    private bool _isOrdered;
    private int _startIndex = 1;
    private int _index = 1;
    private const string _dot = "• ";

    public STextElement TextElement => _container;

    public ListElement(ListBlock listBlock, MarkdownConfiguration config, bool isTopLevel = true)
    {
        _container = new SContainer();
        _config = config;

        if (listBlock.IsOrdered)
        {
            _isOrdered = true;
            _bulletType = ToBulletType(listBlock.BulletType);

            if (listBlock.OrderedStart != null && listBlock.DefaultOrderedStart != listBlock.OrderedStart)
            {
                _startIndex = int.Parse(listBlock.OrderedStart, NumberFormatInfo.InvariantInfo);
                _index = _startIndex;
            }
        }

        _stackPanel = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = config.Themes.Spacing,
        };

        if (isTopLevel)
            _stackPanel.Padding = config.Themes.ListMargin;

        _container.UIElement = _stackPanel;
    }

    public void AddChild(IAddChild child)
    {
        Grid grid = new();
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        string bullet;
        if (_isOrdered)
        {
            bullet = _bulletType switch
            {
                BulletType.Number => $"{_index}. ",
                BulletType.LowerAlpha => $"{_index.ToAlphabetical()}. ",
                BulletType.UpperAlpha => $"{_index.ToAlphabetical().ToUpper()}. ",
                BulletType.LowerRoman => $"{_index.ToRomanNumerals().ToLower()} ",
                BulletType.UpperRoman => $"{_index.ToRomanNumerals().ToUpper()} ",
                BulletType.Circle => _dot,
                _ => _dot
            };
            _index++;
        }
        else
        {
            bullet = _dot;
        }
        TextBlock textBlock = new()
        {
            Text = bullet,
            Style = _config.Themes.BodyTextBlockStyle
        };
        textBlock.SetValue(Grid.ColumnProperty, 0);
        textBlock.VerticalAlignment = VerticalAlignment.Top;
        grid.Children.Add(textBlock);
        FlowDocumentElement flowDoc = new(_config, false);
        flowDoc.AddChild(child);

        flowDoc.StackPanel.SetValue(Grid.ColumnProperty, 1);
        flowDoc.StackPanel.Padding = new(_config.Themes.ListBulletSpacing, 0, 0, 0);
        flowDoc.StackPanel.VerticalAlignment = VerticalAlignment.Top;
        grid.Children.Add(flowDoc.StackPanel);

        _stackPanel.Children.Add(grid);
    }

    private static BulletType ToBulletType(char bullet)
    {
        // Gets or sets the type of the bullet (e.g: '1', 'a', 'A', 'i', 'I').
        return bullet switch
        {
            '1' => BulletType.Number,
            'a' => BulletType.LowerAlpha,
            'A' => BulletType.UpperAlpha,
            'i' => BulletType.LowerRoman,
            'I' => BulletType.UpperRoman,
            _ => BulletType.Circle,
        };
    }
}

internal enum BulletType
{
    Circle,
    Number,
    LowerAlpha,
    UpperAlpha,
    LowerRoman,
    UpperRoman
}
