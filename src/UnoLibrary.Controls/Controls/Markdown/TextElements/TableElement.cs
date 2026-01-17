using Markdig.Extensions.Tables;

namespace Symptum.UI.Markdown.TextElements;

public class TableElement : IAddChild
{
    private Table _table;
    private SContainer _container;
    private Grid _grid;
    private int _columnCount;
    private int _rowCount;
    private MarkdownConfiguration _config;

    public STextElement TextElement => _container;

    public TableElement(Table table, MarkdownConfiguration config)
    {
        _table = table;
        _config = config;
        _container = new();
        _columnCount = table.FirstOrDefault() is TableRow row ? row.Select(x => x is TableCell cell ? cell.ColumnSpan : 1).Sum() : 0;
        _rowCount = table.Count;

        _grid = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new(16, 8, 8, 8)
        };

        for (int i = 0; i < _columnCount; i++)
        {
            _grid.ColumnDefinitions.Add(new());
        }
        for (int i = 0; i < _rowCount; i++)
        {
            _grid.RowDefinitions.Add(new());
        }

        _container.UIElement = _grid;
    }

    public void AddChild(IAddChild child)
    {
        if (child is TableCellElement cellChild && cellChild.TextElement is SContainer container && container.UIElement is Grid cell)
        {
            Grid.SetColumn(cell, cellChild.ColumnIndex);
            Grid.SetRow(cell, cellChild.RowIndex);
            Grid.SetColumnSpan(cell, cellChild.ColumnSpan);
            Grid.SetRowSpan(cell, cellChild.RowSpan);

            cell.BorderThickness = GetBorderThickness(cellChild.RowIndex, cellChild.ColumnIndex);
            cell.CornerRadius = GetCornerRadius(cellChild.RowIndex, cellChild.ColumnIndex, cellChild.RowSpan, cellChild.ColumnSpan);
            if (!cellChild.IsHeader && cellChild.RowIndex % 2 == 0)
            {
                cell.Style = _config.Themes.AltTableCellGridStyle;
            }

            _grid.Children.Add(cell);
        }
    }

    private double uniformBorderThickness = 1;

    private Thickness GetBorderThickness(int rowIndex, int columnIndex)
    {
        double l = columnIndex == 0 ? uniformBorderThickness : 0;
        double t = rowIndex == 0 ? uniformBorderThickness : 0;
        double r = uniformBorderThickness;
        double b = uniformBorderThickness;
        return new(l, t, r, b);
    }

    private double cornerRadius = 4;

    private CornerRadius GetCornerRadius(int rowIndex, int columnIndex, int rowSpan, int columnSpan)
    {
        double tl = rowIndex == 0 && columnIndex == 0 ? cornerRadius : 0;
        double tr = rowIndex == 0 && columnIndex == _columnCount - columnSpan ? cornerRadius : 0;
        double br = rowIndex == _rowCount - rowSpan && columnIndex == _columnCount - columnSpan ? cornerRadius : 0;
        double bl = rowIndex == _rowCount - rowSpan && columnIndex == 0 ? cornerRadius : 0;
        return new(tl, tr, br, bl);
    }
}
