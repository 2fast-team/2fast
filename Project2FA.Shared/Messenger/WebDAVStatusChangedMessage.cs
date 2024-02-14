using CommunityToolkit.Mvvm.Messaging.Messages;
using Project2FA.Repository.Models.Enums;

namespace Project2FA.Core.Messenger
{
    public class WebDAVStatusChangedMessage : ValueChangedMessage<WebDAVStatus>
    {
        public WebDAVStatusChangedMessage(WebDAVStatus value) : base(value)
        {
        }
    }
}
