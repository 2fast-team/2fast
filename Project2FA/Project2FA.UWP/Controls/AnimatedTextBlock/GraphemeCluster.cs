using System;
using Windows.Foundation;

namespace Project2FA.UWP.Controls;

public partial class GraphemeCluster : IEquatable<GraphemeCluster>
{
    /// <summary>
    /// The content of the cluster.
    /// </summary>
    public string Characters { get; set; }

    /// <summary>
    /// Start index of the first character in the cluster.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Length of the characters in the cluster.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// A rectangle describing the layout bounds of the cluster.
    /// </summary>
    public Rect LayoutBounds { get; set; }

    /// <summary>
    /// A rectangle describing the bounds of the parts of the cluster that would get drawn.
    /// </summary>
    public Rect DrawBounds { get; set; }

    /// <summary>
    /// Indicates if the cluster is the trimming sign cluster.
    /// </summary>
    public bool IsTrimmed { get; set; }

    /// <summary>
    /// Progress of the cluster's layout animation.
    /// </summary>
    public float Progress { get; set; }

    public bool IsAnimationFinished { get; internal set; }

    public bool Equals(GraphemeCluster other)
    {
        return other != null && string.Equals(this.Characters, other.Characters, StringComparison.Ordinal);
    }

    public override bool Equals(object obj) => this.Equals(obj as GraphemeCluster);
    public override int GetHashCode()
    {
        return Characters?.GetHashCode() ?? 0;
    }
}
