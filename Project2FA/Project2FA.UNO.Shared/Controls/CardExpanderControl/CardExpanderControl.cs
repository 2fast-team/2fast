// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Source https://github.com/microsoft/PowerToys/blob/main/src/settings-ui/Settings.UI/Controls/SettingExpander/SettingExpander.cs

using Microsoft.UI.Xaml.Controls;

#if WINDOWS_UWP
using Project2FA.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
#else
using Project2FA.UNO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
#endif

namespace Project2FA.Controls
{
    public partial class CardExpanderControl : Expander
    {
        public CardExpanderControl()
        {
            DefaultStyleKey = typeof(Expander);
            this.Style = (Style)App.Current.Resources["CardExpanderStyle"];
            this.RegisterPropertyChangedCallback(Expander.HeaderProperty, OnHeaderChanged);
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyProperty dp)
        {
            CardExpanderControl self = (CardExpanderControl)d;
            if (self.Header != null)
            {
                if (self.Header.GetType() == typeof(CardControl))
                {
                    CardControl selfSetting = (CardControl)self.Header;
                    selfSetting.Style = (Style)App.Current.Resources["ExpanderHeaderSettingStyle"];

                    if (!string.IsNullOrEmpty(selfSetting.Header))
                    {
                        AutomationProperties.SetName(self, selfSetting.Header);
                    }
                }
            }
        }
    }
}
