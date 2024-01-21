using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Project2FA.Core.Messenger
{
    public class FilteringChangedMessage : ValueChangedMessage<bool>
    {
        public FilteringChangedMessage(bool status) : base(status)
        {

        }
    }
}
