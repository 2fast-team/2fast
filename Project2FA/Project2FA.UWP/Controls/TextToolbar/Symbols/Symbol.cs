// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Project2FA.UWP.Helpers;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Text;
#endif

namespace Project2FA.UWP.Controls.TextToolbarSymbols
{
    /// <summary>
    /// Common implementation for a symbol icon
    /// </summary>
    [TemplateVisualState(GroupName = Common, Name = Normal)]
    [TemplateVisualState(GroupName = Common, Name = Disabled)]
    public abstract partial class Symbol : Control
    {
        internal const string Common = "CommonStates";
        internal const string Normal = "Normal";
        internal const string Disabled = "Disabled";

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            isEnabledWatcher = new DependencyPropertyWatcher<bool>(this, nameof(IsEnabled));
            isEnabledWatcher.PropertyChanged += IsEnabledWatcher_PropertyChanged;
            CheckIsEnabled();
            base.OnApplyTemplate();
        }

        private void IsEnabledWatcher_PropertyChanged(object sender, System.EventArgs e)
        {
            CheckIsEnabled();
        }

        private void CheckIsEnabled()
        {
            if (IsEnabled)
            {
                VisualStateManager.GoToState(this, Normal, true);
            }
            else
            {
                VisualStateManager.GoToState(this, Disabled, true);
            }
        }

        private DependencyPropertyWatcher<bool> isEnabledWatcher;

        /// <summary>
        /// Finalizes an instance of the <see cref="Symbol"/> class.
        /// </summary>
        ~Symbol()
        {
            try
            {
                Dispatcher?.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        isEnabledWatcher?.Dispose();
                    }
                    catch (System.Exception)
                    {
                    }
                });
            }
            catch (System.Exception)
            {
            }
        }
    }
}