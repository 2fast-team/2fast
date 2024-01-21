using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Project2FA.Core.Messenger
{
    public class CategoriesChangedMessage : ValueChangedMessage<bool>
    {
        public CategoriesChangedMessage(bool status) : base(status)
        {

        }
    }
}
