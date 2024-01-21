using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Core.Messenger
{
    public class IsScreenCaptureEnabledChangedMessage : ValueChangedMessage<bool>
    {
        public IsScreenCaptureEnabledChangedMessage(bool status) : base(status)
        {

        }
    }
}
