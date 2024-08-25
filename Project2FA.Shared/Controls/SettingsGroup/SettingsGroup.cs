﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
#if WINDOWS_UWP
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#endif

namespace Project2FA.Controls
{
    /// <summary>
    /// Represents a control that can contain multiple settings (or other) controls
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplatePart(Name = PartDescriptionPresenter, Type = typeof(ContentPresenter))]
    public partial class SettingsGroup : ItemsControl
    {
        private const string PartDescriptionPresenter = "DescriptionPresenter";
        private ContentPresenter _descriptionPresenter;
        private SettingsGroup _settingsGroup;

        public SettingsGroup()
        {
            DefaultStyleKey = typeof(SettingsGroup);
        }

        [Localizable(true)]
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(string),
            typeof(SettingsGroup),
            new PropertyMetadata(default(string)));

        [Localizable(true)]
        public object Description
        {
            get => (object)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description",
            typeof(object),
            typeof(SettingsGroup),
            new PropertyMetadata(null, OnDescriptionChanged));

        protected override void OnApplyTemplate()
        {
            IsEnabledChanged -= SettingsGroup_IsEnabledChanged;
            _settingsGroup = (SettingsGroup)this;
            _descriptionPresenter = (ContentPresenter)_settingsGroup.GetTemplateChild(PartDescriptionPresenter);
            SetEnabledState();
            IsEnabledChanged += SettingsGroup_IsEnabledChanged;
            base.OnApplyTemplate();
        }

        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingsGroup)d).Update();
        }

        private void SettingsGroup_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        private void Update()
        {
            if (_settingsGroup == null)
            {
                return;
            }

            if (_settingsGroup.Description == null)
            {
                _settingsGroup._descriptionPresenter.Visibility = Visibility.Collapsed;
            }
            else
            {
                _settingsGroup._descriptionPresenter.Visibility = Visibility.Visible;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SettingsGroupAutomationPeer(this);
        }
    }
}
