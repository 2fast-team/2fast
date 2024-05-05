using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Project2FA.UNO.MauiControls;
public class ControlDisposeMessage : ValueChangedMessage<bool>
{
    public ControlDisposeMessage(bool value) : base(value)
    {
    }
}
