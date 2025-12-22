using Microsoft.Graphics.Canvas.Text;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Controls;

internal static partial class Win2dHelpers
{
    public static CanvasHorizontalAlignment MapCanvasHorizontalAlignment(TextAlignment alignment)
    {
        switch (alignment)
        {
            case TextAlignment.DetectFromContent:
            case TextAlignment.Center:
                return CanvasHorizontalAlignment.Center;
            default:
            case TextAlignment.Left:
                return CanvasHorizontalAlignment.Left;
            case TextAlignment.Right:
                return CanvasHorizontalAlignment.Right;
            case TextAlignment.Justify:
                return CanvasHorizontalAlignment.Justified;
        }
    }

    public static CanvasTextDirection MapTextDirection(AnimatedTextBlockTextDirection textDirection)
    {
        switch (textDirection)
        {
            default:
            case AnimatedTextBlockTextDirection.LeftToRightThenTopToBottom:
                return CanvasTextDirection.LeftToRightThenTopToBottom;
            case AnimatedTextBlockTextDirection.RightToLeftThenTopToBottom:
                return CanvasTextDirection.RightToLeftThenTopToBottom;
            case AnimatedTextBlockTextDirection.LeftToRightThenBottomToTop:
                return CanvasTextDirection.LeftToRightThenBottomToTop;
            case AnimatedTextBlockTextDirection.RightToLeftThenBottomToTop:
                return CanvasTextDirection.RightToLeftThenBottomToTop;
            case AnimatedTextBlockTextDirection.TopToBottomThenLeftToRight:
                return CanvasTextDirection.TopToBottomThenLeftToRight;
            case AnimatedTextBlockTextDirection.BottomToTopThenLeftToRight:
                return CanvasTextDirection.BottomToTopThenLeftToRight;
            case AnimatedTextBlockTextDirection.TopToBottomThenRightToLeft:
                return CanvasTextDirection.TopToBottomThenRightToLeft;
            case AnimatedTextBlockTextDirection.BottomToTopThenRightToLeft:
                return CanvasTextDirection.BottomToTopThenRightToLeft;
        }
    }

    public static CanvasTextTrimmingGranularity MapTrimmingGranularity(TextTrimming textTrimming)
    {
        switch (textTrimming)
        {
            default:
            case TextTrimming.None:
                return CanvasTextTrimmingGranularity.None;
            case TextTrimming.CharacterEllipsis:
                return CanvasTextTrimmingGranularity.Character;
            case TextTrimming.WordEllipsis:
                return CanvasTextTrimmingGranularity.Word;
            case TextTrimming.Clip:
                return CanvasTextTrimmingGranularity.None;
        }
    }

    public static CanvasWordWrapping MapWordWrapping(TextWrapping textWrapping)
    {
        switch (textWrapping)
        {
            default:
            case TextWrapping.NoWrap:
                return CanvasWordWrapping.NoWrap;
            case TextWrapping.Wrap:
                return CanvasWordWrapping.Character;
            case TextWrapping.WrapWholeWords:
                return CanvasWordWrapping.WholeWord;
        }
    }

    public static string GenerateTrimmingSign(this CanvasTextLayout layout)
    {
        return "\u2026";
    }
}
