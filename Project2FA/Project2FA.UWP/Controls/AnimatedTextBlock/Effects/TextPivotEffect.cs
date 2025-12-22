using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;

// based on https://github.com/ghost1372/DevWinUI/tree/f47772be554df867dfff2f09bb912244d35e6437/dev/DevWinUI.Controls/Controls/Win2DAndComposition/AnimatedTextBlock

namespace Project2FA.UWP.Controls;

public partial class TextPivotEffect : ITextEffect
{
    public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

    public TimeSpan DelayPerCluster { get; set; } = TimeSpan.FromMilliseconds(30);

    public void Update(string oldText,
        string newText,
        List<TextDiffResult> diffResults,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        AnimatedTextBlockRedrawState state,
        ICanvasAnimatedControl canvas,
        CanvasAnimatedUpdateEventArgs args)
    {

    }

    public void DrawText(string oldText,
        string newText,
        List<TextDiffResult> diffResults,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush,
        AnimatedTextBlockRedrawState state,
        CanvasDrawingSession drawingSession,
        CanvasAnimatedDrawEventArgs args)
    {
        if (diffResults == null)
            return;

        var ds = args.DrawingSession;

        if (state == AnimatedTextBlockRedrawState.Idle)
        {
            DrawIdle(ds,
                oldTextLayout,
                newTextLayout,
                textFormat,
                textColor,
                gradientBrush);

            return;
        }

        foreach (var diffResult in diffResults)
        {
            switch (diffResult.Type)
            {
                case AnimatedTextBlockDiffOperationType.Insert:
                    DrawInsert(ds,
                        diffResult.OldGlyphCluster,
                        diffResult.NewGlyphCluster,
                        oldTextLayout,
                        newTextLayout,
                        textFormat,
                        textColor,
                        gradientBrush);
                    break;
                case AnimatedTextBlockDiffOperationType.Remove:
                    DrawRemove(ds,
                        diffResult.OldGlyphCluster,
                        diffResult.NewGlyphCluster,
                        oldTextLayout,
                        newTextLayout,
                        textFormat,
                        textColor,
                        gradientBrush);
                    break;
                case AnimatedTextBlockDiffOperationType.Stay:
                case AnimatedTextBlockDiffOperationType.Move:
                    DrawMove(ds,
                        diffResult.OldGlyphCluster,
                        diffResult.NewGlyphCluster,
                        oldTextLayout,
                        newTextLayout,
                        textFormat,
                        textColor,
                        gradientBrush);
                    break;
                case AnimatedTextBlockDiffOperationType.Update:
                    DrawUpdate(ds,
                        diffResult.OldGlyphCluster,
                        diffResult.NewGlyphCluster,
                        oldTextLayout,
                        newTextLayout,
                        textFormat,
                        textColor,
                        gradientBrush);
                    break;
            }
        }
    }

    private void DrawIdle(CanvasDrawingSession ds,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush)
    {
        ds.Transform = Matrix3x2.Identity;
        ds.DrawTextLayout(newTextLayout, 0, 0, textColor);
    }

    private void DrawInsert(CanvasDrawingSession ds,
        GraphemeCluster oldCluster,
        GraphemeCluster newCluster,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush)
    {
        if (newCluster == null)
        {
            return;
        }

        CanvasCommandList cl = new CanvasCommandList(ds);
        float newProgress = 1.0f - Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);

        using (CanvasDrawingSession clds = cl.CreateDrawingSession())
        {
            clds.DrawText(
                newCluster.IsTrimmed
                    ? newTextLayout.GenerateTrimmingSign()
                    : newCluster.Characters,
                (float)newCluster.DrawBounds.X,
                (float)newCluster.DrawBounds.Y,
                textColor,
                textFormat);

            clds.Transform = Matrix3x2.Identity;
        }

