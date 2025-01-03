// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace Project2FA.UWP.Controls
{
    public sealed partial class CopyButton : Button
    {
        public CopyButton()
        {
            this.DefaultStyleKey = typeof(CopyButton);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetTemplateChild("CopyToClipboardSuccessAnimation") is Storyboard _storyBoard)
            {
                _storyBoard.Begin();
                //UIHelper.AnnounceActionForAccessibility(this, "Copied to clipboard", "CopiedToClipboardActivityId");
            }
        }

        protected override void OnApplyTemplate()
        {
            Click -= CopyButton_Click;
            base.OnApplyTemplate();
            Click += CopyButton_Click;
        }
    }
}
