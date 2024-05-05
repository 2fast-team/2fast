using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Project2FA.UNO.MauiControls.Messenger;
public class QRCodeScannedMessage : ValueChangedMessage<string>
{
    public QRCodeScannedMessage(string value) : base(value)
    {
    }
}
