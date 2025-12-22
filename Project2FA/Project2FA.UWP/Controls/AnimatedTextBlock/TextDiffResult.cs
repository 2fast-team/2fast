namespace Project2FA.UWP.Controls;

public partial class TextDiffResult
{
    public AnimatedTextBlockDiffOperationType Type { get; set; }

    public GraphemeCluster OldGlyphCluster { get; set; }

    public GraphemeCluster NewGlyphCluster { get; set; }

    public int OldClusterOffset { get; set; }

    public int NewClusterOffset { get; set; }

    public TextDiffResult(AnimatedTextBlockDiffOperationType type)
    {
        Type = type;
    }

    public TextDiffResult(AnimatedTextBlockDiffOperationType type, int oldClusterOffset, int newClusterOffset)
    {
        Type = type;
        OldClusterOffset = oldClusterOffset;
        NewClusterOffset = newClusterOffset;
    }

    public TextDiffResult(AnimatedTextBlockDiffOperationType type, GraphemeCluster oldGlyphCluster, GraphemeCluster newGlyphCluster)
    {
        Type = type;
        OldGlyphCluster = oldGlyphCluster;
        NewGlyphCluster = newGlyphCluster;
    }

    public TextDiffResult(AnimatedTextBlockDiffOperationType type, GraphemeCluster oldGlyphCluster, GraphemeCluster newGlyphCluster, int oldClusterOffset, int newClusterOffset)
    {
        Type = type;
        OldGlyphCluster = oldGlyphCluster;
        NewGlyphCluster = newGlyphCluster;
        OldClusterOffset = oldClusterOffset;
        NewClusterOffset = newClusterOffset;
    }
}
