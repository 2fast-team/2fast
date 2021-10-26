// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Controls
{
    /// <summary>
    /// Represents a control that can contain multiple settings (or other) controls
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    public partial class CardGroupControl : ItemsControl
    {
        public CardGroupControl()
        {
            DefaultStyleKey = typeof(CardGroupControl);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(string),
            typeof(CardGroupControl),
            new PropertyMetadata(default(string)));

        [Localizable(true)]
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            IsEnabledChanged -= SettingsGroup_IsEnabledChanged;
            SetEnabledState();
            IsEnabledChanged += SettingsGroup_IsEnabledChanged;
            base.OnApplyTemplate();
        }

        private void SettingsGroup_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CardGroupAutomationPeer(this);
        }
    }
}
