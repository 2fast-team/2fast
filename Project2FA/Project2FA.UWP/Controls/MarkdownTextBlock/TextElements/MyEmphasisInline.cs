// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Markdig.Syntax.Inlines;
using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif

namespace CommunityToolkit.Labs.WinUI.MarkdownTextBlock.TextElements
{
    internal class MyEmphasisInline : IAddChild
    {
        private Span _span;
        private EmphasisInline _markdownObject;

        private bool _isBold;
        private bool _isItalic;
        private bool _isStrikeThrough;

        public TextElement TextElement
        {
            get => _span;
        }

        public MyEmphasisInline(EmphasisInline emphasisInline)
        {
            _span = new Span();
            _markdownObject = emphasisInline;
        }
#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Run))]
#endif
        public void AddChild(IAddChild child)
        {
            try
            {
                if (child is MyInlineText inlineText)
                {
                    _span.Inlines.Add((Run)inlineText.TextElement);
                }
                else if (child is MyEmphasisInline emphasisInline)
                {
                    if (emphasisInline._isBold) { SetBold(); }
                    if (emphasisInline._isItalic) { SetItalic(); }
                    if (emphasisInline._isStrikeThrough) { SetStrikeThrough(); }
                    _span.Inlines.Add(emphasisInline._span);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in {nameof(MyEmphasisInline)}.{nameof(AddChild)}: {ex.Message}");
            }
        }

        public void SetBold()
        {
            #if WINUI3
            _span.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
            #elif WINUI2
            _span.FontWeight = Windows.UI.Text.FontWeights.Bold;
            #endif

            _isBold = true;
        }

        public void SetItalic()
        {
            _span.FontStyle = FontStyle.Italic;
            _isItalic = true;
        }

        public void SetStrikeThrough()
        {
            #if WINUI3
            _span.TextDecorations = Windows.UI.Text.TextDecorations.Strikethrough;
            #elif WINUI2
            _span.TextDecorations = Windows.UI.Text.TextDecorations.Strikethrough;
            #endif

            _isStrikeThrough = true;
        }

        public void SetSubscript()
        {
            _span.SetValue(Typography.VariantsProperty, FontVariants.Subscript);
        }

        public void SetSuperscript()
        {
            _span.SetValue(Typography.VariantsProperty, FontVariants.Superscript);
        }
    }
}
