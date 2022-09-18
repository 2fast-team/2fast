using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Project2FA.Core.Messenger
{
    public class PasswordStatusChangedMessage : ValueChangedMessage<(bool, string)>
    {
        public PasswordStatusChangedMessage(bool invalid, string hash) : base((invalid, hash))
        {

        }
    }
}
