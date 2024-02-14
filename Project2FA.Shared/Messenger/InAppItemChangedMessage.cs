#if WINDOWS_UWP
using CommunityToolkit.Mvvm.Messaging.Messages;
using Project2FA.Repository.Models;

namespace Project2FA.Core.Messenger
{
    public class InAppItemChangedMessage : ValueChangedMessage<InAppPaymentItemModel>
    {
        public InAppItemChangedMessage(InAppPaymentItemModel model) : base(model)
        {

        }
    }
}
#endif