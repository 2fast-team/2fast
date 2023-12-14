using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Project2FA.UNO.MauiControls;
public class EmbeddedControlViewModel : ObservableRecipient
{
    private string _parameter = string.Empty;

    public string Parameter
    {
        get => _parameter;
        set
        {
            if(SetProperty(ref _parameter, value) && !string.IsNullOrWhiteSpace(value))
            {
                SendParameterMessage();
            }
        }
    }

    public void SendParameterMessage()
    {
        Messenger.Send(new QRCodeScannedMessage(Parameter));
    }
}

