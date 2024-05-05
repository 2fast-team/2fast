using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Project2FA.UNO.MauiControls.Messenger;
using ZXing.Net.Maui.Controls;

namespace Project2FA.UNO.MauiControls.ViewModel;
public class EmbeddedControlViewModel : ObservableRecipient
{
    private string _parameter = string.Empty;
    private CameraBarcodeReaderView _cameraReader;
    private bool _disconnectHandler;

    public EmbeddedControlViewModel()
    {
        Messenger.Register<EmbeddedControlViewModel, ControlDisposeMessage>(this, (r, m) => r.DisconnectHandler = m.Value);
    }

    public string Parameter
    {
        get => _parameter;
        set
        {
            if (SetProperty(ref _parameter, value) && !string.IsNullOrWhiteSpace(value))
            {
                SendParameterMessage();
                if (_parameter.Contains("otpauth"))
                {
                    _cameraReader.Handler.DisconnectHandler();
                }
            }
        }
    }

    public CameraBarcodeReaderView CameraReader
    {
        get => _cameraReader;
        set => _cameraReader = value;
    }
    public bool DisconnectHandler 
    { 
        get => _disconnectHandler; 
        set
        {
            if(SetProperty(ref _disconnectHandler, value) && value == true)
            {
                _cameraReader?.Handler.DisconnectHandler();
            }
        }
            
    }

    public void SendParameterMessage()
    {
        Messenger.Send(new QRCodeScannedMessage(Parameter));
    }
}

