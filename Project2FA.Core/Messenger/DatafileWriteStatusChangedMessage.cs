using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Core.Messenger
{
    public sealed class DatafileWriteStatusChangedMessage : ValueChangedMessage<bool>
    {
        public DatafileWriteStatusChangedMessage(bool status) : base(status)
        {

        }
    }
}
