using Markdig.Extensions.Tables;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers.Extensions;

public class TableRenderer : WinUIObjectRenderer<Table>
{
    protected override void Write(WinUIRenderer renderer, Table table)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(table);

        TableElement _table = new(table, renderer.Configuration);

        renderer.Push(_table);

        for (int rowIndex = 0; rowIndex < table.Count; rowIndex++)
        {
            Markdig.Syntax.Block rowObj = table[rowIndex];
            TableRow row = (TableRow)rowObj;

            for (int i = 0; i < row.Count; i++)
            {
                Markdig.Syntax.Block cellObj = row[i];
                TableCell cell = (TableCell)cellObj;
                TextAlignment textAlignment = TextAlignment.Left;

                int columnIndex = i;

                if (table.ColumnDefinitions.Count > 0)
                {
                    columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= table.ColumnDefinitions.Count
                        ? i
                        : cell.ColumnIndex;
                    columnIndex = columnIndex >= table.ColumnDefinitions.Count ? table.ColumnDefinitions.Count - 1 : columnIndex;
                    TableColumnAlign? alignment = table.ColumnDefinitions[columnIndex].Alignment;
                    textAlignment = alignment switch
                    {
                        TableColumnAlign.Center => TextAlignment.Center,
                        TableColumnAlign.Left => TextAlignment.Left,
                        TableColumnAlign.Right => TextAlignment.Right,
                        _ => TextAlignment.Left,
                    };
                }

                TableCellElement _cell = new(cell, renderer.Configuration, textAlignment, row.IsHeader, columnIndex, rowIndex);

                renderer.Push(_cell);
                renderer.Write(cell);
                renderer.Pop();
            }
        }

        renderer.Pop();
    }
}
