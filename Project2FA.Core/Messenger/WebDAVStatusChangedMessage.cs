using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using Project2FA.Repository.Models.Enums;

namespace Project2FA.Core.Messenger
{
    internal class WebDAVStatusChangedMessage : ValueChangedMessage<WebDAVStatus>
    {
        public WebDAVStatusChangedMessage(WebDAVStatus value) : base(value)
        {
        }
    }
}