        using (ds.CreateLayer(1.0f - newProgress))
        {
            using (var transformEffect = new Transform3DEffect())
            {
                transformEffect.Source = cl;
                transformEffect.TransformMatrix = Matrix4x4.CreateRotationY(newProgress,
                    new Vector3((float)(newCluster.LayoutBounds.X + newCluster.LayoutBounds.Width * 0.5),
                        (float)(newCluster.LayoutBounds.Y + newCluster.LayoutBounds.Height * 0.5),
                        0));

                ds.DrawImage(transformEffect);
            }
        }
    }

    private void DrawMove(CanvasDrawingSession ds,
        GraphemeCluster oldCluster,
        GraphemeCluster newCluster,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush)
    {
        if (oldCluster == null || newCluster == null)
        {
            return;
        }

        float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);
        float pivotProgress = 0;

        var oX = oldCluster.DrawBounds.X;
        var oY = oldCluster.DrawBounds.Y;
        var nX = newCluster.DrawBounds.X;
        var nY = newCluster.DrawBounds.Y;

        var dX = nX - oX;
        var dY = nY - oY;

        if (dX != 0)
        {
            pivotProgress = Easing.UpdateProgress(oldCluster.Progress * 2.0f, Easing.EasingFunction.SinusoidalOut);
            pivotProgress = (float)Math.Clamp(pivotProgress, 0, 0.5);
        }

        CanvasCommandList cl = new CanvasCommandList(ds);
        using (CanvasDrawingSession clds = cl.CreateDrawingSession())
        {
            clds.DrawText(
                newCluster.IsTrimmed
                    ? newTextLayout.GenerateTrimmingSign()
                    : newCluster.Characters,
                (float)(oX + dX * oldProgress),
                (float)(oY + dY * oldProgress),
                textColor,
                textFormat);

            clds.Transform = Matrix3x2.Identity;
        }

        using (ds.CreateLayer(1.0f))
        {
            using (var transformEffect = new Transform3DEffect())
            {
                transformEffect.Source = cl;
                transformEffect.TransformMatrix = Matrix4x4.CreateRotationY(pivotProgress,
                    new Vector3((float)(newCluster.LayoutBounds.X + newCluster.LayoutBounds.Width * 0.5),
                        (float)(newCluster.LayoutBounds.Y + newCluster.LayoutBounds.Height * 0.5),
                        0));

                ds.DrawImage(transformEffect);
            }
        }
    }

    private void DrawUpdate(CanvasDrawingSession ds,
        GraphemeCluster oldCluster,
        GraphemeCluster newCluster,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush)
    {
        if (oldCluster == null || newCluster == null)
        {
            return;
        }

        float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);
        float newProgress = 1.0f - Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);

        CanvasCommandList oCl = new CanvasCommandList(ds);

        using (CanvasDrawingSession clds = oCl.CreateDrawingSession())
        {
            clds.DrawText(
                oldCluster.IsTrimmed
                    ? oldTextLayout.GenerateTrimmingSign()
                    : oldCluster.Characters,
                (float)oldCluster.DrawBounds.X,
                (float)oldCluster.DrawBounds.Y,
                textColor,
                textFormat);

            clds.Transform = Matrix3x2.Identity;
        }

        using (ds.CreateLayer(1.0f - oldProgress))
        {
            using (var transformEffect = new Transform3DEffect())
            {
                transformEffect.Source = oCl;
                transformEffect.TransformMatrix = Matrix4x4.CreateRotationY(oldProgress,
                    new Vector3((float)(oldCluster.LayoutBounds.X + oldCluster.LayoutBounds.Width * 0.5),
                        (float)(oldCluster.LayoutBounds.Y + oldCluster.LayoutBounds.Height * 0.5),
                        0));

                ds.DrawImage(transformEffect);
            }
        }

        CanvasCommandList nCl = new CanvasCommandList(ds);

        using (CanvasDrawingSession clds = nCl.CreateDrawingSession())
        {
            clds.Transform = Matrix3x2.CreateTranslation(0,
                (float)(newCluster.LayoutBounds.Height * newProgress));

            clds.DrawText(
                newCluster.IsTrimmed
                    ? newTextLayout.GenerateTrimmingSign()
                    : newCluster.Characters,
                (float)newCluster.DrawBounds.X,
                (float)newCluster.DrawBounds.Y,
                textColor,
                textFormat);

            clds.Transform = Matrix3x2.Identity;
        }

        using (ds.CreateLayer(1.0f - newProgress))
        {
            using (var transformEffect = new Transform3DEffect())
            {
                transformEffect.Source = nCl;
                transformEffect.TransformMatrix = Matrix4x4.CreateRotationY(newProgress,
                    new Vector3((float)(newCluster.LayoutBounds.X + newCluster.LayoutBounds.Width * 0.5),
                        (float)(newCluster.LayoutBounds.Y + newCluster.LayoutBounds.Height * 0.5),
                        0));

                ds.DrawImage(transformEffect);
            }
        }
    }

    private void DrawRemove(CanvasDrawingSession ds,
        GraphemeCluster oldCluster,
        GraphemeCluster newCluster,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush)
    {
        if (oldCluster == null)
        {
            return;
        }

        CanvasCommandList cl = new CanvasCommandList(ds);
        float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);

        using (CanvasDrawingSession clds = cl.CreateDrawingSession())
        {
            clds.DrawText(
                oldCluster.IsTrimmed
                    ? oldTextLayout.GenerateTrimmingSign()
                    : oldCluster.Characters,
                (float)oldCluster.DrawBounds.X,
                (float)oldCluster.DrawBounds.Y,
                textColor,
                textFormat);

            clds.Transform = Matrix3x2.Identity;
        }

        using (ds.CreateLayer(1.0f - oldProgress))
        {
            using (var transformEffect = new Transform3DEffect())
            {
                transformEffect.Source = cl;
                transformEffect.TransformMatrix = Matrix4x4.CreateRotationY(oldProgress,
                    new Vector3((float)(oldCluster.LayoutBounds.X + oldCluster.LayoutBounds.Width * 0.5),
                        (float)(oldCluster.LayoutBounds.Y + oldCluster.LayoutBounds.Height * 0.5),
                        0));

                ds.DrawImage(transformEffect);
            }
        }
    }
}
