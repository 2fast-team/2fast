// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS_UWP
using Windows.UI.Xaml.Automation.Peers;
#else
using Microsoft.UI.Xaml.Automation.Peers;
#endif

namespace Project2FA.Controls
{
    public class CardGroupAutomationPeer : FrameworkElementAutomationPeer
    {
        public CardGroupAutomationPeer(CardGroupControl owner)
            : base(owner)
        {
        }

        protected override string GetNameCore()
        {
            var selectedSettingsGroup = (CardGroupControl)Owner;
            return selectedSettingsGroup.Header;
        }
    }
}
