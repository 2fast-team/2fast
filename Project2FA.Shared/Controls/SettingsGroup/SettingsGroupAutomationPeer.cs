using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.UI.Xaml.Automation.Peers;
#else
using Microsoft.UI.Xaml.Automation.Peers;
#endif

namespace Project2FA.Controls
{
    public class SettingsGroupAutomationPeer : FrameworkElementAutomationPeer
    {
        public SettingsGroupAutomationPeer(SettingsGroup owner)
            : base(owner)
        {
        }

        protected override string GetNameCore()
        {
            var selectedSettingsGroup = (SettingsGroup)Owner;
            return selectedSettingsGroup.Header;
        }
    }
}
