using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace Project2FA.UWP.Controls;

internal static partial class TextRenderingHelper
{
    public static List<GraphemeCluster> GenerateGraphemeClusters(string source, CanvasTextLayout textLayout)
    {
        List<GraphemeCluster> graphemeClusters = new List<GraphemeCluster>();

        int clusterCount = textLayout.ClusterMetrics.Length;
        int offset = 0;

        for (int i = 0; i < clusterCount; i++)
        {
            GraphemeCluster cluster = new GraphemeCluster();
            textLayout.GetCaretPosition(offset, true, out CanvasTextLayoutRegion clusterLayoutRegion);

            // Detect trimmed clusters
            if (i > 0 && offset > 0 && clusterLayoutRegion.CharacterCount == 0)
            {
                graphemeClusters.Last().IsTrimmed = true;
                break;
            }
            else
            {
                cluster.Characters = source.Substring(clusterLayoutRegion.CharacterIndex, clusterLayoutRegion.CharacterCount);
                cluster.Offset = clusterLayoutRegion.CharacterIndex;
                cluster.Length = clusterLayoutRegion.CharacterCount;
                cluster.LayoutBounds = clusterLayoutRegion.LayoutBounds;
                cluster.DrawBounds = GetClusterDrawBounds(cluster, textLayout);

                offset += clusterLayoutRegion.CharacterCount;
                graphemeClusters.Add(cluster);
            }
        }

        return graphemeClusters;
    }

    public static Rect GetClusterDrawBounds(GraphemeCluster cluster, CanvasTextLayout textLayout)
    {
        switch (textLayout.HorizontalAlignment)
        {
            case CanvasHorizontalAlignment.Justified:
            case CanvasHorizontalAlignment.Left:
                return new Rect(cluster.LayoutBounds.Left,
                    cluster.LayoutBounds.Y + cluster.LayoutBounds.Height * 0.5,
                    cluster.LayoutBounds.Width,
                    cluster.LayoutBounds.Height);
            case CanvasHorizontalAlignment.Right:
                return new Rect(cluster.LayoutBounds.Right,
                    cluster.LayoutBounds.Y + cluster.LayoutBounds.Height * 0.5,
                    cluster.LayoutBounds.Width,
                    cluster.LayoutBounds.Height);
            case CanvasHorizontalAlignment.Center:
                return new Rect(cluster.LayoutBounds.X + cluster.LayoutBounds.Width * 0.5,
                    cluster.LayoutBounds.Y + cluster.LayoutBounds.Height * 0.5,
                    cluster.LayoutBounds.Width,
                    cluster.LayoutBounds.Height);
        }

        return cluster.LayoutBounds;
    }
}
