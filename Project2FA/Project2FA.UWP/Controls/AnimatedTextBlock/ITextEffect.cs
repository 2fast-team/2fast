using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace Project2FA.UWP.Controls;

public interface ITextEffect
{
    /// <summary>
    /// Gets or sets the length of time that the animation takes.
    /// </summary>
    TimeSpan AnimationDuration { get; set; }

    /// <summary>
    /// Gets or sets the amount of time to wait from applying the animation to every grapheme cluster.
    /// </summary>
    TimeSpan DelayPerCluster { get; set; }

    /// <summary>
    /// Implement this method to update any values used for the animation. This method is called on a timed interval. 
    /// </summary>
    /// <param name="oldText">The unchanged text.</param>
    /// <param name="newText">The changed text.</param>
    /// <param name="diffResults">A set of changes as a result of the calculated differences of the texts.</param>
    /// <param name="oldTextLayout">The text layout instance of the unchanged text.</param>
    /// <param name="newTextLayout">The text layout instance of the changed text.</param>
    /// <param name="state">Current drawing state of the control.</param>
    /// <param name="canvas">The animated Win2D canvas control intended for rendering text.</param>
    /// <param name="args">Data for update calculations.</param>
    void Update(string oldText,
        string newText,
        List<TextDiffResult> diffResults,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        AnimatedTextBlockRedrawState state,
        ICanvasAnimatedControl canvas,
        CanvasAnimatedUpdateEventArgs args);

    /// <summary>
    /// Implement this method to draw the texts.
    /// </summary>
    /// <param name="oldText">The unchanged text.</param>
    /// <param name="newText">The changed text.</param>
    /// <param name="diffResults">A set of changes as a result of the calculated differences of the texts.</param>
    /// <param name="oldTextLayout">The text layout instance of the unchanged text.</param>
    /// <param name="newTextLayout">The text layout instance of the changed text.</param>
    /// <param name="textFormat">The text format instance which describes desired font styles.</param>
    /// <param name="textColor">The desired text color.</param>
    /// <param name="gradientBrush">The gradient brush for rendering text with gradient colors.</param>
    /// <param name="state">Current drawing state of the control.</param>
    /// <param name="drawingSession">The drawing sessions used to issue text drawing commands.</param>
    /// <param name="args">Data for drawing operations.</param>
    void DrawText(string oldText,
        string newText,
        List<TextDiffResult> diffResults,
        CanvasTextLayout oldTextLayout,
        CanvasTextLayout newTextLayout,
        CanvasTextFormat textFormat,
        Color textColor,
        CanvasLinearGradientBrush gradientBrush,
        AnimatedTextBlockRedrawState state,
        CanvasDrawingSession drawingSession,
        CanvasAnimatedDrawEventArgs args);
}
